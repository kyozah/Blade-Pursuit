using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    public LobbyUI lobbyUI;
    public NetworkPrefabRef playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

    void Awake()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        DontDestroyOnLoad(gameObject);
    }

    // ── Lobby ──────────────────────────────────────────────
    public async void JoinLobby()
    {
        Debug.Log("[NET] Đang kết nối lobby...");
        var res = await _runner.JoinSessionLobby(SessionLobby.Custom, "MainLobby");
        if (!res.Ok) Debug.LogError("[NET] Không vào được lobby: " + res.ShutdownReason);
        else Debug.Log("[NET] Vào lobby thành công!");
    }

    // ── Tạo phòng ──────────────────────────────────────────
    public async void CreateRoom(string roomName)
    {
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>()
                        ?? gameObject.AddComponent<NetworkSceneManagerDefault>();

        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            SceneManager = sceneManager,
            IsVisible = true,
            IsOpen = true,
            CustomLobbyName = "MainLobby",
            // Không load scene ở đây — giữ Host trong lobby
            // Scene sẽ được load khi nhấn nút "Bắt đầu"
        });
        if (!res.Ok) Debug.LogError("[NET] Không tạo được phòng: " + res.ShutdownReason);
        else Debug.Log("[NET] Tạo phòng thành công: " + roomName);
    }

    // ── Bắt đầu game (Host gọi khi muốn start) ────────────
    // ── Bắt đầu game (Host gọi khi nhấn nút) ────────────
    public void StartGame()
    {
        // Kiểm tra IsServer vì chỉ Host mới có quyền đổi Scene cho cả phòng
        if (_runner != null && _runner.IsServer)
        {
            Debug.Log("[NET] Host yêu cầu tất cả chuyển sang scene Gameplay...");
            
            // QUAN TRỌNG: Đảm bảo Scene Gameplay ở Index 1 trong Build Settings
            _runner.LoadScene(SceneRef.FromIndex(1)); 
        }
    }

    // ── Player Joined (Lúc này vẫn đang ở Lobby) ────────
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // KHÔNG SPAWN Ở ĐÂY. 
        // Nếu spawn ở đây, nhân vật sẽ xuất hiện ở Scene Lobby (Index 0).
        Debug.Log($"[NET] Player {player} đã vào phòng chờ.");
    }

    // ── Khi Scene Gameplay đã load xong trên máy ─────────
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // Fix lỗi gạch đỏ: runner.CurrentScene không tồn tại trực tiếp trong một số bản Fusion
        Debug.Log("[NET] Một Scene mới đã được load xong trên máy này!");

        // Chỉ Host/Server mới có quyền thực hiện lệnh Spawn để đồng bộ cho tất cả
        if (runner.IsServer) 
        {
            // runner.ActivePlayers chứa danh sách tất cả người chơi đã kết nối thành công
            foreach (var player in runner.ActivePlayers)
            {
                // Kiểm tra tránh spawn trùng nếu scene load lại hoặc có người vào sau
                if (!_spawnedPlayers.ContainsKey(player))
                {
                    Debug.Log($"[NET] Đang Spawn nhân vật cho Player: {player}");
                    
                    // Đặt Y = 1 để nhân vật rơi nhẹ xuống sàn, tránh bị kẹt (glitch) dưới đất
                    Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-4f, 4f), 1f, UnityEngine.Random.Range(-4f, 4f));
                    
                    // Spawn prefab và gán quyền điều khiển (Input Authority) cho đúng người chơi
                    NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
                    
                    // Lưu vào Dictionary để quản lý (ví dụ: để xóa khi họ thoát)
                    _spawnedPlayers.Add(player, playerObject);
                }
            }
        }
    }
    // ── Vào phòng ──────────────────────────────────────────
    public async void JoinRoom(string roomName)
    {
        Debug.Log("[NET] Đang vào phòng: " + roomName);
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>()
                        ?? gameObject.AddComponent<NetworkSceneManagerDefault>();

        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            SceneManager = sceneManager,
            CustomLobbyName = "MainLobby",
        });
        if (!res.Ok) Debug.LogError("[NET] Không vào được phòng: " + res.ShutdownReason);
        else Debug.Log("[NET] Vào phòng thành công!");
    }

    // ── Spawn / Despawn ────────────────────────────────────
    

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        if (_spawnedPlayers.TryGetValue(player, out var obj))
        {
            runner.Despawn(obj);
            _spawnedPlayers.Remove(player);
        }
    }

    // ── Input ──────────────────────────────────────────────
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        var move = Vector2.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;
        data.move = move.normalized;
        input.Set(data);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("[NET] OnSessionListUpdated: " + sessionList.Count + " phòng");
        foreach (var s in sessionList) Debug.Log("[NET] Phòng: " + s.Name);
        lobbyUI?.BuildRoomList(sessionList);
    }

    // ── Callbacks ──────────────────────────────────────────
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { Debug.Log("[NET] Shutdown: " + reason); }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectedToServer(NetworkRunner runner, NetAddress remoteAddress) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    { 
        Debug.LogError($"[NET] Disconnected from server. Reason: {reason}");
        // Handle disconnection: e.g., show UI, attempt reconnect, etc.
        // For example, return to lobby or main menu
        lobbyUI?.ShowDisconnectedMessage(reason.ToString());
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("[NET] Đang load scene..."); }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
}
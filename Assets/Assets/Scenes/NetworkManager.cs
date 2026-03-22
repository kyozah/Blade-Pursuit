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
        // Chỉ tạo 1 lần, không bao giờ destroy
        if (FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
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
        Debug.Log("[NET] Tạo phòng: " + roomName);
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
        });
        if (!res.Ok) Debug.LogError("[NET] Không tạo được phòng: " + res.ShutdownReason);
        else Debug.Log("[NET] Tạo phòng thành công: " + roomName);
    }

    // ── Bắt đầu game ──────────────────────────────────────
    public void StartGame()
    {
        if (_runner != null && _runner.IsServer)
        {
            Debug.Log("[NET] Host load scene Gameplay...");
            _runner.LoadScene(SceneRef.FromIndex(1));
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
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        var pos = new Vector3(UnityEngine.Random.Range(-4f, 4f), 0, UnityEngine.Random.Range(-4f, 4f));
        var obj = runner.Spawn(playerPrefab, pos, Quaternion.identity, player);
        _spawnedPlayers[player] = obj;
    }

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
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { Debug.Log("[NET] Scene load xong!"); }
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
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
    public NetworkPrefabRef networkChatManagerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
    private NetworkObject _spawnedChatManager;

    private string localPlayerName = "";

    void Awake()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        DontDestroyOnLoad(gameObject);
        localPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
    }

    public void SetLocalPlayerName(string name)
    {
        localPlayerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    public string GetLocalPlayerName() => localPlayerName;

    public async void JoinLobby()
    {
        Debug.Log("[NET] JoinLobby");
        var res = await _runner.JoinSessionLobby(SessionLobby.Custom, "MainLobby");
        if (!res.Ok) Debug.LogError("[NET] Không vào lobby: " + res.ShutdownReason);
    }

    public async void CreateRoom(string roomName)
    {
        Debug.Log($"[NET] CreateRoom: {roomName}");
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>();
        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            SceneManager = sceneManager,
            IsVisible = true,
            IsOpen = true,
            CustomLobbyName = "MainLobby",
        });
        if (!res.Ok)
        {
            Debug.LogError("[NET] Tạo phòng thất bại: " + res.ShutdownReason);
        }
        else
        {
            Debug.Log("[NET] Tạo phòng thành công: " + roomName);
            // Thông báo cho UI đã vào phòng
            if (lobbyUI != null) lobbyUI.NotifyEnteredRoom();
        }
    }

    public void StartGame()
    {
        if (_runner != null && _runner.IsServer)
        {
            Debug.Log("[NET] StartGame");
            lobbyUI?.HideLobby();
            foreach (var obj in _spawnedPlayers.Values)
            {
                var sync = obj.GetComponent<NetworkPlayerSync>();
                if (sync != null) sync.RpcHideLobbyOnClients();
            }
        }
    }

    public async void JoinRoom(string roomName)
    {
        Debug.Log($"[NET] JoinRoom: {roomName}");
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>();
        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            SceneManager = sceneManager,
            CustomLobbyName = "MainLobby",
        });
        if (!res.Ok)
        {
            Debug.LogError("[NET] Vào phòng thất bại: " + res.ShutdownReason);
        }
        else
        {
            Debug.Log("[NET] Vào phòng thành công!");
            if (lobbyUI != null) lobbyUI.NotifyEnteredRoom();
        }
    }

    // INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[NET] OnPlayerJoined: {player.PlayerId}");
        if (!runner.IsServer) return;
        EnsureNetworkChatManagerSpawned(runner);
        var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Vector3 spawnPos = spawnPoints.Length > 0 ? spawnPoints[_spawnedPlayers.Count % spawnPoints.Length].transform.position : new Vector3(0, 1, 0);
        Quaternion spawnRot = spawnPoints.Length > 0 ? spawnPoints[_spawnedPlayers.Count % spawnPoints.Length].transform.rotation : Quaternion.identity;
        var obj = runner.Spawn(playerPrefab, spawnPos, spawnRot, player);
        _spawnedPlayers[player] = obj;
        // Nếu là local player (host) thì cũng thông báo lại (đề phòng)
        if (player == runner.LocalPlayer && lobbyUI != null)
            lobbyUI.NotifyEnteredRoom();
    }

    private void EnsureNetworkChatManagerSpawned(NetworkRunner runner)
    {
        if (!runner.IsServer) return;
        if (_spawnedChatManager != null) return;
        var existing = FindFirstObjectByType<NetworkChatManager>();
        if (existing != null && existing.Object != null)
        {
            _spawnedChatManager = existing.Object;
            return;
        }
        if (networkChatManagerPrefab.Equals(default(NetworkPrefabRef))) return;
        _spawnedChatManager = runner.Spawn(networkChatManagerPrefab, Vector3.zero, Quaternion.identity);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[NET] OnPlayerLeft: {player.PlayerId}");
        if (!runner.IsServer) return;
        if (_spawnedPlayers.TryGetValue(player, out var obj))
        {
            runner.Despawn(obj);
            _spawnedPlayers.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) move.y += 1;
            if (Input.GetKey(KeyCode.S)) move.y -= 1;
            if (Input.GetKey(KeyCode.A)) move.x -= 1;
            if (Input.GetKey(KeyCode.D)) move.x += 1;
            data.move = move.normalized;
            data.sprint = Input.GetKey(KeyCode.LeftShift);
        }
        var cam = FindFirstObjectByType<ThirdPersonCamera>();
        data.cameraYaw = cam != null ? cam.GetCameraYaw() : 0f;
        data.buttons.Set(0, Input.GetMouseButton(0));
        data.buttons.Set(1, Input.GetKey(KeyCode.Space));
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        Debug.Log("[NET] Shutdown: " + reason);
        if (lobbyUI != null) lobbyUI.NotifyLeftRoom();
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[NET] ConnectedToServer");
        if (lobbyUI != null) lobbyUI.NotifyEnteredRoom();
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("[NET] Disconnected: " + reason);
        if (lobbyUI != null) lobbyUI.ShowDisconnectedMessage(reason.ToString());
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError("[NET] ConnectFailed: " + reason);
        if (lobbyUI != null) lobbyUI.NotifyLeftRoom();
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("[NET] SessionListUpdated: " + sessionList.Count);
        lobbyUI?.BuildRoomList(sessionList);
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
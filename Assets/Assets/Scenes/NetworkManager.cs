using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        DontDestroyOnLoad(gameObject);
    }

    // ── Lobby ──────────────────────────────────────────────
    public async void JoinLobby()
    {
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
        if (!res.Ok) Debug.LogError("Không vào được lobby: " + res.ShutdownReason);
    }

    // ── Tạo phòng ──────────────────────────────────────────
    public async void CreateRoom(string roomName)
    {
        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(1),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        if (!res.Ok) Debug.LogError("Không tạo được phòng: " + res.ShutdownReason);
    }

    // ── Vào phòng ──────────────────────────────────────────
    public async void JoinRoom(string roomName)
    {
        var res = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
        });
        if (!res.Ok) Debug.LogError("Không vào được phòng: " + res.ShutdownReason);
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
        lobbyUI?.BuildRoomList(sessionList);
    }

    // ── Bỏ trống các callback không cần ───────────────────
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
}
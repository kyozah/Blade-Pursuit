using Fusion;
using UnityEngine;

// Gán script này vào PlayerPrefab (đã có NetworkObject)
public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    public LobbyUI lobbyUI;
    
    // Trạng thái bắt đầu game được replicate cho toàn bộ peer.
    // Late joiner cũng sẽ nhận đúng state hiện tại.
    [Networked]
    private NetworkBool IsGameStarted { get; set; }
    private bool _lobbyHiddenLocal;

    public override void Spawned()
    {
        Instance = this;
        EnsureLobbyUI();

        // Nếu peer vào trễ sau khi game đã start thì vẫn ẩn lobby đúng.
        if (IsGameStarted)
            HideLobbyLocal();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    // Host gọi hàm này để ẩn lobby cho tất cả
    public void TriggerStartGame()
    {
        if (!HasStateAuthority)
            return;

        if (IsGameStarted)
            return;

        IsGameStarted = true;
    }

    public override void Render()
    {
        // Dùng polling nhẹ trong Render để tương thích nhiều version Fusion
        // mà vẫn giữ đồng bộ state qua Networked property.
        if (IsGameStarted && !_lobbyHiddenLocal)
            HideLobbyLocal();
    }

    private void HideLobbyLocal()
    {
        EnsureLobbyUI();
        lobbyUI?.HideLobby();
        _lobbyHiddenLocal = true;
    }

    private void EnsureLobbyUI()
    {
        if (lobbyUI == null)
            lobbyUI = FindFirstObjectByType<LobbyUI>(FindObjectsInactive.Include);
    }
}
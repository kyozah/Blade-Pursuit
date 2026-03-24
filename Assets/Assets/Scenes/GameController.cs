using Fusion;
using UnityEngine;

// Gán script này vào PlayerPrefab (đã có NetworkObject)
public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    public LobbyUI lobbyUI;

    public override void Spawned()
    {
        // Chỉ giữ instance của State Authority (Host)
        if (HasStateAuthority)
            Instance = this;
    }

    // Host gọi hàm này để ẩn lobby cho tất cả
    public void TriggerStartGame()
    {
        if (HasStateAuthority)
            RpcHideLobby();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcHideLobby()
    {
        lobbyUI?.HideLobby();
    }
}
using Fusion;
using UnityEngine;

public class NetworkPlayerSync : NetworkBehaviour
{
    private ThirdPersonController _controller;
    private PlayerHealth _health;
    private AttackComboController _attack;
    private RollController _roll;

    // ✅ Reference đến LobbyUI để ẩn khi nhận RPC
    private LobbyUI _lobbyUI;

    void Awake()
    {
        _controller = GetComponent<ThirdPersonController>();
        // Set trước khi Start() của ThirdPersonController chạy
        if (_controller != null)
            _controller.isNetworkControlled = true;
    }

    public override void Spawned()
    {
        _controller = GetComponent<ThirdPersonController>();
        _health     = GetComponent<PlayerHealth>();
        _attack     = GetComponent<AttackComboController>();
        _roll       = GetComponent<RollController>();
        _lobbyUI    = FindFirstObjectByType<LobbyUI>();

        if (HasInputAuthority)
        {
            _controller.isNetworkControlled = true;
            _controller.enabled = true;
            _controller.EnableInput();

            if (_attack != null) _attack.enabled = true;
            if (_roll   != null) _roll.enabled   = true;

            var camera = FindFirstObjectByType<ThirdPersonCamera>();
            if (camera != null)
                camera.SetTarget(transform);
            else
                Debug.LogWarning("[NET] Không tìm thấy ThirdPersonCamera!");

            var healthBar = FindFirstObjectByType<PlayerHealthBar>();
            if (healthBar != null)
                healthBar.SetTarget(_health);

            Debug.Log("[NET] Local player spawned: " + gameObject.name);
        }
        else
        {
            _controller.isNetworkControlled = true;
            _controller.enabled = false;
            _controller.DisableInput();

            if (_attack != null) _attack.enabled = false;
            if (_roll   != null) _roll.enabled   = false;

            Debug.Log("[NET] Remote player spawned: " + gameObject.name);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (_controller == null || !_controller.enabled) return;

        if (GetInput(out NetworkInputData input))
        {
            // ✅ Chỉ set input — ThirdPersonController.Update() sẽ không đọc input riêng
            _controller.SetNetworkInput(input.move, input.sprint);
        }
    }

    // ✅ Host gọi để ẩn lobby trên tất cả client
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcHideLobbyOnClients()
    {
        Debug.Log("[NET] RPC HideLobby nhận được!");
        if (_lobbyUI == null)
            _lobbyUI = FindFirstObjectByType<LobbyUI>();
        _lobbyUI?.HideLobby();
    }
}
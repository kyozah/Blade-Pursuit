using Fusion;
using UnityEngine;
using TMPro;

public class NetworkPlayerSync : NetworkBehaviour
{
    // Networked properties để đồng bộ
    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public Vector3 NetworkRotation { get; set; }
    [Networked] public float NetworkAnimSpeed { get; set; }
    [Networked] public NetworkBool NetworkIsMoving { get; set; }
    [Networked] public NetworkBool NetworkIsAttacking { get; set; }
    [Networked] public int NetworkAttackIndex { get; set; }
    [Networked] public NetworkBool NetworkIsRolling { get; set; }
    
    // Tên người chơi đồng bộ qua mạng
    [Networked] public NetworkString<_16> NetworkPlayerName { get; set; }

    // Components
    private ThirdPersonController _controller;
    private CharacterController _cc;
    private PlayerHealth _health;
    private AttackComboController _attack;
    private RollController _roll;
    private LobbyUI _lobbyUI;
    private PlayerNameTag _nameTag;
    private ThirdPersonCamera _camera;
    private NetworkButtons _previousButtons;

    void Awake()
    {
        _controller = GetComponent<ThirdPersonController>();
        if (_controller != null)
            _controller.isNetworkControlled = true;
    }

    public override void Spawned()
    {
        Debug.Log($"[NetworkPlayerSync] Spawned - HasInputAuthority: {HasInputAuthority}, HasStateAuthority: {HasStateAuthority}");
        
        // Lấy các components
        _controller = GetComponent<ThirdPersonController>();
        _cc = GetComponent<CharacterController>();
        _health = GetComponent<PlayerHealth>();
        _attack = GetComponent<AttackComboController>();
        _roll = GetComponent<RollController>();
        _lobbyUI = FindFirstObjectByType<LobbyUI>();
        
        // ✅ LẤY NAME TAG TỪ PREFAB (đã được tạo bằng tay)
        _nameTag = GetComponentInChildren<PlayerNameTag>();
        
        if (_nameTag == null)
        {
            Debug.LogError("[NetworkPlayerSync] PlayerNameTag not found in prefab! Please add it manually.");
        }
        else
        {
            Debug.Log("[NetworkPlayerSync] PlayerNameTag found in prefab");
            _nameTag.followTarget = transform;
        }

        // ===== LOCAL PLAYER (Input Authority) =====
        if (HasInputAuthority)
        {
            Debug.Log("[NetworkPlayerSync] Setting up LOCAL player");
            
            _controller.isNetworkControlled = true;
            _controller.enabled = false;
            _controller.EnableInput();

            if (_attack != null)
            {
                _attack.isNetworkControlled = true;
                _attack.SetLocalInputEnabled(true);
                _attack.enabled = true;
            }
            
            if (_roll != null)
            {
                _roll.SetLocalInputEnabled(true);
                _roll.enabled = true;
            }

            // Gán camera cho local player
            _camera = FindFirstObjectByType<ThirdPersonCamera>();
            if (_camera != null)
                _camera.SetTarget(transform);

            // Gán health bar cho local player
            GameObject hud = GameObject.FindGameObjectWithTag("MainHealthBar");
            if (hud != null)
            {
                var healthBar = hud.GetComponent<PlayerHealthBar>();
                if (healthBar != null)
                    healthBar.SetTarget(_health);
            }
            
            // LẤY TÊN TỪ NETWORK MANAGER VÀ ĐỒNG BỘ
            var networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager != null)
            {
                string playerName = networkManager.GetLocalPlayerName();
                if (string.IsNullOrEmpty(playerName))
                {
                    playerName = PlayerPrefs.GetString("PlayerName", "Player");
                }
                
                Debug.Log($"[NetworkPlayerSync] Local player name: {playerName}");
                SubmitLocalPlayerName(playerName);
                
                if (_nameTag != null)
                {
                    _nameTag.SetPlayerName(playerName);
                    Debug.Log($"[NetworkPlayerSync] Set name tag to: {playerName}");
                }
            }
            else
            {
                Debug.LogWarning("[NetworkPlayerSync] NetworkManager not found!");
            }
            
            // ĐĂNG KÝ TÊN VỚI CHAT MANAGER
            var runner = Runner;
            if (runner != null && runner.LocalPlayer != null)
            {
                var chatManager = FindFirstObjectByType<NetworkChatManager>();
                if (chatManager != null && !string.IsNullOrEmpty(NetworkPlayerName.ToString()))
                {
                    chatManager.RegisterPlayerName(runner.LocalPlayer, NetworkPlayerName.ToString());
                    Debug.Log($"[NetworkPlayerSync] Registered name with ChatManager: {NetworkPlayerName}");
                }
            }

            Debug.Log("[NetworkPlayerSync] Local player setup complete");
        }
        
        // ===== REMOTE PLAYER =====
        else
        {
            Debug.Log("[NetworkPlayerSync] Setting up REMOTE player");
            
            _controller.isNetworkControlled = true;
            _controller.enabled = false;
            _controller.DisableInput();

            if (!HasStateAuthority && _cc != null)
                _cc.enabled = false;

            if (_attack != null)
            {
                _attack.isNetworkControlled = true;
                _attack.SetLocalInputEnabled(false);
                _attack.enabled = HasStateAuthority;
            }

            if (_roll != null)
            {
                _roll.SetLocalInputEnabled(false);
                _roll.enabled = HasStateAuthority;
            }

            Debug.Log("[NetworkPlayerSync] Remote player setup complete");
        }
    }
    
    // CẬP NHẬT TÊN CHO REMOTE PLAYER
    public override void Render()
    {
        if (!HasInputAuthority && _nameTag != null && !string.IsNullOrEmpty(NetworkPlayerName.ToString()))
        {
            string networkName = NetworkPlayerName.ToString();
            string currentName = _nameTag.GetPlayerName();
            
            if (currentName != networkName)
            {
                _nameTag.SetPlayerName(networkName);
                Debug.Log($"[NetworkPlayerSync] Updated remote player name to: {networkName}");
            }
        }
    }

    private void SubmitLocalPlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        if (HasStateAuthority)
        {
            NetworkPlayerName = playerName;
            SyncNameIntoChat(playerName, Object.InputAuthority);
            return;
        }

        RpcSetPlayerName(playerName);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetPlayerName(string playerName, RpcInfo info = default)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        NetworkPlayerName = playerName;
        SyncNameIntoChat(playerName, info.Source);
    }

    private void SyncNameIntoChat(string playerName, PlayerRef playerRef)
    {
        var chatManager = FindFirstObjectByType<NetworkChatManager>();
        if (chatManager == null)
            return;

        chatManager.RegisterPlayerName(playerRef, playerName);
    }

    public override void FixedUpdateNetwork()
    {
        if (_cc == null) return;

        if (GetInput(out NetworkInputData input))
        {
            Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;
            var pressed = input.buttons.GetPressed(_previousButtons);
            _previousButtons = input.buttons;

            if (HasStateAuthority)
            {
                if (pressed.IsSet((int)NetworkInputButtons.Attack))
                    _attack?.TryAttackFromNetwork();

                if (pressed.IsSet((int)NetworkInputButtons.Roll))
                    _roll?.TryRollFromNetwork(input.move, input.cameraYaw);
            }

            if (inputDir.magnitude >= 0.1f)
            {
                float cameraYaw = input.cameraYaw;

                Vector3 camForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
                Vector3 camRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
                Vector3 moveDir = (camForward * input.move.y + camRight * input.move.x).normalized;

                float speed = input.sprint ? _controller.sprintSpeed : _controller.moveSpeed;
                
                if (HasStateAuthority)
                {
                    _cc.Move(moveDir * speed * Runner.DeltaTime);

                    if (moveDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            Quaternion.LookRotation(moveDir),
                            _controller.rotationSpeed * Runner.DeltaTime
                        );
                    }

                    _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime);
                }
            }

            UpdateAnimationFromInput(input);
            
            if (HasStateAuthority)
                UpdateNetworkAnimationSnapshot(input);
        }
        else if (HasStateAuthority)
        {
            UpdateNetworkAnimationSnapshot(default);
        }

        // Chỉ StateAuthority mới được ghi Networked transform.
        // Nếu proxy/client cũng ghi, sẽ gây jitter/lag do state bị "giành" qua lại.
        if (HasStateAuthority)
        {
            NetworkPosition = transform.position;
            NetworkRotation = transform.eulerAngles;
        }
    }

    void Update()
    {
        if (!HasInputAuthority)
        {
            if (Vector3.Distance(transform.position, NetworkPosition) > 0.01f)
            {
                // Dùng Runner.DeltaTime nếu có để mượt theo tick Fusion.
                float dt = Runner != null ? Runner.DeltaTime : Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, NetworkPosition, dt * 10f);
            }
            else
            {
                transform.position = NetworkPosition;
            }
            transform.eulerAngles = NetworkRotation;
            ApplyNetworkAnimationSnapshot();
        }
    }

    public void UpdateAnimationFromInput(NetworkInputData input)
    {
        Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;
        
        if (inputDir.magnitude >= 0.1f)
        {
            float speed = input.sprint ? _controller.sprintSpeed : _controller.moveSpeed;
            UpdateAnimation(speed, true);
        }
        else
        {
            UpdateAnimation(0, false);
        }
    }

    private void UpdateAnimation(float speed, bool isMoving) 
    {
        if (_controller.animator != null) 
        {
            _controller.animator.SetFloat("Speed", speed);
            _controller.animator.SetBool("IsMoving", isMoving);
        }
    }

    private void UpdateNetworkAnimationSnapshot(NetworkInputData input)
    {
        Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;
        bool isMoving = inputDir.magnitude >= 0.1f;
        float speed = isMoving ? (input.sprint ? _controller.sprintSpeed : _controller.moveSpeed) : 0f;

        NetworkAnimSpeed = speed;
        NetworkIsMoving = isMoving;
        NetworkIsAttacking = _attack != null && _attack.IsAttacking();
        NetworkAttackIndex = _attack != null ? _attack.GetCurrentComboIndex() : 0;
        NetworkIsRolling = _roll != null && _roll.IsRolling();
    }

    private void ApplyNetworkAnimationSnapshot()
    {
        if (_controller == null || _controller.animator == null)
            return;

        _controller.animator.SetFloat("Speed", NetworkAnimSpeed);
        _controller.animator.SetBool("IsMoving", NetworkIsMoving);
        _controller.animator.SetBool("isAttacking", NetworkIsAttacking);
        _controller.animator.SetInteger("attackIndex", NetworkAttackIndex);
        _controller.animator.SetBool("isRolling", NetworkIsRolling);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcHideLobbyOnClients()
    {
        Debug.Log("[NET] RPC HideLobby received!");
        if (_lobbyUI == null)
            _lobbyUI = FindFirstObjectByType<LobbyUI>();
        _lobbyUI?.HideLobby();
    }
}
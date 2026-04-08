using Fusion;
using UnityEngine;

public class NetworkPlayerSync : NetworkBehaviour
{
    private ThirdPersonController _controller;
    private CharacterController _cc;
    private PlayerHealth _health;
    private AttackComboController _attack;
    private RollController _roll;
    private LobbyUI _lobbyUI;

    // Cache camera yaw để tính hướng di chuyển
    private ThirdPersonCamera _camera;

    void Awake()
    {
        _controller = GetComponent<ThirdPersonController>();
        if (_controller != null)
            _controller.isNetworkControlled = true;
    }

    public override void Spawned()
    {
        _controller = GetComponent<ThirdPersonController>();
        _cc = GetComponent<CharacterController>();
        _health = GetComponent<PlayerHealth>();
        _attack = GetComponent<AttackComboController>();
        _roll = GetComponent<RollController>();
        _lobbyUI = FindFirstObjectByType<LobbyUI>();

        if (HasInputAuthority)
        {
            _controller.isNetworkControlled = true;
            _controller.enabled = false;
            _controller.EnableInput();

            if (_attack != null)
            {
                _attack.isNetworkControlled = true;
                _attack.enabled = true;
            }
            if (_roll != null) _roll.enabled = true;

            _camera = FindFirstObjectByType<ThirdPersonCamera>();
            if (_camera != null)
                _camera.SetTarget(transform);

            GameObject hud = GameObject.FindGameObjectWithTag("MainHealthBar");
            if (hud != null)
            {
                var healthBar = hud.GetComponent<PlayerHealthBar>();
                if (healthBar != null)
                    healthBar.SetTarget(_health);
            }

            Debug.Log("[NET] Local player spawned: " + gameObject.name);

            // Chỉ Host mới có quyền ẩn lobby, nên gọi RPC từ đây là hợp lý
        }
        else
        {
            _controller.isNetworkControlled = true;
            _controller.enabled = false;
            _controller.DisableInput();

            if (_attack != null) _attack.enabled = false;
            if (_roll != null) _roll.enabled = false;

            Debug.Log("[NET] Remote player spawned: " + gameObject.name);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // BỎ DÒNG: if (!HasInputAuthority) return; 
        // Vì cả Server (State Authority) và Client (Input Authority) đều cần chạy code này.

        if (_cc == null) return;

        // GetInput sẽ trả về True nếu:
        // 1. Đây là máy Client đang điều khiển nhân vật này (Input Authority)
        // 2. Đây là máy Server đang nhận Input từ Client đó (State Authority)
        if (GetInput(out NetworkInputData input))
        {
            // ... (Giữ nguyên logic kiểm tra Attack, Rolling, Impact...)

            Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;

            if (inputDir.magnitude >= 0.1f)
            {
                // LƯU Ý: Ở Server, _camera sẽ là null, nên đoạn cameraYaw cần xử lý an toàn
                float cameraYaw = (_camera != null) ? _camera.GetCameraYaw() : transform.eulerAngles.y;

                Vector3 camForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
                Vector3 camRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
                Vector3 moveDir = (camForward * input.move.y + camRight * input.move.x).normalized;

                float speed = input.sprint ? _controller.sprintSpeed : _controller.moveSpeed;

                // Di chuyển CharacterController (Chạy trên cả 2 phía)
                _cc.Move(moveDir * speed * Runner.DeltaTime);

                if (moveDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(moveDir),
                        _controller.rotationSpeed * Runner.DeltaTime
                    );
                }
                
                // Animator chỉ nên chạy trên máy có quyền hiển thị (thường là tất cả hoặc Proxy)
                UpdateAnimation(speed, true);
            }
            else
            {
                UpdateAnimation(0, false);
            }

            // Gravity - Quan trọng: Phải chạy trên cả Server để vị trí Y đồng bộ
            _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime);
        }
    }

    // Tách hàm Animator để code sạch hơn
    private void UpdateAnimation(float speed, bool isMoving) {
        if (_controller.animator != null) {
            _controller.animator.SetFloat("Speed", speed);
            _controller.animator.SetBool("IsMoving", isMoving);
        }
    }
    
    // public override void FixedUpdateNetwork()
    // {
    //     if (!HasInputAuthority) return;
    //     if (_cc == null) return;

    //     if (GetInput(out NetworkInputData input))
    //     {
    //         bool isAttacking = _attack != null && _attack.IsAttacking();
    //         bool isRolling = _roll != null && _roll.IsRolling();
    //         bool isInImpact = _health != null && _health.IsInImpact();
    //         bool isDead = _health != null && _health.IsDead();

    //         Animator animator = _controller.animator;

    //         if (isAttacking || isRolling || isInImpact || isDead)
    //         {
    //             if (animator != null)
    //             {
    //                 animator.SetFloat("Speed", 0);
    //                 animator.SetBool("IsMoving", false);
    //             }
    //             return;
    //         }

    //         Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;

    //         if (inputDir.magnitude >= 0.1f)
    //         {
    //             float cameraYaw = _camera != null
    //                 ? _camera.GetCameraYaw()
    //                 : transform.eulerAngles.y;

    //             Vector3 camForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
    //             Vector3 camRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
    //             Vector3 moveDir = (camForward * input.move.y + camRight * input.move.x).normalized;

    //             float speed = input.sprint ? _controller.sprintSpeed : _controller.moveSpeed;

    //             // ✅ Gọi trực tiếp CharacterController.Move()
    //             _cc.Move(moveDir * speed * Runner.DeltaTime);

    //             // Xoay nhân vật
    //             if (moveDir != Vector3.zero)
    //             {
    //                 transform.rotation = Quaternion.Slerp(
    //                     transform.rotation,
    //                     Quaternion.LookRotation(moveDir),
    //                     _controller.rotationSpeed * Runner.DeltaTime
    //                 );
    //             }

    //             if (animator != null)
    //             {
    //                 animator.SetFloat("Speed", speed);
    //                 animator.SetBool("IsMoving", true);
    //             }
    //         }
    //         else
    //         {
    //             if (animator != null)
    //             {
    //                 animator.SetFloat("Speed", 0);
    //                 animator.SetBool("IsMoving", false);
    //             }
    //         }

    //         // Gravity
    //         _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime * Runner.DeltaTime);
    //     }
    // }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcHideLobbyOnClients()
    {
        Debug.Log("[NET] RPC HideLobby nhận được!");
        if (_lobbyUI == null)
            _lobbyUI = FindFirstObjectByType<LobbyUI>();
        _lobbyUI?.HideLobby();
    }
}
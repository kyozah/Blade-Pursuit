using Fusion;
using UnityEngine;

public class NetworkPlayerSync : NetworkBehaviour
{
    // ✅ Networked properties để đồng bộ vị trí và xoay cho tất cả players
    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public Vector3 NetworkRotation { get; set; }

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

            // ✅ Chỉ disable CharacterController cho proxy players (không phải Server)
            // Server vẫn cần CharacterController để di chuyển player này
            if (!HasStateAuthority && _cc != null)
                _cc.enabled = false;

            if (_attack != null) _attack.enabled = false;
            if (_roll != null) _roll.enabled = false;

            Debug.Log("[NET] Remote player spawned: " + gameObject.name);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_cc == null) return;

        if (GetInput(out NetworkInputData input))
        {
            Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;

            if (inputDir.magnitude >= 0.1f)
            {
                // ✅ Sử dụng cameraYaw từ input thay vì transform.eulerAngles.y
                float cameraYaw = input.cameraYaw;

                Vector3 camForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
                Vector3 camRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
                Vector3 moveDir = (camForward * input.move.y + camRight * input.move.x).normalized;

                float speed = input.sprint ? _controller.sprintSpeed : _controller.moveSpeed;
                
                // ✅ CHỈ HasStateAuthority (Host) di chuyển player
                // Điều này tránh di chuyển 2 lần (host + client)
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

                    // Gravity chỉ host
                    _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime);
                }
            }

            UpdateAnimationFromInput(input);
        }

        // ✅ LUÔN sync network position/rotation
        NetworkPosition = transform.position;
        NetworkRotation = transform.eulerAngles;
    }

    // ✅ Áp dụng network position/rotation cho remote players mỗi frame
    void Update()
    {
        // Nếu không phải input authority (tức là player của remote)
        // Thì cập nhật position/rotation từ network value
        if (!HasInputAuthority)
        {
            // Lerp smooth để tránh teleport
            if (Vector3.Distance(transform.position, NetworkPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, NetworkPosition, Time.deltaTime * 5f);
            }
            else
            {
                transform.position = NetworkPosition;
            }
            transform.eulerAngles = NetworkRotation;
        }
    }

    // ✅ Chạy animation dựa trên input (cho cả local và remote)
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
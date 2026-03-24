using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Attack Settings")]
    public bool autoRotateTowardsCameraOnAttack = true;
    public float attackStartRotationSpeed = 15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("References")]
    public ThirdPersonCamera cameraController;
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool isSprinting;

    // ✅ true = Fusion điều khiển input, false = local Input System
    [HideInInspector] public bool isNetworkControlled = false;

    private AttackComboController attackController;
    private RollController rollController;
    private PlayerHealth playerHealth;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        // Chỉ enable input nếu không phải network controlled
        if (!isNetworkControlled)
            inputActions.Player.Enable();
    }

    void OnDisable()
    {
        if (!isNetworkControlled)
            inputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogError("❌ CharacterController not found!");

        // Chỉ tìm camera nếu không phải network controlled
        // (network controlled sẽ được gán từ NetworkPlayerSync.Spawned)
        if (!isNetworkControlled && cameraController == null)
            cameraController = FindFirstObjectByType<ThirdPersonCamera>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        attackController = GetComponent<AttackComboController>();
        if (attackController != null)
            attackController.OnAttackStart += HandleAttackStart;
        else
            Debug.LogWarning("⚠ AttackComboController not found.");

        rollController = GetComponent<RollController>();
        playerHealth   = GetComponent<PlayerHealth>();
    }

    void OnDestroy()
    {
        if (attackController != null)
            attackController.OnAttackStart -= HandleAttackStart;
    }

    // ✅ Gọi từ NetworkPlayerSync để bật input cho local player
    public void EnableInput()
    {
        inputActions.Player.Enable();
    }

    // ✅ Gọi từ NetworkPlayerSync để tắt input cho remote player
    public void DisableInput()
    {
        inputActions.Player.Disable();
    }

    // ✅ Fusion gọi để truyền input
    public void SetNetworkInput(Vector2 move, bool sprint)
    {
        moveInput   = move;
        isSprinting = sprint;
    }

    void HandleAttackStart()
    {
        if (autoRotateTowardsCameraOnAttack && cameraController != null)
        {
            float cameraYaw       = cameraController.GetCameraYaw();
            Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
            Quaternion targetRot  = Quaternion.LookRotation(cameraForward);
            transform.rotation    = Quaternion.Slerp(
                transform.rotation, targetRot,
                attackStartRotationSpeed * Time.deltaTime * 10f);
        }
    }

    void Update()
    {
        if (!isNetworkControlled)
            ReadInput();

        HandleGroundCheck();
        HandleMovement();
        HandleGravity();
    }

    void ReadInput()
    {
        moveInput   = inputActions.Player.Move.ReadValue<Vector2>();
        isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        bool isAttacking = attackController != null && attackController.IsAttacking();
        bool isRolling   = rollController   != null && rollController.IsRolling();
        bool isInImpact  = playerHealth     != null && playerHealth.IsInImpact();
        bool isDead      = playerHealth     != null && playerHealth.IsDead();

        if (isAttacking || isRolling || isInImpact || isDead)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsMoving", false);
            }
            return;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float cameraYaw = cameraController != null
                ? cameraController.GetCameraYaw()
                : transform.eulerAngles.y;

            Vector3 camForward   = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
            Vector3 camRight     = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
            Vector3 moveDir      = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            float   currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            if (moveDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDir),
                    rotationSpeed * Time.deltaTime);
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", currentSpeed);
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsMoving", false);
            }
        }
    }

    void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

// ── NOTE ─────────────────────────────────────────────────
// Khi isNetworkControlled = true:
//   - Update() KHÔNG đọc input từ Input System
//   - NetworkPlayerSync.FixedUpdateNetwork() gọi SetNetworkInput() mỗi tick Fusion
//   - Update() vẫn chạy HandleMovement() để áp dụng moveInput đã được set
// ─────────────────────────────────────────────────────────
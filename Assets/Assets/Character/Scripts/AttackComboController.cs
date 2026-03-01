using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class AttackComboController : MonoBehaviour
{
    private Animator animator;
    private int currentCombo = 0;
    private bool canReceiveInput = false;
    private bool isExecutingAttack = false;

    [Header("Weapon Hitbox")]
    // ✅ Không cần gán trong Inspector hay tìm trong Start().
    // PlayerWeaponManager sẽ tự gán field này sau khi player equip weapon.
    // Khi chưa có weapon, field này là null và các call Enable/DisableDamage sẽ bị bỏ qua an toàn.
    public WeaponHitbox weaponHitbox;

    [Header("Attack Movement")]
    public float dashDistance = 1.5f;
    public float dashDuration = 0.2f;

    private CharacterController characterController;
    private Rigidbody rb;
    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashTimer;
    private float dashSpeed;

    // Input System
    private PlayerInputActions inputActions;

    // References
    private RollController rollController;
    private PlayerHealth playerHealth;

    public event Action OnAttackStart;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Attack.performed += OnAttackInput;
    }

    void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttackInput;
        inputActions.Player.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        // ✅ KHÔNG tìm WeaponHitbox ở đây nữa.
        // Lý do: lúc Start() chạy, player chưa equip weapon nào.
        // PlayerWeaponManager.SetupWeaponHitbox() sẽ gán weaponHitbox khi player pickup weapon.
        // Nếu player bắt đầu với weapon có sẵn trong tay (pre-equipped),
        // thì PlayerWeaponManager cần gọi PickupWeapon() trong Start() của nó.

        rollController = GetComponent<RollController>();
        if (rollController == null)
        {
            Debug.LogWarning("⚠ RollController not found.");
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("⚠ PlayerHealth not found.");
        }
    }

    void Update()
    {
        HandleDash();
    }

    void OnAttackInput(InputAction.CallbackContext context)
    {
        HandleAttackLogic();
    }

    void HandleAttackLogic()
    {
        // Không thể attack khi đang roll
        if (rollController != null && rollController.IsRolling())
            return;

        // Không thể attack khi bị impact hoặc đã chết
        if (playerHealth != null && (playerHealth.IsInImpact() || playerHealth.IsDead()))
            return;

        // ✅ Không thể attack nếu không có weapon
        if (weaponHitbox == null)
        {
            Debug.LogWarning("⚠ No weapon equipped! Cannot attack.");
            return;
        }

        // Slash 1: Chỉ khi KHÔNG đang attack
        if (currentCombo == 0 && !isExecutingAttack)
        {
            StartCombo(1);
        }
        // Slash 2: Khi đang combo 1 VÀ có thể nhận input
        else if (currentCombo == 1 && canReceiveInput)
        {
            canReceiveInput = false;
            StartCombo(2);
        }
        // Slash 3: Khi đang combo 2 VÀ có thể nhận input
        else if (currentCombo == 2 && canReceiveInput)
        {
            canReceiveInput = false;
            StartCombo(3);
        }
    }

    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer > 0)
            {
                Vector3 movement = dashDirection * dashSpeed * Time.deltaTime;

                if (characterController != null)
                    characterController.Move(movement);
                else if (rb != null)
                    rb.MovePosition(rb.position + movement);
                else
                    transform.position += movement;
            }
            else
            {
                isDashing = false;
            }
        }
    }

    void StartCombo(int comboIndex)
    {
        currentCombo = comboIndex;
        canReceiveInput = false;
        isExecutingAttack = true;

        animator.SetBool("isAttacking", true);
        animator.SetInteger("attackIndex", comboIndex);
        Invoke(nameof(ClearAttackIndex), 0.1f);

        OnAttackStart?.Invoke();

        Debug.Log($"⚔️ Started Combo {comboIndex} - MOVEMENT LOCKED");
    }

    void ClearAttackIndex()
    {
        animator.SetInteger("attackIndex", 0);
    }

    // ===== ANIMATION EVENTS =====

    public void EnableNextInput()
    {
        canReceiveInput = true;
        Debug.Log("✅ Can receive next input");
    }

    public void DisableNextInput()
    {
        canReceiveInput = false;

        if (currentCombo != 3)
            ResetCombo();

        Debug.Log("🛑 Input disabled");
    }

    public void FinishCombo()
    {
        Debug.Log("🏁 Combo finished - UNLOCKING MOVEMENT");
        ResetCombo();
    }

    void ResetCombo()
    {
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;

        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);

        Debug.Log("🔓 Movement UNLOCKED");
    }

    // ✅ Force reset khi bị interrupt (impact, stun, etc)
    public void ForceResetCombo()
    {
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;
        isDashing = false;

        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);

        if (weaponHitbox != null)
            weaponHitbox.DisableDamage();

        CancelInvoke(nameof(ClearAttackIndex));

        Debug.Log("⚠️ FORCED COMBO RESET (interrupted)");
    }

    // ===== ANIMATION EVENTS: Weapon Damage =====

    public void EnableWeaponDamage()
    {
        Debug.Log("📣 EnableWeaponDamage()");
        if (weaponHitbox != null)
        {
            weaponHitbox.EnableDamage();
        }
        else
        {
            // ✅ Warning thay vì Error — có thể player chưa equip weapon
            Debug.LogWarning("⚠ EnableWeaponDamage called but no weapon equipped.");
        }
    }

    public void DisableWeaponDamage()
    {
        Debug.Log("📣 DisableWeaponDamage()");
        if (weaponHitbox != null)
            weaponHitbox.DisableDamage();
    }

    public void DashForward()
    {
        dashDirection = transform.forward;
        dashDirection.y = 0;
        dashDirection.Normalize();

        dashSpeed = dashDistance / dashDuration;

        isDashing = true;
        dashTimer = dashDuration;

        Debug.Log($"⚡ Dash forward! Direction: {dashDirection}");
    }

    public bool IsAttacking()
    {
        return isExecutingAttack;
    }
}
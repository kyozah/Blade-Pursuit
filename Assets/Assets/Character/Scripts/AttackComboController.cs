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
    public WeaponHitbox weaponHitbox;

    [Header("Attack Movement")]
    public float dashDistance = 1.5f;
    public float dashDuration = 0.2f;

    // ✅ Set true khi là network player — tắt dash để tránh teleport
    [HideInInspector] public bool isNetworkControlled = false;

    private CharacterController characterController;
    private Rigidbody rb;
    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashTimer;
    private float dashSpeed;

    private PlayerInputActions inputActions;
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
        rollController = GetComponent<RollController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // ✅ Tắt dash khi network controlled — tránh teleport
        if (!isNetworkControlled)
            HandleDash();
    }

    void OnAttackInput(InputAction.CallbackContext context)
    {
        HandleAttackLogic();
    }

    void HandleAttackLogic()
    {
        if (rollController != null && rollController.IsRolling()) return;
        if (playerHealth != null && (playerHealth.IsInImpact() || playerHealth.IsDead())) return;
        if (weaponHitbox == null) { Debug.LogWarning("⚠ No weapon equipped!"); return; }

        if (currentCombo == 0 && !isExecutingAttack)
            StartCombo(1);
        else if (currentCombo == 1 && canReceiveInput)
        { canReceiveInput = false; StartCombo(2); }
        else if (currentCombo == 2 && canReceiveInput)
        { canReceiveInput = false; StartCombo(3); }
    }

    void HandleDash()
    {
        if (!isDashing) return;

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

    void StartCombo(int comboIndex)
    {
        currentCombo = comboIndex;
        canReceiveInput = false;
        isExecutingAttack = true;

        animator.SetBool("isAttacking", true);
        animator.SetInteger("attackIndex", comboIndex);
        Invoke(nameof(ClearAttackIndex), 0.1f);

        OnAttackStart?.Invoke();
        Debug.Log($"⚔️ Started Combo {comboIndex}");
    }

    void ClearAttackIndex()
    {
        animator.SetInteger("attackIndex", 0);
    }

    public void EnableNextInput()
    {
        canReceiveInput = true;
    }

    public void DisableNextInput()
    {
        canReceiveInput = false;
        if (currentCombo != 3) ResetCombo();
    }

    public void FinishCombo()
    {
        ResetCombo();
    }

    void ResetCombo()
    {
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;

        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);
    }

    public void ForceResetCombo()
    {
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;
        isDashing = false;

        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);

        if (weaponHitbox != null) weaponHitbox.DisableDamage();
        CancelInvoke(nameof(ClearAttackIndex));
    }

    public void EnableWeaponDamage()
    {
        if (weaponHitbox != null) weaponHitbox.EnableDamage();
        else Debug.LogWarning("⚠ EnableWeaponDamage: no weapon equipped.");
    }

    public void DisableWeaponDamage()
    {
        if (weaponHitbox != null) weaponHitbox.DisableDamage();
    }

    public void DashForward()
    {
        // ✅ Không dash khi network controlled
        if (isNetworkControlled) return;

        dashDirection = transform.forward;
        dashDirection.y = 0;
        dashDirection.Normalize();
        dashSpeed = dashDistance / dashDuration;
        isDashing = true;
        dashTimer = dashDuration;
    }

    public bool IsAttacking() => isExecutingAttack;
}
using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 5f;
    private float currentHealth;

    [Header("AI Settings")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float retreatDistance = 5f;
    public float moveSpeed = 10f;
    public float attackDelay = 1f; // Thời gian chuẩn bị attack
    public float attackCooldownOverride = -1f; // If >0, this enemy uses its own cooldown seconds

    [Header("Knockback - Velocity Based")]
    [Tooltip("Lực knockback (m/s)")]
    public float knockbackForce = 10f;

    [Tooltip("Lực đẩy lên trên")]
    public float knockbackUpwardForce = 2f;

    [Tooltip("Thời gian knockback kéo dài")]
    public float knockbackDuration = 0.3f;

    [Tooltip("Drag trong lúc knockback")]
    public float knockbackDrag = 8f;

    [Header("Knockback Direction")]
    [Tooltip("Dùng hướng player đang nhìn thay vì hướng từ player đến enemy")]
    public bool usePlayerForwardDirection = true;

    [Header("Debug")]
    public bool showDebugInfo = false;

    [Header("Animation")]
    [Tooltip("Movement uses bool 'IsMoving'. Attack/Hit use triggers 'Attack'/'Hit'. For death prefer bool 'IsDead' (set true when dead).")]
    // Movement parameter is fixed to 'IsMoving' to keep animation mapping simple

    private Rigidbody rb;
    private Animator animator;
    private bool hasIsMoving = false;
    private bool hasIsDead = false;
    private bool isKnockedBack = false;
    private float originalDrag;
    private Coroutine knockbackCoroutine;
    private bool isDead = false; // true after Die() called to stop AI and interactions

    // AI variables
    private EnemyManager manager;
    private Transform player;
    private enum AIState { Idle, Chase, Attack, Retreat }
    private AIState currentState = AIState.Idle;
    private Vector3 retreatPosition;
    private float lastActionTime = -1f;
    private bool isAttacking = false;
    private Vector3 moveTarget;
    private bool shouldMove = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Try to find animator in children if missing
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Debug.LogWarning($"Animator found on child of '{gameObject.name}'. Consider moving Animator to root or keep it on child.");
            }
        }

        if (rb == null)
        {
            Debug.LogError($"❌ Enemy '{gameObject.name}' MISSING RIGIDBODY!");
            return;
        }

        // Ensure animator exists
        if (animator == null)
        {
            Debug.LogWarning($"⚠️ Enemy '{gameObject.name}' has no Animator component. Movement/Attack animations won't be played.");
        }
        else
        {
            // Verify Animator has the bool parameters 'IsMoving' and 'IsDead'
            hasIsMoving = false;
            hasIsDead = false;
            foreach (var p in animator.parameters)
            {
                if (p.type == UnityEngine.AnimatorControllerParameterType.Bool)
                {
                    if (p.name == "IsMoving") hasIsMoving = true;
                    if (p.name == "IsDead") hasIsDead = true;
                    if (hasIsMoving && hasIsDead) break;
                }
            }
            if (showDebugInfo && !hasIsMoving)
            {
                string paramList = string.Join(",", System.Array.ConvertAll(animator.parameters, x => x.name));
                Debug.LogWarning($"Animator on '{gameObject.name}' missing bool param 'IsMoving'. Parameters: {paramList}");
            }
            if (showDebugInfo && !hasIsDead)
            {
                string paramList = string.Join(",", System.Array.ConvertAll(animator.parameters, x => x.name));
                Debug.LogWarning($"Animator on '{gameObject.name}' missing bool param 'IsDead' (recommended for death transitions). Parameters: {paramList}");
            }

            if (animator.runtimeAnimatorController == null && showDebugInfo)
            {
                Debug.LogWarning($"Animator on '{gameObject.name}' has no Controller assigned. Assign one with 'Walk','Attack','Hit','Death' states.");
            }
        }

        // ✅ ENSURE PROPER RIGIDBODY SETUP
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        originalDrag = rb.linearDamping;

        if (showDebugInfo)
        {
            Debug.Log($"🟢 Enemy '{gameObject.name}' initialized");
            Debug.Log($"  Use Player Forward Direction: {usePlayerForwardDirection}");
            Debug.Log($"  Gravity enabled: {rb.useGravity}");
            Debug.Log($"  Is Kinematic: {rb.isKinematic}");
            Debug.Log($"  Animator present: {animator != null}, Has IsMoving: {hasIsMoving}");
        }
    }

    void Update()
    {
        if (isDead) return; // dead enemies do nothing
        if (isKnockedBack || isAttacking) return;

        UpdateAI();
    }

    void FixedUpdate()
    {
        // Đảm bảo Rigidbody không bị sleep
        if (rb != null && isKnockedBack)
        {
            rb.WakeUp();
        }

        // AI Movement
        if (isDead) return; // safety: no movement after death

        if (shouldMove && !isKnockedBack && !isAttacking)
        {
            MoveTowards(moveTarget);
        }
        if (animator != null)
        {
            // Always set the canonical 'IsMoving' bool when moving
            bool movingValue = shouldMove && !isAttacking && !isKnockedBack;
            animator.SetBool("IsMoving", movingValue);

            // Debug: if moving flag is true but animator not playing Walk state, try to force it
            if (showDebugInfo && movingValue)
            {
                // Check current state
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("Walk") && !animator.IsInTransition(0))
                {
                    // If controller contains a state named "Walk", crossfade to it as a fallback
                    int walkHash = Animator.StringToHash("Walk");
                    if (animator.HasState(0, walkHash))
                    {
                        Debug.Log($"Animator for '{gameObject.name}' not in 'Walk' while moving, forcing Walk state.");
                        animator.CrossFade(walkHash, 0.1f);
                    }
                    else
                    {
                        Debug.Log($"Animator for '{gameObject.name}' moving flag true but no 'Walk' state found. Current state: {stateInfo.shortNameHash}");
                    }
                }
            }
        }   
 }

    void UpdateAI()
    {
        if (manager == null || player == null) return;

        if (lastActionTime >= 0f && Time.time - lastActionTime < 3f) return; // Delay 3s after actions (not on spawn)

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Idle:
                if (manager.IsPlayerInZone() && distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chase;
                }
                break;

            case AIState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    if (manager.CanAttack(this))
                    {
                        StartAttack();
                        lastActionTime = Time.time;
                        if (showDebugInfo) Debug.Log($"{gameObject.name} starting attack (distance {distanceToPlayer:F2})");
                    }
                    else
                    {
                        // couldn't attack due to cooldown/other; keep chasing and log
                        moveTarget = player.position;
                        shouldMove = true;
                        if (showDebugInfo || (manager != null && manager.showDebugInfo)) Debug.Log($"{gameObject.name} in attack range ({distanceToPlayer:F2}) but cannot attack yet. Keeping chase.");
                    }
                }
                else
                {
                    moveTarget = player.position;
                    if (!shouldMove)
                    {
                        shouldMove = true;
                        if (showDebugInfo) Debug.Log($"{gameObject.name} chase: moving towards player (distance {distanceToPlayer:F2})");
                    }
                }
                break;

            case AIState.Attack:
                // Attack logic handled in coroutine
                break;

            case AIState.Retreat:
                if (Vector3.Distance(transform.position, retreatPosition) < 1f)
                {
                    currentState = AIState.Idle;
                    lastActionTime = Time.time;
                    shouldMove = false;
                }
                else
                {
                    moveTarget = retreatPosition;
                    shouldMove = true;
                }
                break;
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);
        if (player != null)
        {
            transform.LookAt(player.position);
        }
    }

    void StartAttack()
    {
        currentState = AIState.Attack;
        isAttacking = true;
        manager.StartAttack(this);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (showDebugInfo || (manager != null && manager.showDebugInfo))
            Debug.Log($"{gameObject.name} START_ATTACK at time {Time.time:F2}. attackDelay={attackDelay}, attackDamage={attackDamage}");

        StartCoroutine(PerformAttack());
    }

    protected virtual IEnumerator PerformAttack()
    {
        // Default attack behaviour: wait attackDelay then apply damage immediately
        yield return new WaitForSeconds(attackDelay);

        if (isDead)
        {
            if (showDebugInfo) Debug.Log($"{gameObject.name} aborted attack because dead");
            manager.EndAttack(this);
            yield break;
        }

        // Attack logic: damage player
        if (showDebugInfo) Debug.Log($"{gameObject.name} PERFORMING_ATTACK at time {Time.time:F2}");

        ApplyAttackDamage();

        // After attack, retreat
        retreatPosition = transform.position + (transform.position - player.position).normalized * retreatDistance;
        currentState = AIState.Retreat;
        isAttacking = false;
        manager.EndAttack(this);
    }

    /// <summary>
    /// Applies attack damage to the player if in range. Returns true if damage was applied.
    /// </summary>
    protected bool ApplyAttackDamage()
    {
        if (player == null)
        {
            if (showDebugInfo) Debug.LogWarning($"{gameObject.name} cannot apply attack: player reference is null");
            return false;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (showDebugInfo) Debug.Log($"{gameObject.name} attempting attack at distance {dist:F2}");

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            // Try to find PlayerHealth on player object by tag
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
                if (playerHealth != null && showDebugInfo) Debug.Log($"Found PlayerHealth via tag lookup for {gameObject.name}");
            }
        }

        // Use horizontal distance check (ignore Y) to be more robust
        float horizDist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(player.position.x, 0, player.position.z));
        if (showDebugInfo) Debug.Log($"{gameObject.name} horizontal distance to player: {horizDist:F2}, attackRange: {attackRange:F2}");

        bool hitApplied = false;

        if (playerHealth != null && horizDist <= attackRange + 0.5f)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            Debug.Log($"Enemy attacked player for {attackDamage} damage (direct)");
            hitApplied = true;
        }
        else
        {
            // Fallback: check for player collider within attackRange using OverlapSphere
            Collider[] overlaps = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var col in overlaps)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(attackDamage, transform.position);
                        Debug.Log($"Enemy attacked player for {attackDamage} damage (overlap)");
                        hitApplied = true;
                        break;
                    }
                }
            }
        }

        if (!hitApplied)
        {
            Debug.LogWarning($"{gameObject.name} did not hit player - out of range or no player collider present.");
        }

        return hitApplied;
    }

    /// <summary>
    /// Animation event hook: call this from the attack animation event to apply damage at the precise frame.
    /// </summary>
    public void OnAttackHit()
    {
        if (showDebugInfo || (manager != null && manager.showDebugInfo))
            Debug.Log($"{gameObject.name} OnAttackHit() called by animation at {Time.time:F2}");
        ApplyAttackDamage();
    }

    /// <summary>
    /// Helper for subclasses to finish an attack and transition to Retreat state.
    /// </summary>
    protected void FinishAttackAndRetreat()
    {
        if (player != null)
            retreatPosition = transform.position + (transform.position - player.position).normalized * retreatDistance;
        currentState = AIState.Retreat;
        isAttacking = false;
        if (manager != null) manager.EndAttack(this);
    }

    // ✅ THÊM overload để nhận player forward direction
    public void TakeDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (rb == null)
        {
            Debug.LogError("❌ No Rigidbody!");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"\n💥 TakeDamage: {gameObject.name}");
            Debug.Log($"  Attacker forward: {attackerForward}");
        }

        // Trừ máu
        currentHealth -= damage;

        // Animation
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // ✅✅✅ TÍNH HƯỚNG KNOCKBACK
        Vector3 knockbackDirection;

        if (usePlayerForwardDirection)
        {
            // ✅ DÙNG HƯỚNG PLAYER ĐANG NHÌN (forward)
            knockbackDirection = attackerForward;
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();

            if (showDebugInfo)
            {
                Debug.Log($"  Using FORWARD direction: {knockbackDirection}");
            }
        }
        else
        {
            // ❌ Cách cũ: Từ player đến enemy (không dự đoán được)
            knockbackDirection = (transform.position - attackerPosition).normalized;
            knockbackDirection.y = 0;

            if (knockbackDirection.magnitude < 0.1f)
            {
                knockbackDirection = attackerForward;
            }

            knockbackDirection.Normalize();

            if (showDebugInfo)
            {
                Debug.Log($"  Using POSITION direction: {knockbackDirection}");
            }
        }

        // Apply knockback
        ApplyVelocityKnockback(knockbackDirection);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ✅ Giữ lại overload cũ để tương thích
    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        // Fallback: tính forward từ position
        Vector3 direction = (transform.position - attackerPosition).normalized;
        TakeDamage(damage, attackerPosition, direction);
    }

    void ApplyVelocityKnockback(Vector3 direction)
    {
        if (rb == null) return;

        // Dừng coroutine cũ
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        // Wake up rigidbody
        rb.WakeUp();

        // Use AddForce to allow gravity to work naturally
        Vector3 knockbackForceVector = direction * knockbackForce;
        
        if (showDebugInfo)
        {
            Debug.Log($"⚡ Knockback force: {knockbackForceVector}");
            Debug.Log($"  Direction: {direction}");
            Debug.Log($"  Force: {knockbackForce} m/s");
        }

        // Apply knockback force and upward force separately
        rb.linearVelocity = new Vector3(knockbackForceVector.x, rb.linearVelocity.y, knockbackForceVector.z);
        rb.AddForce(Vector3.up * knockbackUpwardForce, ForceMode.VelocityChange);
        rb.linearDamping = knockbackDrag;

        isKnockedBack = true;

        // Debug
        StartCoroutine(DebugKnockback());

        // Reset state
        knockbackCoroutine = StartCoroutine(ResetKnockbackState());
    }

    IEnumerator DebugKnockback()
    {
        if (!showDebugInfo) yield break;

        Vector3 startPos = transform.position;
        yield return new WaitForSeconds(0.1f);

        float moved = Vector3.Distance(startPos, transform.position);

        if (moved < 0.05f)
        {
            Debug.LogError($"❌ NOT MOVING! Only {moved:F3}m");
        }
        else
        {
            Debug.Log($"✅ Moving! Distance: {moved:F2}m");
        }
    }

    IEnumerator ResetKnockbackState()
    {
        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
        {
            rb.linearDamping = originalDrag;
        }

        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 {gameObject.name} died");

        // Remove from manager list immediately
        if (manager != null)
        {
            manager.RemoveEnemy(this);
        }

        // Play death animation if possible, prefer boolean 'IsDead'
        if (animator != null)
        {
            if (hasIsDead)
            {
                if (showDebugInfo) Debug.Log($"{gameObject.name} setting Animator.IsDead = true");
                animator.SetBool("IsDead", true);
            }
            else
            {
                animator.SetTrigger("Death");
            }
        }

        // Stop ongoing coroutines and knockback
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        StopAllCoroutines();

        // Disable colliders to prevent further hits / interactions
        var cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            c.enabled = false;
        }

        // Freeze rigidbody to avoid physics interactions
        if (rb != null)
        {
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Finally destroy the gameobject after the death animation delay
        Destroy(gameObject, 2f);
    }

    public void SetManager(EnemyManager mgr)
    {
        manager = mgr;
        player = mgr.GetPlayer();

        // Ensure newly spawned enemies can act immediately (no spawn idle)
        lastActionTime = -1f;

        // If player is in the manager zone and within detection, start chasing immediately
        if (manager != null && player != null && manager.IsPlayerInZone())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chase;
                shouldMove = true;
                moveTarget = player.position;
            }
        }
    }
}
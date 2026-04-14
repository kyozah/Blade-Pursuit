using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class Enemy : NetworkBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 5f;

    [Networked] public float CurrentHealth { get; set; }
    [Networked] public NetworkBool IsDeadNetworked { get; set; }

    [Header("AI Settings")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float retreatDistance = 5f;
    public float moveSpeed = 10f;
    public bool invertFacing = false;
    public float rotationSpeed = 720f;
    public float attackDelay = 1f;
    public float attackCooldownOverride = -1f;

    [Header("Knockback - Velocity Based")]
    public float knockbackForce = 10f;
    public float knockbackUpwardForce = 2f;
    public float knockbackDuration = 0.3f;
    public float knockbackDrag = 8f;

    [Header("Knockback Direction")]
    public bool usePlayerForwardDirection = true;

    [Header("Debug")]
    public bool showDebugInfo = false;

    [Header("Animation")]
    public float deathAnimationDuration = 2f;
    public bool useDeathAnimationEvent = false;

    [Header("Pain / Stun")]
    public float damageStunDuration = 2f;
    private float lastDamageTime = -Mathf.Infinity;

    private Rigidbody rb;
    private Animator animator;
    private bool hasIsMoving = false;
    private bool hasIsDead = false;
    private bool isKnockedBack = false;
    private float originalDrag;
    private Coroutine knockbackCoroutine;
    private EnemyAudioSystem audioSystem;

    private EnemyManager manager;
    protected Transform player;
    private enum AIState { Idle, Chase, Attack, Retreat }
    private AIState currentState = AIState.Idle;
    private Vector3 retreatPosition;
    private float lastActionTime = -1f;
    private bool isAttacking = false;
    private Vector3 moveTarget;
    private bool shouldMove = false;

    protected virtual void Start()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHealth = maxHealth;
            IsDeadNetworked = false;
        }

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSystem = GetComponent<EnemyAudioSystem>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError($"❌ Enemy '{gameObject.name}' MISSING RIGIDBODY!");
            return;
        }

        // Client: tắt vật lý để tránh xung đột với NetworkTransform
        if (!Object.HasStateAuthority)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            originalDrag = rb.linearDamping;
        }
    }

    void Update()
    {
        if (!Object.HasStateAuthority) return;
        if (IsDeadNetworked) return;
        if (isKnockedBack) return;

        UpdateAI();
    }

    void FixedUpdate()
    {
        if (!Object.HasStateAuthority) return;
        if (IsDeadNetworked) return;

        if (shouldMove && !isKnockedBack)
        {
            MoveTowards(moveTarget);
        }
        if (animator != null)
        {
            bool movingValue = shouldMove && !isKnockedBack;
            animator.SetBool("IsMoving", movingValue);
        }
    }

    void UpdateAI()
    {
        if (manager == null) return;
        player = ResolveNearestPlayerTarget();
        if (player == null) return;

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
                    if (Time.time - lastDamageTime < damageStunDuration)
                    {
                        moveTarget = player.position;
                        shouldMove = true;
                        break;
                    }

                    if (manager.CanAttack(this))
                    {
                        StartAttack();
                    }
                    else
                    {
                        moveTarget = player.position;
                        shouldMove = true;
                    }
                }
                else
                {
                    moveTarget = player.position;
                    if (!shouldMove)
                    {
                        shouldMove = true;
                    }
                }
                break;

            case AIState.Attack:
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

    private Transform ResolveNearestPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length == 0)
            return null;

        Transform best = null;
        float bestDistanceSqr = float.MaxValue;

        foreach (GameObject playerObj in players)
        {
            if (playerObj == null)
                continue;

            var health = playerObj.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead())
                continue;

            float sqr = (playerObj.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestDistanceSqr)
            {
                bestDistanceSqr = sqr;
                best = playerObj.transform;
            }
        }

        return best;
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        Vector3 direction = dir.normalized;
        if (direction.sqrMagnitude > 0f)
        {
            rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);

            Quaternion targetRot = Quaternion.LookRotation(direction);
            if (invertFacing) targetRot *= Quaternion.Euler(0f, 180f, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    protected virtual void StartAttack()
    {
        FacePlayerInstant();

        currentState = AIState.Attack;
        isAttacking = true;
        manager.StartAttack(this);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (audioSystem != null)
        {
            audioSystem.PlayAttackSound();
        }

        StartCoroutine(PerformAttack());
    }

    protected void FacePlayerInstant()
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        if (invertFacing) targetRot *= Quaternion.Euler(0f, 180f, 0f);
        transform.rotation = targetRot;
    }

    protected virtual IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        if (IsDeadNetworked)
        {
            manager.EndAttack(this);
            yield break;
        }

        ApplyAttackDamage();

        currentState = AIState.Chase;
        isAttacking = false;
        if (player != null)
        {
            moveTarget = player.position;
            shouldMove = true;
        }
        if (manager != null) manager.EndAttack(this);
    }

    protected bool ApplyAttackDamage()
    {
        if (player == null) return false;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        float horizDist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(player.position.x, 0, player.position.z));
        bool hitApplied = false;

        if (playerHealth != null && horizDist <= attackRange + 0.5f)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            hitApplied = true;
        }
        else
        {
            Collider[] overlaps = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var col in overlaps)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(attackDamage, transform.position);
                        hitApplied = true;
                        break;
                    }
                }
            }
        }

        return hitApplied;
    }

    public void OnAttackHit()
    {
        ApplyAttackDamage();
    }

    protected void FinishAttackAndRetreat()
    {
        if (player != null)
            retreatPosition = transform.position + (transform.position - player.position).normalized * retreatDistance;
        currentState = AIState.Retreat;
        isAttacking = false;
        if (manager != null) manager.EndAttack(this);
    }

    protected void FinishAttackAndChase()
    {
        currentState = AIState.Chase;
        isAttacking = false;
        if (player != null)
        {
            moveTarget = player.position;
            shouldMove = true;
        }
        if (manager != null) manager.EndAttack(this);
    }

    // ==================== SỬA LỖI DAMAGE CHO CLIENT ====================
    public void TakeDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (Object.HasStateAuthority)
        {
            // Host trực tiếp gây damage
            ApplyDamageAuthority(damage, attackerPosition, attackerForward);
        }
        else
        {
            // Client gửi RPC yêu cầu Host gây damage
            RpcRequestDamage(damage, attackerPosition, attackerForward);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        ApplyDamageAuthority(damage, attackerPosition, attackerForward);
    }
    // ====================================================================

    public void ApplyDamageAuthority(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (IsDeadNetworked) return;

        if (rb == null)
        {
            Debug.LogError("❌ No Rigidbody!");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"\n💥 TakeDamage: {gameObject.name}");
        }

        CurrentHealth -= damage;
        lastDamageTime = Time.time;

        RpcPlayDamageEffects();

        Vector3 knockbackDirection;

        if (usePlayerForwardDirection)
        {
            knockbackDirection = attackerForward;
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();
        }
        else
        {
            knockbackDirection = (transform.position - attackerPosition).normalized;
            knockbackDirection.y = 0;
            if (knockbackDirection.magnitude < 0.1f)
            {
                knockbackDirection = attackerForward;
            }
            knockbackDirection.Normalize();
        }

        ApplyVelocityKnockback(knockbackDirection);

        if (CurrentHealth <= 0)
        {
            IsDeadNetworked = true;
            Die();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayDamageEffects()
    {
        if (audioSystem != null) audioSystem.PlayDamageSound();
        if (animator != null) animator.SetTrigger("Hit");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayDeathEffects()
    {
        if (audioSystem != null) audioSystem.PlayDeathSound();
        if (animator != null)
        {
            if (hasIsDead)
            {
                try { animator.SetBool("IsDead", true); } catch { }
            }
            else
            {
                animator.SetTrigger("Death");
            }
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        Vector3 direction = (transform.position - attackerPosition).normalized;
        TakeDamage(damage, attackerPosition, direction);
    }

    void ApplyVelocityKnockback(Vector3 direction)
    {
        if (rb == null) return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        rb.WakeUp();

        Vector3 knockbackForceVector = direction * knockbackForce;

        rb.linearVelocity = new Vector3(knockbackForceVector.x, rb.linearVelocity.y, knockbackForceVector.z);
        rb.AddForce(Vector3.up * knockbackUpwardForce, ForceMode.VelocityChange);
        rb.linearDamping = knockbackDrag;

        isKnockedBack = true;

        knockbackCoroutine = StartCoroutine(ResetKnockbackState());
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
        if (IsDeadNetworked == false) return;

        Debug.Log($"💀 {gameObject.name} died");

        RpcPlayDeathEffects();

        if (manager != null)
        {
            manager.RemoveEnemy(this);
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        StopAllCoroutines();

        var cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            c.enabled = false;
        }

        if (rb != null)
        {
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (Object.HasStateAuthority)
        {
            StartCoroutine(DestroyAfterDelay(deathAnimationDuration));
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnDeathAnimationComplete()
    {
        if (Object.HasStateAuthority)
        {
            if (Object != null && Runner != null)
                Runner.Despawn(Object);
            else
                Destroy(gameObject);
        }
    }

    public void SetManager(EnemyManager mgr)
    {
        manager = mgr;
        player = mgr.GetPlayer();

        lastActionTime = -1f;

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

    public bool IsDead
    {
        get { return IsDeadNetworked; }
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }
}
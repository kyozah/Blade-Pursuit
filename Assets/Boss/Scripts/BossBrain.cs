using UnityEngine;
using System.Collections;

public class BossBrain : MonoBehaviour
{
    public Transform player;

    [Header("Ranges")]
    public float detectRange = 12f;
    public float meleeRange = 2.5f;

    [Header("Attack 2")]
    public float chargeMoveTime = 2.5f;
    public float chargeCooldown = 5f;

    [Header("Combat Behavior")]
    public bool enableReposition = false; // Toggle reposition
    public float attackCooldown = 1.0f; // Cooldown giữa các attacks

    public enum State { Idle, Roar, Move, Attack, Cooldown, Dead }
    public enum Phase { Phase1, Phase2 }

    public State currentState = State.Idle;
    public Phase currentPhase = Phase.Phase1;

    bool hasRoaredOnce;
    bool phase2Roared;

    float moveTimer;
    float lastChargeTime = -999f;
    float cooldownTimer;

    BossCombat combat;
    BossMovement movement;
    BossHealth health;
    Rigidbody rb;

    void Awake()
    {
        combat = GetComponent<BossCombat>();
        movement = GetComponent<BossMovement>();
        health = GetComponentInChildren<BossHealth>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (currentState == State.Dead || player == null) return;

        CheckPhase();

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(dist);
                break;
            case State.Move:
                HandleMove(dist);
                break;
            case State.Cooldown:
                HandleCooldown(dist);
                break;
        }
    }

    void CheckPhase()
    {
        if (currentPhase == Phase.Phase1 &&
            health.CurrentHP <= health.MaxHP * 0.5f &&
            !phase2Roared)
        {
            phase2Roared = true;
            currentPhase = Phase.Phase2;
            currentState = State.Roar;

            movement.Lock();
            combat.DoRoar2();
        }
    }

    void HandleIdle(float dist)
    {
        movement.LookAt(player.position);

        if (!hasRoaredOnce && dist <= detectRange)
        {
            hasRoaredOnce = true;
            currentState = State.Roar;
            combat.DoRoar1();
            return;
        }

        if (dist <= detectRange)
            currentState = State.Move;
    }

    void HandleMove(float dist)
    {
        movement.LookAt(player.position);
        moveTimer += Time.deltaTime;

        if (dist <= meleeRange)
        {
            moveTimer = 0;
            StartAttack(true);
            return;
        }

        if (moveTimer >= chargeMoveTime &&
            Time.time >= lastChargeTime + chargeCooldown)
        {
            moveTimer = 0;
            lastChargeTime = Time.time;
            StartAttack(false);
            return;
        }

        MoveByPhase();
    }

    void MoveByPhase()
    {
        if (currentPhase == Phase.Phase1)
            movement.WalkTo(player.position);
        else
            movement.RunTo(player.position);
    }

    void StartAttack(bool close)
    {
        currentState = State.Attack;
        movement.Stop();

        if (close)
        {
            if (currentPhase == Phase.Phase2 && Random.value < 0.4f)
                combat.DoAttack3();
            else
                combat.DoAttack1();
        }
        else
        {
            combat.DoAttack2();
        }
    }

    void HandleCooldown(float dist)
    {
        cooldownTimer -= Time.deltaTime;

        // Nhìn player trong lúc cooldown
        movement.LookAt(player.position);

        if (cooldownTimer <= 0f)
        {
            // Hết cooldown -> quay lại Move
            currentState = State.Move;
            moveTimer = 0;
        }
    }

    public void OnRoarFinished()
    {
        ClearPhysics();
        moveTimer = 0;
        currentState = State.Move;
        movement.Unlock();
    }

    public void OnAttackFinished()
    {
        ClearPhysics();
        moveTimer = 0;
        StartCoroutine(UnlockAfterFrame());
    }

    IEnumerator UnlockAfterFrame()
    {
        yield return new WaitForFixedUpdate();
        ClearPhysics();

        if (enableReposition)
        {
            // Old behavior: lùi lại
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < 4f)
            {
                currentState = State.Cooldown;
                cooldownTimer = attackCooldown;
                movement.MoveAway(player.position);
            }
            else
            {
                currentState = State.Move;
            }
        }
        else
        {
            // New behavior: cooldown tại chỗ rồi tiếp tục tấn công
            currentState = State.Cooldown;
            cooldownTimer = attackCooldown;
        }
    }

    void ClearPhysics()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void OnDie()
    {
        if (currentState == State.Dead) return;

        currentState = State.Dead;
        movement.Lock();
        ClearPhysics();
        combat.DoDie();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
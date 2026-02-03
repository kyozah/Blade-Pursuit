using UnityEngine;

public class ChargeSMB : StateMachineBehaviour
{
    [Header("Charge Settings")]
    public float speed = 8f;
    public float maxChargeDistance = 10f;

    [Header("Collision Detection")]
    public LayerMask obstacleLayer;
    public float raycastDistance = 1.5f;
    public float colliderRadius = 0.6f;

    Rigidbody rb;
    BossMovement movement;
    Transform bossTransform;

    Vector3 chargeDir;
    Vector3 startPos;

    bool isCharging;
    bool hasHitObstacle;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        movement = animator.GetComponentInParent<BossMovement>();
        rb = animator.GetComponentInParent<Rigidbody>();
        bossTransform = rb.transform;

        movement.Lock();

        startPos = rb.position;
        chargeDir = bossTransform.forward;
        chargeDir.y = 0;
        chargeDir.Normalize();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isCharging = false;
        hasHitObstacle = false;
    }

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        float normalizedTime = stateInfo.normalizedTime;

        isCharging = normalizedTime >= 0.2f && normalizedTime <= 0.8f;

        if (!isCharging || hasHitObstacle)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        float traveled = Vector3.Distance(startPos, rb.position);
        if (traveled >= maxChargeDistance || CheckObstacleAhead())
        {
            hasHitObstacle = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 newPos =
            rb.position + chargeDir * speed * Time.fixedDeltaTime;

        rb.MovePosition(newPos);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    bool CheckObstacleAhead()
    {
        Vector3 center = rb.position + Vector3.up;
        float checkDist = speed * Time.fixedDeltaTime + raycastDistance;

        Vector3[] points =
        {
            center,
            center + bossTransform.right * 0.4f,
            center - bossTransform.right * 0.4f,
            center + Vector3.up * 0.5f,
            center - Vector3.up * 0.5f
        };

        foreach (var p in points)
        {
            if (Physics.Raycast(p, chargeDir, checkDist, obstacleLayer))
                return true;
        }

        return Physics.SphereCast(
            center,
            colliderRadius,
            chargeDir,
            out _,
            checkDist,
            obstacleLayer);
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        Vector3 finalPos = rb.position;
        finalPos.y = startPos.y;
        rb.position = finalPos;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        movement.Unlock();
    }
}

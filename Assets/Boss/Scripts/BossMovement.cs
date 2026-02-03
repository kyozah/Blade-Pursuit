using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossMovement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;
    public float rotateSpeed = 8f;

    Rigidbody rb;
    Animator animator;

    Vector3 targetPos;
    float currentSpeed;
    bool hasTarget;
    bool locked;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (locked || !hasTarget)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 dir = targetPos - rb.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            Stop();
            return;
        }

        rb.MovePosition(
            rb.position + dir.normalized * currentSpeed * Time.fixedDeltaTime
        );

        rb.linearVelocity = Vector3.zero;
    }

    void Update()
    {
        if (locked || !hasTarget) return;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    public void WalkTo(Vector3 target)
    {
        SetMove(target, walkSpeed, false);
    }

    public void RunTo(Vector3 target)
    {
        SetMove(target, runSpeed, true);
    }

    void SetMove(Vector3 target, float speed, bool running)
    {
        if (locked) return;

        targetPos = target;
        targetPos.y = transform.position.y;

        currentSpeed = speed;
        hasTarget = true;

        animator.SetBool("IsMoving", true);
        animator.SetBool("IsRunning", running);
    }

    public void Stop()
    {
        hasTarget = false;
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsRunning", false);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Lock()
    {
        locked = true;
        Stop();
    }

    public void Unlock()
    {
        locked = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void MoveAway(Vector3 from)
    {
        if (locked) return;

        Vector3 dir = (transform.position - from).normalized;
        dir.y = 0f;

        SetMove(transform.position + dir * 3f, walkSpeed, false);
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}

using UnityEngine;

public class BossCombat : MonoBehaviour
{
    Animator animator;
    BossBrain brain;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        brain = GetComponent<BossBrain>();
    }

    public void DoRoar1() => animator.SetTrigger("Roar1");
    public void DoRoar2() => animator.SetTrigger("Roar2");
    public void DoAttack1() => animator.SetTrigger("Attack1");
    public void DoAttack2() => animator.SetTrigger("Attack2");
    public void DoAttack3() => animator.SetTrigger("Attack3");

    // ✅ THÊM
    public void DoDie()
    {
        animator.SetTrigger("Die");
    }

    public void AE_RoarEnd() => brain.OnRoarFinished();
    public void AE_AttackEnd() => brain.OnAttackFinished();
}

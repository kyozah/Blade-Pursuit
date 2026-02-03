using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float MaxHP = 1000f;
    public float CurrentHP;

    void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(float dmg)
    {
        CurrentHP -= dmg;

        if (CurrentHP <= 0)
        {
            GetComponentInParent<BossBrain>().OnDie();
        }
    }
}

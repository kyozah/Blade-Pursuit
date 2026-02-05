using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    public string BossName = "Boss";
    // Optional icon/sprite to display instead of text in UI
    public Sprite BossIcon;

    public float MaxHP = 1000f;
    public float CurrentHP;

    // Event: current, max
    public event Action<float, float> OnHealthChanged;
    // Event fired when boss dies (HP reaches 0)
    public event Action OnDied;

    void Awake()
    {
        CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(float dmg)
    {
        CurrentHP -= dmg;
        CurrentHP = Mathf.Clamp(CurrentHP, 0f, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP <= 0f)
        {
            OnDied?.Invoke();
            GetComponentInParent<BossBrain>().OnDie();
        }
    }
}

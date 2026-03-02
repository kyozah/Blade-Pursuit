using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    // (previously used static counter for boss tracking; now handled by VictoryMenuUI)
    // public static int activeBosses = 0; // no longer needed

    public string BossName = "Boss";
    public Sprite BossIcon;

    public float MaxHP = 1000f;
    public float CurrentHP;

    [Header("Death Rewards")]
    [Tooltip("Kéo Chest GameObject trong scene vào đây. Chest sẽ bị ẩn lúc đầu và hiện ra khi boss chết.")]
    public GameObject rewardChest;

    [Tooltip("Delay trước khi rương xuất hiện (giây) — cho animation chết boss kịp chạy xong)")]
    public float chestRevealDelay = 2f;

    // Event: current, max
    public event Action<float, float> OnHealthChanged;
    // Event fired when boss dies
    public event Action OnDied;

    void Awake()
    {
        CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);

        // register boss count (no longer used)
        //activeBosses++;

        // Ẩn rương ngay từ đầu
        if (rewardChest != null)
            rewardChest.SetActive(false);
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

            // previously we used a static counter to notify GameManager when all bosses died.
            // That logic has been removed in favor of VictoryMenuUI tracking an explicit list of bosses.
            // If you still need to notify GameManager here, you can call it directly from VictoryMenuUI.

            if (rewardChest != null)
                StartCoroutine(RevealChest());
        }
    }

    System.Collections.IEnumerator RevealChest()
    {
        yield return new WaitForSeconds(chestRevealDelay);
        rewardChest.SetActive(true);
        Debug.Log($"🎁 Chest revealed at {rewardChest.transform.position}");
    }
}
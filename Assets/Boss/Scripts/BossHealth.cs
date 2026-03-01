using UnityEngine;

public class BossHealth : MonoBehaviour
{
<<<<<<< Updated upstream
    public float MaxHP = 1000f;
    public float CurrentHP;

    void Awake()
    {
        CurrentHP = MaxHP;
=======
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

        // Ẩn rương ngay từ đầu
        if (rewardChest != null)
            rewardChest.SetActive(false);
>>>>>>> Stashed changes
    }

    public void TakeDamage(float dmg)
    {
        CurrentHP -= dmg;

        if (CurrentHP <= 0)
        {
            GetComponentInParent<BossBrain>().OnDie();

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
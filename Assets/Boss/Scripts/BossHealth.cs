using UnityEngine;
using System;
using System.Collections;
using Fusion;

public class BossHealth : MonoBehaviour
{
    public string BossName = "Boss";
    public Sprite BossIcon;
    public float MaxHP = 1000f;
    public float CurrentHP;

    [Header("Death Rewards")]
    public GameObject rewardChest;
    public float chestRevealDelay = 2f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;
    private BossNetworkSync networkSync;

    void Awake()
    {
        networkSync = GetComponentInParent<BossNetworkSync>();
        CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        if (rewardChest != null) rewardChest.SetActive(false);
    }

    public void TakeDamage(float dmg)
    {
        // Nếu không có networkSync, apply directly (single player)
        if (networkSync == null || networkSync.Object == null)
        {
            Debug.Log($"[BossHealth] Single player damage: {dmg}");
            TakeDamageAuthority(dmg);
            return;
        }
        
        // Multiplayer: Always use RequestDamage (đúng Photon Fusion cách)
        // Cho dù host hay client, đều gửi RPC để host xử lý
        Debug.Log($"[BossHealth] Requesting damage via network: {dmg}");
        networkSync.RequestDamage(dmg);
    }

    public void TakeDamageAuthority(float dmg)
    {
        CurrentHP -= dmg;
        CurrentHP = Mathf.Clamp(CurrentHP, 0f, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        Debug.Log($"[BossHealth] {BossName} took {dmg} damage. HP: {CurrentHP}/{MaxHP}");
        if (CurrentHP <= 0f)
        {
            OnDied?.Invoke();
            GetComponentInParent<BossBrain>().OnDie();
            if (rewardChest != null) StartCoroutine(RevealChest());
        }
    }

    public void ApplyNetworkHp(float hp, bool dead)
    {
        CurrentHP = Mathf.Clamp(hp, 0f, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        if (dead) GetComponentInParent<BossBrain>()?.OnDie();
    }

    IEnumerator RevealChest()
    {
        yield return new WaitForSeconds(chestRevealDelay);
        rewardChest.SetActive(true);
        Debug.Log($"🎁 Chest revealed at {rewardChest.transform.position}");
    }
}
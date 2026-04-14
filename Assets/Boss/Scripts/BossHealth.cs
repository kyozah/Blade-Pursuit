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
        var runner = FindFirstObjectByType<NetworkRunner>();
        
        // Nếu có network, gửi qua RPC
        if (runner != null && networkSync != null)
        {
            if (!runner.IsServer)
            {
                // Client gửi request đến host
                Debug.Log($"[BossHealth] Client requesting damage: {dmg}");
                networkSync.RequestDamage(dmg);
            }
            else
            {
                // Host apply directly
                Debug.Log($"[BossHealth] Host applying damage: {dmg}");
                TakeDamageAuthority(dmg);
            }
        }
        else
        {
            // Single player - apply directly
            Debug.Log($"[BossHealth] Single player damage: {dmg}");
            TakeDamageAuthority(dmg);
        }
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
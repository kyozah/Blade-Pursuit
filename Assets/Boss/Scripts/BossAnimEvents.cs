using UnityEngine;

public class BossAnimEvents : MonoBehaviour
{
    BossCombat combat;

    [Header("Hitbox References")]
    public BossWeaponHitbox weaponHitbox;      // Vũ khí (Attack1, Attack3)
    public BossWeaponHitbox shoulderHitbox;    // Vai (Attack2/Charge)

    void Awake()
    {
        combat = GetComponentInParent<BossCombat>();
    }

    // ====== ROAR & ATTACK EVENTS ======

    public void AE_RoarEnd()
    {
        combat.AE_RoarEnd();
    }

    public void AE_AttackEnd()
    {
        combat.AE_AttackEnd();
    }

    // ====== WEAPON HITBOX EVENTS ======

    public void AE_WeaponHitboxOn()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.EnableHitbox();
        }
    }

    public void AE_WeaponHitboxOff()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }
    }

    // ====== SHOULDER HITBOX EVENTS (for Charge) ======

    public void AE_ShoulderHitboxOn()
    {
        if (shoulderHitbox != null)
        {
            shoulderHitbox.EnableHitbox();
        }
    }

    public void AE_ShoulderHitboxOff()
    {
        if (shoulderHitbox != null)
        {
            shoulderHitbox.DisableHitbox();
        }
    }
}
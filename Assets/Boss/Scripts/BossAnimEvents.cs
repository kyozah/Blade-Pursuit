using UnityEngine;
using UnityEngine.Playables; 

public class BossAnimEvents : MonoBehaviour
{
    BossCombat combat;

    [Header("Hitbox References")]
    public BossWeaponHitbox weaponHitbox;
    public BossWeaponHitbox shoulderHitbox;

    [Header("VFX Impact Settings")]
    public GameObject impactEffect; // Kéo cái EarthSlam từ Hierarchy vào đây [cite: 2025-09-22]
    public Transform spawnPoint;    // Kéo một Empty Object dưới chân Boss vào đây để định vị [cite: 2025-09-22]
    public float vfxScale = 20f;    

    void Awake()
    {
        combat = GetComponentInParent<BossCombat>();
    }

    public void ExecuteImpact()
{
    if (impactEffect != null)
    {
        // 1. Tắt để reset
        impactEffect.SetActive(false);

        // 2. Ép vị trí: Lấy vị trí của Boss nhưng ép Y = 0 (hoặc sát mặt đất)
        Vector3 groundPos = transform.position;
        groundPos.y = 0.05f; // Ép nó nằm trên mặt sàn một tí
        impactEffect.transform.position = groundPos; 

        // 3. Ép xoay và Scale
        impactEffect.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        impactEffect.transform.localScale = Vector3.one * vfxScale;

        // 4. Bật và Chạy
        impactEffect.SetActive(true);

        var director = impactEffect.GetComponent<UnityEngine.Playables.PlayableDirector>();
        if (director != null)
        {
            director.time = 0;
            director.Play();
        }
        
        Debug.Log("Đã nổ VFX tại chân Boss!");
    }
}
    // Các hàm AE_ của sếp giữ nguyên...
    public void AE_RoarEnd() { combat.AE_RoarEnd(); }
    public void AE_AttackEnd() { combat.AE_AttackEnd(); }
    public void AE_WeaponHitboxOn() { if (weaponHitbox != null) weaponHitbox.EnableHitbox(); }
    public void AE_WeaponHitboxOff() { if (weaponHitbox != null) weaponHitbox.DisableHitbox(); }
    public void AE_ShoulderHitboxOn() { if (shoulderHitbox != null) shoulderHitbox.EnableHitbox(); }
    public void AE_ShoulderHitboxOff() { if (shoulderHitbox != null) shoulderHitbox.DisableHitbox(); }
}
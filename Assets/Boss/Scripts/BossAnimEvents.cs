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
        public GameObject impactEffect1; // Kéo cái EarthSlam từ Hierarchy vào đây [cite: 2025-09-22]
    public Transform spawnPoint1;    // Kéo một Empty Object dưới chân Boss vào đây để định vị [cite: 2025-09-22]
    public float vfxScale1 = 20f;     

    void Awake()
    {
        combat = GetComponentInParent<BossCombat>();
    }

    [Header("VFX Adjustment")]
public float yOffset = 0.1f; // Tăng số này lên để lôi VFX lên khỏi mặt đất

public void ExecuteImpact()
{
    if (impactEffect != null)
    {
        impactEffect.SetActive(false);

        // Lấy vị trí Boss và cộng thêm một khoảng Y để không bị chìm
        Vector3 spawnPos = transform.position;
        spawnPos.y += yOffset; 
        impactEffect.transform.position = spawnPos;

        // Giữ nguyên Scale khổng lồ của sếp
        impactEffect.transform.localScale = Vector3.one * vfxScale;

        impactEffect.SetActive(true);
        
        // [cite: 2025-09-22] Nếu sếp muốn nó không xoay láo, hãy thử Reset Rotation về mặc định
        // impactEffect.transform.rotation = Quaternion.identity; 

        Debug.Log("VFX đã được lôi lên độ cao: " + spawnPos.y);
    }
}
public void ExecuteImpact1()
{
    if (impactEffect1 != null)
    {
        impactEffect1.SetActive(false);

        // Lấy vị trí Boss và cộng thêm một khoảng Y để không bị chìm
        Vector3 spawnPos = transform.position;
        spawnPos.y += yOffset; 
        impactEffect1.transform.position = spawnPos;

        // Giữ nguyên Scale khổng lồ của sếp
        impactEffect1.transform.localScale = Vector3.one * vfxScale1;

        impactEffect1.SetActive(true);
        
        // [cite: 2025-09-22] Nếu sếp muốn nó không xoay láo, hãy thử Reset Rotation về mặc định
        // impactEffect.transform.rotation = Quaternion.identity; 

        Debug.Log("VFX đã được lôi lên độ cao: " + spawnPos.y);
    }
}
public void PlayBossSound(AudioClip clip)
{
    if (clip != null && GetComponent<AudioSource>() != null)
    {
        GetComponent<AudioSource>().PlayOneShot(clip);
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
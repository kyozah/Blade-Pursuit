using UnityEngine;
using UnityEngine.Playables; // Thư viện để chạy Timeline (Playable Director)
using System.Collections;

public class VfxCombatTrigger : MonoBehaviour
{
    [Header("Cấu hình hiệu ứng")]
    [Tooltip("Kéo cái VFX đã gắn sẵn ở chân/tay Boss vào đây")]
    public GameObject impactEffect; 
    
    [Tooltip("Tỉ lệ phóng đại hiệu ứng")]
    public float effectScaleMultiplier = 10f;

    private PlayableDirector director;

    void Start()
    {
        if (impactEffect != null)
        {
            director = impactEffect.GetComponent<PlayableDirector>();
            impactEffect.SetActive(false); // Đảm bảo nó tắt lúc đầu
        }
    }

    // [cite: 2025-09-22] Hàm này dùng để gọi từ Animation Event (Sửa grammar: TriggerVFX -> ExecuteImpact)
    public void ExecuteImpact()
    {
        if (impactEffect != null)
        {
            // 1. Kích hoạt Object
            impactEffect.SetActive(true);

            // 2. Ép Scale (Vượt qua các lớp folder cha lỏ)
            impactEffect.transform.localScale = Vector3.one * effectScaleMultiplier;

            // 3. Chạy Timeline từ frame đầu tiên
            if (director != null)
            {
                director.time = 0;
                director.Play();
            }

            // 4. Tự động dọn dẹp (Ẩn đi)
            StopAllCoroutines();
            StartCoroutine(DeactivateAfterDelay(3f));
        }
        else
        {
            Debug.LogWarning("Sếp ơi, chưa kéo cái VFX vào ô Impact Effect kìa!");
        }
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (impactEffect != null) impactEffect.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cập nhật Slider làm thanh máu của Boss.
/// Gắn script này vào Canvas (thường là child của Boss).
/// Canvas có thể để World Space hoặc Screen Space.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth;
    public Slider healthSlider;
    public CanvasGroup canvasGroup; // optional, để show/hide mượt

    [Header("Options")]
    public bool faceCamera = true; // quay về Camera chính nếu world-space
    public float showDuration = 3f; // hiển thị sau khi bị thương

    float hideTimer;

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponentInParent<BossHealth>();

        if (bossHealth == null)
        {
            Debug.LogWarning($"[{nameof(BossHealthBar)}] No BossHealth found for {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogWarning($"[{nameof(BossHealthBar)}] No Slider assigned on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }

        healthSlider.maxValue = bossHealth.MaxHP;
        healthSlider.value = bossHealth.CurrentHP;

        bossHealth.OnHealthChanged += OnHealthChanged;
        Show();
    }

    void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        Show();

        if (current <= 0f)
            HideImmediate();
    }

    void Update()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                FadeOut();
        }

        if (faceCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    void Show()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        hideTimer = showDuration;
    }

    void FadeOut()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (healthSlider != null)
            healthSlider.value = 0f;
    }
}

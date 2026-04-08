using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    [Header("UI Elements")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;

    private float _targetFill = 1f; // Mặc định là đầy thanh (100%)

    void Start()
    {
        // Tự động tìm PlayerHealth ở object cha nếu bạn quên gán trong Inspector
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        // Cập nhật trạng thái đầy máu lúc vừa vào game
        if (playerHealth != null)
        {
            UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            if (healthBarFill != null) healthBarFill.fillAmount = _targetFill;
        }
    }

    // Hàm này nhận dữ liệu từ NetworkHealthSync để trừ máu qua mạng
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0)
        {
            _targetFill = currentHealth / maxHealth;
            
            // Giữ nguyên chức năng hiển thị số (ví dụ: 80/100)
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            }
        }
    }

    public void SetTarget(PlayerHealth target)
    {
        playerHealth = target;
    }

    void Update()
    {
        // 1. Nếu là thanh máu của chính mình (có playerHealth), cập nhật liên tục
        if (playerHealth != null)
        {
            UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }

        // 2. Logic làm mượt (Smooth Fill) - Đảm bảo thanh máu luôn co giãn mượt mà
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, _targetFill, smoothSpeed * Time.deltaTime);
        }

        // 3. Xoay về Camera (Billboard) - Giúp thanh máu trên đầu không bị dẹt
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
        }
    }
}
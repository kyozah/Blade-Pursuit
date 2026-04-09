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
    public TextMeshProUGUI playerNameText; // ✅ THÊM: Text hiển thị tên
    
    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;

    private float _targetFill = 1f;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
        
        // ✅ THÊM: Lấy tên từ NetworkPlayerSync
        var playerSync = GetComponentInParent<NetworkPlayerSync>();
        if (playerSync != null && playerNameText != null)
        {
            string playerName = playerSync.NetworkPlayerName.Value;
            if (string.IsNullOrEmpty(playerName))
            {
                // Fallback: lấy từ NetworkManager
                var networkManager = FindFirstObjectByType<NetworkManager>();
                if (networkManager != null)
                    playerName = networkManager.GetLocalPlayerName();
            }
            playerNameText.text = playerName ?? "Player";
        }

        if (playerHealth != null)
        {
            UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            if (healthBarFill != null) healthBarFill.fillAmount = _targetFill;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0)
        {
            _targetFill = currentHealth / maxHealth;
            
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
        if (playerHealth != null)
        {
            UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, _targetFill, smoothSpeed * Time.deltaTime);
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
        }
    }
}
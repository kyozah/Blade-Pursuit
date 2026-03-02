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

    private float targetFill;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        UpdateUI(true);
    }

    void Update()
    {
        if (playerHealth == null || healthBarFill == null) return;

        UpdateUI(false);
    }

    void UpdateUI(bool instant)
    {
        float current = playerHealth.GetCurrentHealth();
        float max = playerHealth.GetMaxHealth();

        targetFill = playerHealth.GetHealthPercentage();

        if (instant)
        {
            healthBarFill.fillAmount = targetFill;
        }
        else
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFill,
                smoothSpeed * Time.deltaTime
            );
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }
}
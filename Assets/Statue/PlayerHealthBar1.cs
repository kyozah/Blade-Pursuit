using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerHealthBar1 : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    [Header("UI Elements")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;

    void Start()
    {
        // Nếu không gán thủ công thì KHÔNG tự FindFirstObjectByType
        // vì multiplayer có nhiều PlayerHealth — sẽ nhầm player
        if (playerHealth != null)
            UpdateUI(true);
    }

    // ✅ NetworkPlayerSync gọi để gán đúng player
    public void SetTarget(PlayerHealth target)
    {
        playerHealth = target;
        UpdateUI(true);
    }

    void Update()
    {
        if (playerHealth == null || healthBarFill == null) return;
        UpdateUI(false);
    }

    void UpdateUI(bool instant)
    {
        if (playerHealth == null) return;

        float targetFill = playerHealth.GetHealthPercentage();

        if (instant)
            healthBarFill.fillAmount = targetFill;
        else
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFill,
                smoothSpeed * Time.deltaTime
            );

        if (healthText != null)
        {
            float current = playerHealth.GetCurrentHealth();
            float max     = playerHealth.GetMaxHealth();
            healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }

}
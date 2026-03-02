using UnityEngine;

public class HealingStatue : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public bool hasBeenUsed = false;

    [Header("Visual Feedback")]
    [Tooltip("Particle hoặc VFX khi hồi máu (optional)")]
    public GameObject healVFX;

    [Tooltip("Material khi tượng đã hết tác dụng (optional)")]
    public Material usedMaterial;

    private Renderer statueRenderer;

    void Start()
    {
        statueRenderer = GetComponentInChildren<Renderer>();
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (hasBeenUsed)
        {
            Debug.Log("🗿 Tượng này đã được sử dụng rồi.");
            return;
        }

        PlayerHealth playerHealth = interactor.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        // Hồi đầy máu
        playerHealth.Heal(playerHealth.GetMaxHealth());

        hasBeenUsed = true;

        Debug.Log("✨ Hồi đầy máu!");

        // VFX
        if (healVFX != null)
            Instantiate(healVFX, transform.position, Quaternion.identity);

        // Đổi material sang "đã dùng" nếu có
        if (usedMaterial != null && statueRenderer != null)
            statueRenderer.material = usedMaterial;
    }
}
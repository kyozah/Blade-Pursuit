using UnityEngine;

/// <summary>
/// Debug script to check Boss UI setup and identify issues.
/// Attach this to any GameObject in the scene and check the console logs when running.
/// </summary>
public class BossUIDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== BOSS UI DEBUG START ===");

        // Check 1: BossUIManager exists and has Instance
        if (BossUIManager.Instance == null)
        {
            Debug.LogError("[DEBUG] BossUIManager.Instance is NULL! You must create a GameObject with BossUIManager component in the scene.");
        }
        else
        {
            Debug.Log("[DEBUG] ✅ BossUIManager.Instance found.");

            // Check 2: BossUIManager has bossUI assigned
            if (BossUIManager.Instance.GetComponent<BossUIManager>().bossUI == null)
            {
                Debug.LogError("[DEBUG] BossUIManager.bossUI is NULL! Assign BossUI_Root (with BossHealthScreenUI) in the Inspector.");
            }
            else
            {
                Debug.Log("[DEBUG] ✅ BossUIManager.bossUI is assigned.");

                // Check 3: BossHealthScreenUI has required fields
                var ui = BossUIManager.Instance.GetComponent<BossUIManager>().bossUI;
                if (ui.healthSlider == null)
                    Debug.LogError("[DEBUG] BossHealthScreenUI.healthSlider is NULL! Assign the Slider.");
                else
                    Debug.Log("[DEBUG] ✅ BossHealthScreenUI.healthSlider is assigned.");

                if (ui.introNameText == null && ui.introImage == null)
                    Debug.LogWarning("[DEBUG] BossHealthScreenUI.introNameText and introImage are both NULL! You need at least one for intro.");
                else
                    Debug.Log("[DEBUG] ✅ Intro UI has either text or image.");

                if (ui.introCanvasGroup == null)
                    Debug.LogError("[DEBUG] BossHealthScreenUI.introCanvasGroup is NULL! Assign IntroGroup CanvasGroup.");
                else
                    Debug.Log("[DEBUG] ✅ BossHealthScreenUI.introCanvasGroup is assigned.");

                if (ui.mainCanvasGroup == null)
                    Debug.LogError("[DEBUG] BossHealthScreenUI.mainCanvasGroup is NULL! Assign MainGroup CanvasGroup.");
                else
                    Debug.Log("[DEBUG] ✅ BossHealthScreenUI.mainCanvasGroup is assigned.");
            }
        }

        // Check 4: Find all BossBrain instances
        BossBrain[] bosses = FindObjectsOfType<BossBrain>();
        if (bosses.Length == 0)
        {
            Debug.LogWarning("[DEBUG] No BossBrain found in scene.");
        }
        else
        {
            Debug.Log($"[DEBUG] Found {bosses.Length} BossBrain(s):");
            foreach (var boss in bosses)
            {
                // Check player assignment
                if (boss.player == null)
                {
                    Debug.LogError($"[DEBUG] BossBrain '{boss.gameObject.name}' has NO player assigned! Assign player transform in Inspector.");
                }
                else
                {
                    Debug.Log($"[DEBUG] ✅ BossBrain '{boss.gameObject.name}' has player assigned: {boss.player.gameObject.name}");
                }

                // Check BossHealth
                var health = boss.GetComponentInChildren<BossHealth>();
                if (health == null)
                {
                    Debug.LogError($"[DEBUG] BossBrain '{boss.gameObject.name}' has NO BossHealth child! Add BossHealth component as child.");
                }
                else
                {
                    Debug.Log($"[DEBUG] ✅ BossBrain '{boss.gameObject.name}' has BossHealth: {health.gameObject.name}");
                    Debug.Log($"[DEBUG]    BossName: '{health.BossName}', MaxHP: {health.MaxHP}");
                    if (health.BossIcon != null)
                        Debug.Log($"[DEBUG]    BossIcon: {health.BossIcon.name}");
                    else
                        Debug.LogWarning($"[DEBUG]    BossIcon: NOT assigned (will use text intro)");
                }
            }
        }

        Debug.Log("=== BOSS UI DEBUG END ===");
    }
}

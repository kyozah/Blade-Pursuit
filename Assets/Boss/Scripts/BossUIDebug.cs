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

            // Check 2: BossUIManager has bossUIs assigned
            var manager = BossUIManager.Instance.GetComponent<BossUIManager>();
            if (manager.bossUIs == null || manager.bossUIs.Length == 0)
            {
                Debug.LogError("[DEBUG] BossUIManager.bossUIs array is empty! Assign one or more BossHealthScreenUI instances in the Inspector.");
            }
            else
            {
                Debug.Log($"[DEBUG] ✅ BossUIManager has {manager.bossUIs.Length} UI slot(s) assigned.");

                // Check each UI slot
                for (int i = 0; i < manager.bossUIs.Length; i++)
                {
                    var ui = manager.bossUIs[i];
                    if (ui == null)
                    {
                        Debug.LogError($"[DEBUG] bossUIs[{i}] is NULL! Make sure all slots are filled.");
                        continue;
                    }

                    if (ui.healthSlider == null)
                        Debug.LogError($"[DEBUG] bossUIs[{i}].healthSlider is NULL! Assign the Slider.");
                    else
                        Debug.Log($"[DEBUG] ✅ bossUIs[{i}].healthSlider is assigned.");

                    if (ui.introNameText == null && ui.introImage == null)
                        Debug.LogWarning($"[DEBUG] bossUIs[{i}] introNameText and introImage are both NULL! Need at least one.");
                    else
                        Debug.Log($"[DEBUG] ✅ bossUIs[{i}] has either text or image for intro.");

                    if (ui.introCanvasGroup == null)
                        Debug.LogError($"[DEBUG] bossUIs[{i}].introCanvasGroup is NULL! Assign IntroGroup CanvasGroup.");
                    else
                        Debug.Log($"[DEBUG] ✅ bossUIs[{i}].introCanvasGroup is assigned.");

                    if (ui.mainCanvasGroup == null)
                        Debug.LogError($"[DEBUG] bossUIs[{i}].mainCanvasGroup is NULL! Assign MainGroup CanvasGroup.");
                    else
                        Debug.Log($"[DEBUG] ✅ bossUIs[{i}].mainCanvasGroup is assigned.");
                }
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

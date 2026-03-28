using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý UI chiến thắng khi player tiêu diệt boss.
/// Hiển thị 2 nút: 1 để thoát game, 1 để quay lại menu.
/// </summary>
public class VictoryMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button continueButton; // Quay lại menu
    [SerializeField] private Button quitButton;     // Thoát game
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Timing")]
    [Tooltip("Time to wait after the last boss dies before showing the victory menu (seconds)")]
    [SerializeField] private float victoryDelay = 4f; // wait for death animation

    [Header("Bosses")]
    [Tooltip("Kéo vào đây tất cả BossHealth instances cần bị tiêu diệt để kích hoạt menu chiến thắng.")]
    public BossHealth[] bosses;

    private bool isVisible = false;
    private int deadBossCount = 0;

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            Debug.LogError("[VictoryMenuUI] CanvasGroup not found. Attach this script to a Canvas with CanvasGroup component.");
            return;
        }

        // Ẩn menu lúc đầu
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Hook buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        else
            Debug.LogWarning("[VictoryMenuUI] Continue button not assigned");

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        else
            Debug.LogWarning("[VictoryMenuUI] Quit button not assigned");

        // Auto-find bosses if not assigned
        if (bosses == null || bosses.Length == 0)
        {
            Debug.LogWarning("[VictoryMenuUI] No bosses assigned. Attempting auto-discovery...");
            bosses = Object.FindObjectsByType<BossHealth>(FindObjectsSortMode.None);
            if (bosses != null && bosses.Length > 0)
            {
                Debug.Log($"[VictoryMenuUI] Auto-discovered {bosses.Length} boss(es)");
            }
            else
            {
                Debug.LogError("[VictoryMenuUI] No bosses found in scene! Either assign them manually or ensure BossHealth components exist.");
                return;
            }
        }

        // Validate and filter out null bosses
        System.Collections.Generic.List<BossHealth> validBosses = new System.Collections.Generic.List<BossHealth>();
        foreach (var bh in bosses)
        {
            if (bh != null)
                validBosses.Add(bh);
            else
                Debug.LogWarning("[VictoryMenuUI] Found null element in bosses array!");
        }
        
        if (validBosses.Count == 0)
        {
            Debug.LogError("[VictoryMenuUI] No valid bosses found!");
            return;
        }

        bosses = validBosses.ToArray();
        
        // subscribe to boss deaths
        deadBossCount = 0;
        foreach (var bh in bosses)
        {
            bh.OnDied += HandleBossDied;
        }
        Debug.Log($"[VictoryMenuUI] ✅ Successfully subscribed to {bosses.Length} boss(es). Victory will show when all {bosses.Length} are defeated.");
    }


    public void ShowVictoryMenu()
    {
        if (isVisible) return;
        isVisible = true;

        Time.timeScale = 0f; // Pause game

        StartCoroutine(FadeInMenu());

        Debug.Log("[VictoryMenuUI] Victory menu shown");
    }

    private void HandleBossDied()
    {
        deadBossCount++;
        Debug.Log($"[VictoryMenuUI] Boss died ({deadBossCount}/{bosses.Length})");
        
        // Check if ALL configured bosses are dead before showing victory
        if (bosses != null && bosses.Length > 0 && deadBossCount >= bosses.Length)
        {
            Debug.Log($"[VictoryMenuUI] All {bosses.Length} bosses defeated! Showing victory menu after {victoryDelay}s");
            // all bosses dead – wait a moment (e.g. death animation) then show menu
            StartCoroutine(ShowVictoryMenuAfterDelay(victoryDelay));
        }
        else if (bosses != null)
        {
            Debug.Log($"[VictoryMenuUI] Waiting for more bosses to die... {bosses.Length - deadBossCount} remaining");
        }
    }

    System.Collections.IEnumerator ShowVictoryMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowVictoryMenu();
    }

    System.Collections.IEnumerator FadeInMenu()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnContinueClicked()
    {
        Debug.Log("[VictoryMenuUI] Continue button clicked - loading Menu scene");
        Time.timeScale = 1f; // Resume game before loading
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnQuitClicked()
    {
        Debug.Log("[VictoryMenuUI] Quit button clicked");
        Time.timeScale = 1f; // Resume game before quitting
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsVisible => isVisible;
}

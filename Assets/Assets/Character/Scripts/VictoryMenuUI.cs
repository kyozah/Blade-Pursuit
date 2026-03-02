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

        // subscribe to boss deaths if any assigned
        if (bosses != null && bosses.Length > 0)
        {
            deadBossCount = 0;
            foreach (var bh in bosses)
            {
                if (bh != null)
                    bh.OnDied += HandleBossDied;
            }
            Debug.Log($"[VictoryMenuUI] Subscribed to {bosses.Length} boss(es)");
        }
        else
        {
            // fallback to GameManager if no bosses configured
            Debug.Log("[VictoryMenuUI] No bosses assigned, falling back to GameManager event");
        }
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
        if (deadBossCount >= bosses.Length)
        {
            // all bosses dead – wait a moment (e.g. death animation) then show menu
            StartCoroutine(ShowVictoryMenuAfterDelay(victoryDelay));
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

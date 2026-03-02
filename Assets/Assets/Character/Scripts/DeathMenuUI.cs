using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Quản lý UI menu chết của player.
/// Hiển thị sau 2 giây khi player hết máu.
/// Khi bấm Play, load lại scene Gameplay từ đầu.
/// </summary>
public class DeathMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private float fadeInDuration = 0.5f;

    private bool isVisible = false;

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            Debug.LogError("[DeathMenuUI] CanvasGroup not found. Attach this script to a Canvas with CanvasGroup component.");
            return;
        }

        // Ẩn menu lúc đầu
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Hook buttons
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);
        else
            Debug.LogWarning("[DeathMenuUI] Replay button not assigned");

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
        else
            Debug.LogWarning("[DeathMenuUI] Menu button not assigned");

        // Subscribe to player death
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Không có event, nên ta phải check trong Update
            Debug.Log("[DeathMenuUI] PlayerHealth found, waiting for death...");
        }
    }

    void Update()
    {
        // Check nếu player đã chết mà menu chưa hiển thị
        if (!isVisible)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead())
            {
                // Chờ 2 giây nữa rồi show menu
                StartCoroutine(ShowDeathMenuAfterDelay(2f));
            }
        }
    }

    IEnumerator ShowDeathMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDeathMenu();
    }

    public void ShowDeathMenu()
    {
        if (isVisible) return;
        isVisible = true;

        Time.timeScale = 0f; // Pause game

        // ensure other UI elements can't intercept clicks (health bars, etc.)
        DisableOtherCanvasGroups();

        StartCoroutine(FadeInMenu());

        Debug.Log("[DeathMenuUI] Death menu shown");
    }

    IEnumerator FadeInMenu()
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

    public void OnReplayClicked()
    {
        Debug.Log("[DeathMenuUI] Replay button clicked - reloading current scene");
        Time.timeScale = 1f; // Resume game before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuClicked()
    {
        Debug.Log("[DeathMenuUI] Menu button clicked - loading Menu scene");
        Time.timeScale = 1f; // Resume game before loading
        SceneManager.LoadScene("Menu");
    }


    private void DisableOtherCanvasGroups()
    {
        CanvasGroup[] all = FindObjectsOfType<CanvasGroup>();
        foreach (var cg in all)
        {
            if (cg == canvasGroup) continue;
            // stop them from blocking raycasts so our menu receives clicks
            cg.blocksRaycasts = false;
        }
    }

    public bool IsVisible => isVisible;
}

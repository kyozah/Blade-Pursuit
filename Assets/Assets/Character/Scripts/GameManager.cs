using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Quản lý trạng thái chung của game.
/// Điều phối death menu, victory menu, và scene transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private DeathMenuUI deathMenuUI;
    [SerializeField] private VictoryMenuUI victoryMenuUI;

    private bool gameOver = false;
    private bool victory = false;

    [Header("Rewards")]
    [Tooltip("Maximum health bonus granted when a boss dies")]
    [SerializeField] private float bossMaxHealthBonus = 100f;

    [Tooltip("List of bosses that will give the above bonus when they die.")]
    [SerializeField] private BossHealth[] rewardBosses;

    // keep track of who has already rewarded so we don't give multiple times
    private System.Collections.Generic.HashSet<BossHealth> rewardedBosses = new System.Collections.Generic.HashSet<BossHealth>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Auto-find components nếu chưa assign
        if (playerHealth == null)
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();

        if (deathMenuUI == null)
            deathMenuUI = Object.FindFirstObjectByType<DeathMenuUI>();

        if (victoryMenuUI == null)
            victoryMenuUI = Object.FindFirstObjectByType<VictoryMenuUI>();

        // subscribe to reward bosses
        if (rewardBosses != null)
        {
            foreach (var bh in rewardBosses)
            {
                if (bh != null)
                {
                    // capture local variable
                    bh.OnDied += () => GiveBossReward(bh);
                }
            }
        }

        Debug.Log("[GameManager] Initialized");
    }

    /// <summary>
    /// Gọi khi player chết - hiển thị death menu
    /// </summary>
    public void OnPlayerDeath()
    {
        if (gameOver) return;
        
        gameOver = true;
        Debug.Log("[GameManager] Player died!");

        if (deathMenuUI != null)
        {
            deathMenuUI.ShowDeathMenu();
        }
    }

    /// <summary>
    /// Gọi khi boss chết - hiển thị victory menu
    /// </summary>
    public void OnBossDefeated()
    {
        if (victory) return;
        
        victory = true;
        Debug.Log("[GameManager] Boss defeated!");

        // victory menu only, reward handled per-boss via GiveBossReward()
        if (victoryMenuUI != null)
        {
            victoryMenuUI.ShowVictoryMenu();
        }
    }

    /// <summary>
    /// Called when one of the configured reward bosses dies.
    /// Grants the max‑health bonus once.
    /// </summary>
    private void GiveBossReward(BossHealth bh)
    {
        if (bh == null || rewardedBosses.Contains(bh)) return;
        rewardedBosses.Add(bh);
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(bossMaxHealthBonus, true);
            Debug.Log($"[GameManager] Rewarding boss {bh.BossName} with {bossMaxHealthBonus} HP bonus");
        }
    }

    /// <summary>
    /// Quay lại menu chính
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("[GameManager] Going to main menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Tải lại scene hiện tại (gameplay)
    /// </summary>
    public void ReloadCurrentScene()
    {
        Debug.Log("[GameManager] Reloading current scene");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Thoát game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] Quitting game");
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsGameOver => gameOver;
    public bool IsVictory => victory;
}

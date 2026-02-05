using UnityEngine;

public class BossUIManager : MonoBehaviour
{
    public static BossUIManager Instance { get; private set; }

    [Tooltip("Assign the screen-space Boss UI here")]
    public BossHealthScreenUI bossUI;

    // Currently active boss health that UI is bound to
    BossHealth currentBoss;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // keep manager across scenes if needed
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Change the displayed name for the currently-bound boss UI.
    /// If writeBackToBoss is true, also updates the BossHealth.BossName value.
    /// </summary>
    public void SetCurrentBossDisplayName(string newName, bool writeBackToBoss = false)
    {
        if (bossUI == null) return;
        if (currentBoss != null && writeBackToBoss)
            currentBoss.BossName = newName;
        bossUI.SetOverrideName(newName);
    }

    /// <summary>
    /// Clear any override name and revert display to the boss's own name.
    /// </summary>
    public void ClearCurrentBossDisplayName()
    {
        if (bossUI == null) return;
        bossUI.ClearOverrideName();
    }

    /// <summary>
    /// Called by Boss when player is detected / boss roars.
    /// Binds the boss health to the screen UI and shows the intro name.
    /// This method will ignore repeated calls for the same boss (prevents flicker).
    /// Optionally pass a displayName or displaySprite to override the boss's own name/icon on the UI.
    /// </summary>
    public void ShowBoss(BossBrain brain, string displayName = null, Sprite displaySprite = null, Sprite displayHealthBarSprite = null)
    {
        Debug.Log("[BossUIManager.ShowBoss] Called");

        if (bossUI == null)
        {
            Debug.LogError("[BossUIManager] No BossHealthScreenUI assigned.");
            return;
        }

        if (brain == null)
        {
            Debug.LogError("[BossUIManager] brain is NULL!");
            return;
        }

        var h = brain.GetComponentInChildren<BossHealth>();
        if (h == null)
        {
            Debug.LogError("[BossUIManager] BossBrain has no BossHealth child!");
            return;
        }

        Debug.Log($"[BossUIManager] Found BossHealth: {h.BossName}");

        // If we're already showing UI for this boss, ignore duplicate calls
        if (currentBoss == h && bossUI.IsShowingFor(h))
        {
            Debug.Log("[BossUIManager] Already showing UI for this boss, ignoring duplicate call");
            return;
        }

        // Bind to new boss (or rebind to same) and show intro (with optional override name/sprite/healthbar)
        currentBoss = h;
        Debug.Log($"[BossUIManager] Binding to boss: {h.BossName}");
        bossUI.BindToBoss(h, displayName, displaySprite, displayHealthBarSprite);
        
        Debug.Log("[BossUIManager] Calling ShowNameIntro()");
        bossUI.ShowNameIntro();

        // When this boss dies, clear currentBoss so next boss can show
        h.OnDied += () =>
        {
            if (currentBoss == h) currentBoss = null;
        };
    }

    /// <summary>
    /// Override the displayed sprite for the currently-bound boss UI.
    /// If writeBackToBoss is true, also updates the BossHealth.BossIcon value.
    /// </summary>
    public void SetCurrentBossDisplaySprite(Sprite newSprite, bool writeBackToBoss = false)
    {
        if (bossUI == null) return;
        if (currentBoss != null && writeBackToBoss)
            currentBoss.BossIcon = newSprite;
        bossUI.SetOverrideSprite(newSprite);
    }

    /// <summary>
    /// Clear any override sprite and revert display to the boss's own icon/text.
    /// </summary>
    public void ClearCurrentBossDisplaySprite()
    {
        if (bossUI == null) return;
        bossUI.ClearOverrideSprite();
    }

    /// <summary>
    /// Override the health-bar image for the currently-bound boss UI.
    /// If writeBackToBoss is true, this will **not** change boss data (healthbar sprite is UI-only).
    /// </summary>
    public void SetCurrentBossHealthBarSprite(Sprite newSprite, bool writeBackToBoss = false)
    {
        if (bossUI == null) return;
        bossUI.SetHealthBarSprite(newSprite);
    }

    public void ClearCurrentBossHealthBarSprite()
    {
        if (bossUI == null) return;
        bossUI.ClearOverrideSpriteHealthBar();
    }
}
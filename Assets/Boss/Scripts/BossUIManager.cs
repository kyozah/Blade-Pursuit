using UnityEngine;

public class BossUIManager : MonoBehaviour
{
    public static BossUIManager Instance { get; private set; }

    [Tooltip("Assign one or more screen-space Boss UI elements here (one per boss type).")]
    public BossHealthScreenUI[] bossUIs;

    // track which boss (BossHealth) is bound to which UI instance
    readonly System.Collections.Generic.Dictionary<BossHealth, BossHealthScreenUI> boundUIs
        = new System.Collections.Generic.Dictionary<BossHealth, BossHealthScreenUI>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // keep manager across scenes if needed
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Retrieve the UI instance associated with the given boss health.
    /// If the boss is not already bound, the first available UI is returned
    /// (fallback to zero-index) and the binding is recorded.
    /// </summary>
    BossHealthScreenUI GetOrCreateUIForBoss(BossHealth boss)
    {
        if (boss == null || bossUIs == null || bossUIs.Length == 0)
            return null;

        if (boundUIs.TryGetValue(boss, out var ui))
            return ui;

        // fallback: choose based on boss-specific preference (index stored on boss?)
        // for now just grab the first unused UI slot
        foreach (var candidate in bossUIs)
        {
            if (!boundUIs.ContainsValue(candidate))
            {
                boundUIs[boss] = candidate;
                return candidate;
            }
        }

        // all UIs are already taken, just re-use the first one
        boundUIs[boss] = bossUIs[0];
        return bossUIs[0];
    }

    /// <summary>
    /// Change the displayed name for the specified boss's UI.
    /// </summary>
    public void SetBossDisplayName(BossHealth boss, string newName, bool writeBackToBoss = false)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        if (boss != null && writeBackToBoss)
            boss.BossName = newName;
        ui.SetOverrideName(newName);
    }

    /// <summary>
    /// Clear any override name for the specified boss.
    /// </summary>
    public void ClearBossDisplayName(BossHealth boss)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        ui.ClearOverrideName();
    }

    /// <summary>
    /// Called by Boss when player is detected / boss roars.
    /// Binds the boss health to the screen UI and shows the intro name.
    /// This method will ignore repeated calls for the same boss (prevents flicker).
    /// Optionally pass a displayName or displaySprite to override the boss's own name/icon on the UI.
    /// </summary>
    /// <summary>
    /// Show the UI for a boss.  If you have multiple UI objects assigned the manager
    /// will automatically pick one that isn't already hosting another boss.  You can
    /// also supply an explicit index by setting the BossBrain.bossUIIndex field (see
    /// the modified BossBrain below) or by calling the overload that accepts an index.
    /// Optional overrides work exactly as before.
    /// </summary>
    public void ShowBoss(BossBrain brain, string displayName = null, Sprite displaySprite = null, Sprite displayHealthBarSprite = null)
    {
        Debug.Log("[BossUIManager.ShowBoss] Called");

        if (bossUIs == null || bossUIs.Length == 0)
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

        // fetch or create a UI slot for this boss
        var ui = GetOrCreateUIForBoss(h);
        if (ui == null)
        {
            Debug.LogError("[BossUIManager] Unable to obtain a UI instance for boss");
            return;
        }

        // skip duplicate show if the selected UI already shows this boss
        if (ui.IsShowingFor(h))
        {
            Debug.Log("[BossUIManager] Already showing UI for this boss, ignoring duplicate call");
            return;
        }

        Debug.Log($"[BossUIManager] Binding to boss: {h.BossName} using UI '{ui.name}'");
        ui.BindToBoss(h, displayName, displaySprite, displayHealthBarSprite);
        
        Debug.Log("[BossUIManager] Calling ShowNameIntro()");
        ui.ShowNameIntro();

        // when boss dies, free the UI slot
        h.OnDied += () =>
        {
            if (boundUIs.ContainsKey(h))
                boundUIs.Remove(h);
        };
    }

    /// <summary>
    /// Override the displayed sprite for a given boss UI.
    /// If writeBackToBoss is true, also updates the BossHealth.BossIcon value.
    /// </summary>
    public void SetBossDisplaySprite(BossHealth boss, Sprite newSprite, bool writeBackToBoss = false)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        if (boss != null && writeBackToBoss)
            boss.BossIcon = newSprite;
        ui.SetOverrideSprite(newSprite);
    }

    /// <summary>
    /// Clear any override sprite for the given boss.
    /// </summary>
    public void ClearBossDisplaySprite(BossHealth boss)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        ui.ClearOverrideSprite();
    }

    /// <summary>
    /// Override the health-bar image for the given boss UI.
    /// </summary>
    public void SetBossHealthBarSprite(BossHealth boss, Sprite newSprite)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        ui.SetHealthBarSprite(newSprite);
    }

    public void ClearBossHealthBarSprite(BossHealth boss)
    {
        var ui = GetOrCreateUIForBoss(boss);
        if (ui == null) return;
        ui.ClearOverrideSpriteHealthBar();
    }
}
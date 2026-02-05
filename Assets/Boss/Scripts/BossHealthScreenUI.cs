using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-space Boss health UI (Hollow Knight style). Attach to a Canvas in Screen Space - Overlay.
/// - Assign a Slider and optional Text for boss name.
/// - Set `bossHealth` at runtime (recommended) when a boss is engaged, or assign in inspector for single-boss.
/// </summary>
public class BossHealthScreenUI : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth; // set this when showing a boss
    public Slider healthSlider;
    public Text bossNameText;
    public CanvasGroup canvasGroup;

    // Optional images to show instead of text
    public Image introImage;        // big image shown during intro
    public Image bossIconImage;     // small icon shown in main HUD
    public Image healthBarImage;    // optional image used as background/decor for health bar (main HUD)

    // Internal override sprite if set programmatically
    Sprite currentOverrideSprite;
    bool overrideSpriteSet = false;

    // Health bar sprite override
    Sprite currentHealthBarOverride;
    bool overrideHealthBarSet = false;

    [Header("Behavior")]
    public float smoothSpeed = 8f;
    public float showDuration = 3f;

    [Header("Intro")]
    public CanvasGroup introCanvasGroup; // big name intro
    public Text introNameText;
    public CanvasGroup mainCanvasGroup; // main HUD (slider + small name)
    public float introDuration = 3f;

    float targetValue;
    float hideTimer;
    Coroutine introCoroutine;

    // Prevent repeated intro plays for same boss
    bool introPlayed = false;

    /// <summary>
    /// Returns true if this UI is currently showing (intro or main) for the given boss.
    /// </summary>
    public bool IsShowingFor(BossHealth b)
    {
        if (b == null || bossHealth != b) return false;
        if (introCoroutine != null) return true;
        if (mainCanvasGroup != null && mainCanvasGroup.alpha > 0f) return true;
        if (canvasGroup != null && canvasGroup.alpha > 0f) return true;
        return false;
    }

    void Start()
    {
        if (healthSlider == null)
        {
            Debug.LogWarning($"[{nameof(BossHealthScreenUI)}] No Slider assigned. Disabling.");
            enabled = false;
            return;
        }

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Ensure groups are present
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (mainCanvasGroup == null)
            mainCanvasGroup = canvasGroup;

        // Start hidden
        HideImmediate();

        // If a boss is pre-assigned in inspector, start listening
        if (bossHealth != null)
            BindToBoss(bossHealth);
    }

    void Update()
    {
        // Smoothly interpolate slider value
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * smoothSpeed);
        }

        if (canvasGroup != null && canvasGroup.alpha > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                FadeOut();
        }
    }

    public void BindToBoss(BossHealth b, string overrideName = null, Sprite overrideSprite = null, Sprite overrideHealthBarSprite = null)
    {
        if (bossHealth != null)
            Unbind();

        bossHealth = b;
        if (bossHealth == null) return;

        healthSlider.maxValue = bossHealth.MaxHP;
        targetValue = bossHealth.CurrentHP;
        healthSlider.value = targetValue;

        // Determine sprite to use for face/icon: override > boss's own icon
        Sprite displaySprite = overrideSprite != null ? overrideSprite : bossHealth.BossIcon;
        overrideSpriteSet = overrideSprite != null;
        currentOverrideSprite = overrideSprite;

        // Health bar sprite: override (param) or leave default
        overrideHealthBarSet = overrideHealthBarSprite != null;
        currentHealthBarOverride = overrideHealthBarSprite;
        if (healthBarImage != null)
        {
            Sprite hb = overrideHealthBarSprite != null ? overrideHealthBarSprite : healthBarImage.sprite;
            if (hb != null)
            {
                healthBarImage.sprite = hb;
                healthBarImage.gameObject.SetActive(true);
            }
            else
            {
                healthBarImage.gameObject.SetActive(false);
            }
        }

        bossHealth.OnHealthChanged += OnHealthChanged;
        bossHealth.OnDied += OnBossDied;

        // Setup intro: show image only
        if (introImage != null && displaySprite != null)
        {
            introImage.sprite = displaySprite;
            introImage.gameObject.SetActive(true);
        }
        else if (introImage != null)
        {
            introImage.gameObject.SetActive(false);
        }

        // Hide intro text
        if (introNameText != null)
            introNameText.gameObject.SetActive(false);

        // Main HUD: set icon sprite (show icon + healthbar image + slider)
        if (bossIconImage != null)
        {
            Sprite iconSprite = overrideSpriteSet ? currentOverrideSprite : (bossHealth != null ? bossHealth.BossIcon : null);
            if (iconSprite != null)
            {
                bossIconImage.sprite = iconSprite;
                bossIconImage.gameObject.SetActive(true);
            }
            else
            {
                bossIconImage.gameObject.SetActive(false);
            }
        }

        // Hide main text
        if (bossNameText != null)
            bossNameText.gameObject.SetActive(false);

        // Start with intro hidden until ShowNameIntro is called
        if (introCanvasGroup != null)
            introCanvasGroup.alpha = 0f;

        // Start with main panel hidden
        if (mainCanvasGroup != null)
            mainCanvasGroup.alpha = 0f;

        // Reset intro flag for newly bound boss
        introPlayed = false;
    }

    public void Unbind()
    {
        if (bossHealth == null) return;
        bossHealth.OnHealthChanged -= OnHealthChanged;
        bossHealth.OnDied -= OnBossDied;
        bossHealth = null;
        introPlayed = false;
    }

    /// <summary>
    /// Override the displayed boss name in UI without changing the underlying boss data.
    /// </summary>
    /// <summary>
    /// For image-only UI, this method is not used.
    /// To change the image displayed, use SetOverrideSprite instead.
    /// </summary>
    [System.Obsolete("Use SetOverrideSprite instead for image-only UI")]
    public void SetOverrideName(string newName)
    {
        // Do nothing — we're image-only
    }

    [System.Obsolete("Use ClearOverrideSprite instead for image-only UI")]
    public void ClearOverrideName()
    {
        // Do nothing — we're image-only
    }

    /// <summary>
    /// Override the health bar image (background/frame) shown in main HUD.
    /// </summary>
    public void SetHealthBarSprite(Sprite s)
    {
        if (s == null) return;
        currentHealthBarOverride = s;
        overrideHealthBarSet = true;
        if (healthBarImage != null)
        {
            healthBarImage.sprite = s;
            healthBarImage.gameObject.SetActive(true);
        }
    }

    public void ClearOverrideSpriteHealthBar()
    {
        overrideHealthBarSet = false;
        currentHealthBarOverride = null;
        // do not change healthBarImage.sprite here; keep existing or hide if none
    }

    /// <summary>
    /// Override the displayed boss image in UI without changing the underlying boss data.
    /// </summary>
    /// <summary>
    /// Override the displayed boss image in UI (for both intro and main HUD).
    /// </summary>
    public void SetOverrideSprite(Sprite s)
    {
        if (s == null) return;
        currentOverrideSprite = s;
        overrideSpriteSet = true;

        // Update intro image
        if (introImage != null)
        {
            introImage.sprite = s;
            introImage.gameObject.SetActive(true);
        }

        // Update main HUD icon
        if (bossIconImage != null)
        {
            bossIconImage.sprite = s;
            bossIconImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Clear sprite override and revert to boss's own icon.
    /// </summary>
    public void ClearOverrideSprite()
    {
        overrideSpriteSet = false;
        currentOverrideSprite = null;

        Sprite bossSprite = bossHealth != null ? bossHealth.BossIcon : null;

        // Revert intro image
        if (introImage != null)
        {
            if (bossSprite != null)
            {
                introImage.sprite = bossSprite;
                introImage.gameObject.SetActive(true);
            }
            else
            {
                introImage.gameObject.SetActive(false);
            }
        }

        // Revert main HUD icon
        if (bossIconImage != null)
        {
            if (bossSprite != null)
            {
                bossIconImage.sprite = bossSprite;
                bossIconImage.gameObject.SetActive(true);
            }
            else
            {
                bossIconImage.gameObject.SetActive(false);
            }
        }

        ClearOverrideSpriteHealthBar();
    }

    void OnHealthChanged(float current, float max)
    {
        healthSlider.maxValue = max;
        targetValue = current;

        // Ensure main HUD visible (image + healthbar + slider only, no text)
        if (mainCanvasGroup != null)
            mainCanvasGroup.alpha = 1f;

        Show();
    }

    void OnBossDied()
    {
        HideImmediate();
        Unbind();
        introPlayed = false;
    }

    void Show()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        hideTimer = showDuration;
    }

    void FadeOut()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (mainCanvasGroup != null)
            mainCanvasGroup.alpha = 0f;
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (healthSlider != null)
            healthSlider.value = 0f;

        if (mainCanvasGroup != null)
            mainCanvasGroup.alpha = 0f;
        if (introCanvasGroup != null)
            introCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Show big name intro for `duration` seconds, then fade to main HUD.
    /// </summary>
    public void ShowNameIntro(float duration = -1f)
    {
        Debug.Log($"[BossHealthScreenUI] ShowNameIntro START - bossHealth: {(bossHealth != null ? "ASSIGNED" : "NULL")}");
        
        if (introPlayed)
        {
            Debug.LogWarning($"[BossHealthScreenUI] Intro already played for {(bossHealth != null ? bossHealth.BossName : "Unknown")}");
            return;
        }

        Debug.Log($"[BossHealthScreenUI] ShowNameIntro called for boss: {(bossHealth != null ? bossHealth.BossName : "Unknown")}");
        Debug.Log($"[BossHealthScreenUI] introImage is {(introImage != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"[BossHealthScreenUI] bossHealth is {(bossHealth != null ? "ASSIGNED" : "NULL")}");
        if (bossHealth != null)
        {
            Debug.Log($"[BossHealthScreenUI] bossHealth.BossIcon is {(bossHealth.BossIcon != null ? "ASSIGNED" : "NULL")}");
        }

        // Ensure intro shows image only
        Sprite displaySprite = overrideSpriteSet ? currentOverrideSprite : (bossHealth != null ? bossHealth.BossIcon : null);
        Debug.Log($"[BossHealthScreenUI] displaySprite: {(displaySprite != null ? displaySprite.name : "NULL")}");
        
        if (displaySprite == null)
        {
            Debug.LogWarning("[BossHealthScreenUI] No sprite found for intro (BossIcon not assigned or bossHealth null)");
        }
        else
        {
            Debug.Log($"[BossHealthScreenUI] Display sprite found: {displaySprite.name}");
        }
        
        if (introImage != null)
        {
            if (displaySprite != null)
            {
                introImage.sprite = displaySprite;
                introImage.gameObject.SetActive(true);
                Debug.Log("[BossHealthScreenUI] Intro image activated with sprite");
            }
            else
            {
                introImage.gameObject.SetActive(false);
                Debug.LogWarning("[BossHealthScreenUI] Intro image set inactive (no sprite)");
            }
        }
        else
        {
            Debug.LogError("[BossHealthScreenUI] introImage is NULL! Assign it in Inspector");
            return;
        }

        // Ensure intro canvas group is present and will be shown
        if (introCanvasGroup == null)
        {
            Debug.LogError("[BossHealthScreenUI] introCanvasGroup is NULL! Assign it in Inspector");
            return;
        }
        else
        {
            Debug.Log("[BossHealthScreenUI] introCanvasGroup is ASSIGNED");
        }

        // Ensure text is hidden
        if (introNameText != null)
            introNameText.gameObject.SetActive(false);

        if (duration <= 0f)
            duration = introDuration;

        if (introCoroutine != null)
            StopCoroutine(introCoroutine);

        introPlayed = true;
        Debug.Log($"[BossHealthScreenUI] Starting intro coroutine for duration {duration}s");
        introCoroutine = StartCoroutine(IntroRoutine(duration));
    }

    System.Collections.IEnumerator IntroRoutine(float duration)
    {
        // Delay before showing intro
        Debug.Log("[IntroRoutine] Waiting 0.5s before intro...");
        yield return new WaitForSeconds(0.5f);

        // Setup: show intro canvas group (which contains introImage)
        Debug.Log("[IntroRoutine] Starting - showing intro canvas");
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 1f;
            Debug.Log("[IntroRoutine] Intro canvas alpha set to 1");
        }
        else
        {
            Debug.LogError("[IntroRoutine] introCanvasGroup is NULL!");
        }

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
            Debug.Log("[IntroRoutine] Main canvas alpha set to 0");
        }

        Show();

        // Wait for duration
        Debug.Log($"[IntroRoutine] Waiting {duration}s for intro");
        yield return new WaitForSeconds(duration);

        // Fade cross
        Debug.Log("[IntroRoutine] Starting fade transition");
        float fadeTime = 0.4f;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            if (introCanvasGroup != null)
                introCanvasGroup.alpha = a;
            if (mainCanvasGroup != null)
                mainCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }

        if (introCanvasGroup != null)
            introCanvasGroup.alpha = 0f;
        if (mainCanvasGroup != null)
            mainCanvasGroup.alpha = 1f;

        Debug.Log("[IntroRoutine] Fade complete - showing main HUD");

        // Ensure main HUD shows icon + healthbar image + slider (no text)
        if (bossIconImage != null)
        {
            Sprite iconSprite = overrideSpriteSet ? currentOverrideSprite : (bossHealth != null ? bossHealth.BossIcon : null);
            if (iconSprite != null)
            {
                bossIconImage.sprite = iconSprite;
                bossIconImage.gameObject.SetActive(true);
            }
            else
            {
                bossIconImage.gameObject.SetActive(false);
            }
        }

        // Hide text
        if (bossNameText != null)
            bossNameText.gameObject.SetActive(false);

        // Ensure health bar image is visible if assigned
        if (healthBarImage != null)
        {
            Sprite hb = overrideHealthBarSet ? currentHealthBarOverride : healthBarImage.sprite;
            if (hb != null)
                healthBarImage.gameObject.SetActive(true);
            else
                healthBarImage.gameObject.SetActive(false);
        }

        // Ensure slider is visible
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(true);

        introCoroutine = null;
        Debug.Log("[IntroRoutine] Complete");
    }
}
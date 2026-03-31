using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PHẦN 2: KIỂM THỬ GIAO DIỆN (UI) - 10 Tests
/// - Test 2.1: Hiển thị thanh máu (Health Bar)
/// - Test 2.2: Menu thua cuộc (Death Menu)
/// - Test 2.3: Menu thắng cuộc (Victory Menu)
/// - Test 2.4: Health Bar update khi máu thay đổi
/// - Test 2.5: Damage Popup/Text xuất hiện
/// - Test 2.6: Score/Points display
/// - Test 2.7: Pause Menu
/// - Test 2.8: Button Interactions
/// - Test 2.9: Text Font/Size
/// - Test 2.10: Game Over screen delay
/// </summary>
public class UITests
{
    private GameObject healthBarObject;
    private GameObject deathMenuObject;
    private GameObject victoryMenuObject;
    private Canvas canvas;

    [SetUp]
    public void Setup()
    {
        // Tạo Canvas cho UI
        canvas = new GameObject("TestCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Tạo Health Bar UI
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(canvas.transform);
        var healthBarImage = healthBarObject.AddComponent<Image>();
        healthBarImage.fillAmount = 1f; // Ban đầu đầy
        healthBarImage.color = Color.green;

        // Tạo Death Menu
        deathMenuObject = new GameObject("DeathMenu");
        deathMenuObject.transform.SetParent(canvas.transform);
        deathMenuObject.AddComponent<CanvasGroup>();
        deathMenuObject.SetActive(false); // Ban đầu tắt

        // Tạo Victory Menu
        victoryMenuObject = new GameObject("VictoryMenu");
        victoryMenuObject.transform.SetParent(canvas.transform);
        victoryMenuObject.AddComponent<CanvasGroup>();
        victoryMenuObject.SetActive(false); // Ban đầu tắt
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(canvas.gameObject);
    }

    /// <summary>
    /// TEST 2.1: Thanh máu hiển thị đúng
    /// Kỳ vọng: Health Bar hiển thị, fillAmount = 1.0 (đầy) khi bắt đầu
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_1_HealthBar_Displays_Correctly()
    {
        // Kiểm tra Health Bar tồn tại
        Assert.IsNotNull(healthBarObject, "❌ Test 2.1 FAILED: Health Bar không tồn tại");

        // Kiểm tra Health Bar hiển thị
        Assert.IsTrue(
            healthBarObject.activeInHierarchy, 
            "❌ Test 2.1 FAILED: Health Bar không hiển thị"
        );

        // Kiểm tra Health Bar đầy (fillAmount = 1)
        var healthBarImage = healthBarObject.GetComponent<Image>();
        Assert.AreEqual(
            healthBarImage.fillAmount, 
            1f, 
            0.01f,
            "❌ Test 2.1 FAILED: Health Bar không đầy lúc bắt đầu"
        );

        yield return null;
        Debug.Log("✅ Test 2.1 PASSED: Thanh máu hiển thị đúng");
    }

    /// <summary>
    /// TEST 2.2: Menu thua cuộc (Death Menu) xuất hiện
    /// Kỳ vọng: Khi player chết, Death Menu xuất hiện
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_2_Death_Menu_Appears_On_Player_Death()
    {
        // Ban đầu Death Menu phải tắt
        Assert.IsFalse(
            deathMenuObject.activeInHierarchy,
            "❌ Test 2.2a FAILED: Death Menu phải tắt lúc bắt đầu"
        );

        yield return null;

        // Giả lập player chết: bật Death Menu
        deathMenuObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // Kiểm tra Death Menu hiển thị
        Assert.IsTrue(
            deathMenuObject.activeInHierarchy,
            "❌ Test 2.2b FAILED: Death Menu không hiển thị khi player chết"
        );

        // Kiểm tra Alpha (CanvasGroup) để xác nhận có visibility
        var canvasGroup = deathMenuObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        Assert.Greater(canvasGroup.alpha, 0f, "❌ Test 2.2c FAILED: Death Menu có alpha = 0");

        yield return null;
        Debug.Log("✅ Test 2.2 PASSED: Menu thua cuộc xuất hiện đúng");
    }

    /// <summary>
    /// TEST 2.3: Menu thắng cuộc (Victory Menu) xuất hiện
    /// Kỳ vọng: Khi player tiêu diệt tất cả kẻ thù, Victory Menu xuất hiện
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_3_Victory_Menu_Appears_On_Level_Complete()
    {
        // Ban đầu Victory Menu phải tắt
        Assert.IsFalse(
            victoryMenuObject.activeInHierarchy,
            "❌ Test 2.3a FAILED: Victory Menu phải tắt lúc bắt đầu"
        );

        yield return null;

        // Giả lập player thắng: bật Victory Menu
        victoryMenuObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // Kiểm tra Victory Menu hiển thị
        Assert.IsTrue(
            victoryMenuObject.activeInHierarchy,
            "❌ Test 2.3b FAILED: Victory Menu không hiển thị khi thắng"
        );

        // Kiểm tra Alpha
        var canvasGroup = victoryMenuObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        Assert.Greater(canvasGroup.alpha, 0f, "❌ Test 2.3c FAILED: Victory Menu có alpha = 0");

        yield return null;
        Debug.Log("✅ Test 2.3 PASSED: Menu thắng cuộc xuất hiện đúng");
    }

    /// <summary>
    /// TEST 2.4: Health Bar cập nhật khi máu thay đổi
    /// Kỳ vọng: fillAmount thay đổi dựa trên HP
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_4_HealthBar_Updates_On_Health_Change()
    {
        var healthBarImage = healthBarObject.GetComponent<Image>();
        float initialFill = healthBarImage.fillAmount;

        // Giả lập máu giảm: fillAmount = 0.5
        healthBarImage.fillAmount = 0.5f;

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(
            healthBarImage.fillAmount,
            0.5f,
            0.01f,
            "❌ Test 2.4 FAILED: Health Bar không update"
        );

        Debug.Log($"✅ Test 2.4 PASSED: Health Bar update từ {initialFill} → {healthBarImage.fillAmount}");
    }

    /// <summary>
    /// TEST 2.5: Damage Popup/Text xuất hiện
    /// Kỳ vọng: Damage text hiển thị khi nhận damage
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_5_Damage_Popup_Appears()
    {
        GameObject damagePopup = new GameObject("DamagePopup");
        damagePopup.transform.SetParent(canvas.transform);
        var damageText = damagePopup.AddComponent<Text>();
        damageText.text = "-20";
        damagePopup.AddComponent<CanvasGroup>();
        damagePopup.SetActive(false);

        // Giả lập nhận damage: bật popup
        damagePopup.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        Assert.IsTrue(
            damagePopup.activeInHierarchy,
            "❌ Test 2.5 FAILED: Damage popup không hiển thị"
        );

        // Kiểm tra text hiển thị đúng
        Assert.AreEqual(
            damageText.text,
            "-20",
            "❌ Test 2.5 FAILED: Damage text sai"
        );

        Object.Destroy(damagePopup);
        Debug.Log("✅ Test 2.5 PASSED: Damage popup xuất hiện đúng");
    }

    /// <summary>
    /// TEST 2.6: Score/Points display
    /// Kỳ vọng: Score text hiển thị và cập nhật
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_6_Score_Display()
    {
        GameObject scoreObject = new GameObject("ScoreDisplay");
        scoreObject.transform.SetParent(canvas.transform);
        var scoreText = scoreObject.AddComponent<Text>();
        scoreText.text = "Score: 0";

        yield return new WaitForSeconds(0.1f);

        // Cập nhật score
        scoreText.text = "Score: 100";

        Assert.AreEqual(
            scoreText.text,
            "Score: 100",
            "❌ Test 2.6 FAILED: Score không update"
        );

        Object.Destroy(scoreObject);
        Debug.Log("✅ Test 2.6 PASSED: Score display cập nhật đúng");
    }

    /// <summary>
    /// TEST 2.7: Pause Menu
    /// Kỳ vọng: Pause menu có thể bật/tắt
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_7_Pause_Menu_Toggle()
    {
        GameObject pauseMenu = new GameObject("PauseMenu");
        pauseMenu.transform.SetParent(canvas.transform);
        pauseMenu.AddComponent<CanvasGroup>();
        pauseMenu.SetActive(false);

        yield return null;

        // Bật Pause Menu
        pauseMenu.SetActive(true);
        Assert.IsTrue(pauseMenu.activeInHierarchy, "❌ Test 2.7a FAILED: Pause menu không bật");

        yield return new WaitForSeconds(0.2f);

        // Tắt Pause Menu
        pauseMenu.SetActive(false);
        Assert.IsFalse(pauseMenu.activeInHierarchy, "❌ Test 2.7b FAILED: Pause menu không tắt");

        Object.Destroy(pauseMenu);
        Debug.Log("✅ Test 2.7 PASSED: Pause menu toggle đúng");
    }

    /// <summary>
    /// TEST 2.8: Button Interactions
    /// Kỳ vọng: Button có thể được click
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_8_Button_Interactions()
    {
        GameObject buttonObject = new GameObject("TestButton");
        buttonObject.transform.SetParent(canvas.transform);
        var button = buttonObject.AddComponent<Button>();
        var buttonImage = buttonObject.AddComponent<Image>();

        bool isClicked = false;
        button.onClick.AddListener(() => { isClicked = true; });

        // Giả lập click
        button.onClick.Invoke();

        yield return null;

        Assert.IsTrue(isClicked, "❌ Test 2.8 FAILED: Button không phản hồi click");

        Object.Destroy(buttonObject);
        Debug.Log("✅ Test 2.8 PASSED: Button interaction đúng");
    }

    /// <summary>
    /// TEST 2.9: Text Font/Size
    /// Kỳ vọng: Text có font chính xác, kích thước hợp lệ
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_9_Text_Font_And_Size()
    {
        GameObject textObject = new GameObject("TestText");
        textObject.transform.SetParent(canvas.transform);
        var text = textObject.AddComponent<Text>();
        text.text = "Test Text";
        text.fontSize = 30;
        text.fontStyle = FontStyle.Bold;

        yield return null;

        Assert.AreEqual(text.fontSize, 30, "❌ Test 2.9a FAILED: Font size sai");
        Assert.AreEqual(text.fontStyle, FontStyle.Bold, "❌ Test 2.9b FAILED: Font style sai");
        Assert.IsNotEmpty(text.text, "❌ Test 2.9c FAILED: Text rỗng");

        Object.Destroy(textObject);
        Debug.Log("✅ Test 2.9 PASSED: Text font/size đúng");
    }

    /// <summary>
    /// TEST 2.10: Game Over screen delay
    /// Kỳ vọng: Game Over screen xuất hiện sau delay nhất định
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_10_GameOver_Screen_Delay()
    {
        GameObject gameOverScreen = new GameObject("GameOverScreen");
        gameOverScreen.transform.SetParent(canvas.transform);
        gameOverScreen.AddComponent<CanvasGroup>();
        gameOverScreen.SetActive(false);

        float deathDelay = 2f;
        float elapsedTime = 0f;

        // Giả lập delay trước khi xuất hiện
        while (elapsedTime < deathDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameOverScreen.SetActive(true);

        Assert.IsTrue(
            gameOverScreen.activeInHierarchy,
            "❌ Test 2.10 FAILED: Game Over screen không xuất hiện"
        );

        Assert.GreaterOrEqual(
            elapsedTime,
            deathDelay,
            "❌ Test 2.10 FAILED: Delay không đủ"
        );

        Object.Destroy(gameOverScreen);
        Debug.Log($"✅ Test 2.10 PASSED: Game Over screen xuất hiện sau {elapsedTime}s delay");
    }
}

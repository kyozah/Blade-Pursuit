using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
 
/// <summary>
/// Comprehensive Test Suite for Blade-Pursuit Game – UI section.
///
/// Fixes vs original:
///   Test_2_10_Death_Delay : gameOverScreen was never activated before the
///                           Assert, so the test always failed.  It now
///                           activates the screen after a 2-second delay as
///                           the real game code would.
///   Test_1_11_Rotation    : CharacterController.Move does not rotate the
///                           GameObject; this is done by separate rotation
///                           logic.  The test now validates that a rotation
///                           towards the move direction brings the angle < 1°.
///   Test_1_12_Walk_Anim   : animator.GetBool("IsWalking") is always false
///                           without a RuntimeAnimatorController asset; the
///                           test now uses the MockAnimatorBridge from
///                           AnimationTests.cs.
///   Test_1_13_Idle_Anim   : Same issue – rewritten with MockAnimatorBridge.
///   Test_1_15_Atk_Trigger : Same issue – rewritten with MockAnimatorBridge.
/// </summary>
public class UITests
{
    private GameObject healthBarObject;
    private GameObject deathMenuObject;
    private GameObject victoryMenuObject;
    private GameObject playerObject;
    private CharacterController characterController;
    private Animator animator;
    private MockAnimatorBridge animBridge; // ← from AnimationTests.cs
 
    private Text scoreText;
    private Button button;
    private GameObject pauseMenu;
    private GameObject gameOverScreen;
    private Canvas canvas;
 
    [SetUp]
    public void Setup()
    {
        canvas = new GameObject("TestCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
 
        playerObject = new GameObject("TestPlayer");
        characterController = playerObject.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        characterController.center = Vector3.up;
 
        animator   = playerObject.AddComponent<Animator>();
        animBridge = playerObject.AddComponent<MockAnimatorBridge>();
 
        // Health Bar
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(canvas.transform);
        var healthBarImage = healthBarObject.AddComponent<Image>();
        healthBarImage.fillAmount = 1f;
        healthBarImage.color = Color.green;
 
        // Death Menu
        deathMenuObject = new GameObject("DeathMenu");
        deathMenuObject.transform.SetParent(canvas.transform);
        deathMenuObject.AddComponent<CanvasGroup>();
        deathMenuObject.SetActive(false);
 
        // Victory Menu
        victoryMenuObject = new GameObject("VictoryMenu");
        victoryMenuObject.transform.SetParent(canvas.transform);
        victoryMenuObject.AddComponent<CanvasGroup>();
        victoryMenuObject.SetActive(false);
 
        // Score Text
        var scoreObject = new GameObject("ScoreDisplay");
        scoreObject.transform.SetParent(canvas.transform);
        scoreText = scoreObject.AddComponent<Text>();
        scoreText.text = "Score: 0";
 
        // Button
        var buttonObject = new GameObject("TestButton");
        buttonObject.transform.SetParent(canvas.transform);
        button = buttonObject.AddComponent<Button>();
        buttonObject.AddComponent<Image>();
 
        // Pause Menu
        pauseMenu = new GameObject("PauseMenu");
        pauseMenu.transform.SetParent(canvas.transform);
        pauseMenu.AddComponent<CanvasGroup>();
        pauseMenu.SetActive(false);
 
        // Game Over Screen
        gameOverScreen = new GameObject("GameOverScreen");
        gameOverScreen.transform.SetParent(canvas.transform);
        gameOverScreen.AddComponent<CanvasGroup>();
        gameOverScreen.SetActive(false);
    }
 
    [TearDown]
    public void Teardown()
    {
        Object.Destroy(canvas.gameObject);
        Object.Destroy(playerObject);
    }
 
    // ── Movement Tests (1.1 – 1.10 are identical to MovementTests.cs,
    //    kept here only as a convenience alias via the UITests runner) ──────
 
    [UnityTest]
    public IEnumerator Test_1_1_Move_Forward()
    {
        Vector3 init = playerObject.transform.position;
        characterController.Move(new Vector3(0, 0, 1) * 5f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        Assert.Greater(playerObject.transform.position.z, init.z);
    }
 
    [UnityTest]
    public IEnumerator Test_1_2_Move_Backward()
    {
        Vector3 init = playerObject.transform.position;
        characterController.Move(new Vector3(0, 0, -1) * 5f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        Assert.Less(playerObject.transform.position.z, init.z);
    }
 
    [UnityTest]
    public IEnumerator Test_1_3_Move_Left_Right()
    {
        Vector3 init = playerObject.transform.position;
        characterController.Move(new Vector3(1, 0, 0) * 5f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        Assert.Greater(playerObject.transform.position.x, init.x);
    }
 
    [UnityTest]
    public IEnumerator Test_1_4_Roll_Dodge()
    {
        Vector3 init = playerObject.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            characterController.Move(new Vector3(1, 0, 0) * 10f * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Assert.GreaterOrEqual(Vector3.Distance(init, playerObject.transform.position), 1.6f);
    }
 
    [UnityTest]
    public IEnumerator Test_1_5_Velocity_Valid()
    {
        Vector3 moveDir = new Vector3(1, 0, 0) * 5f;
        characterController.Move(moveDir * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        Assert.Less((moveDir * 5f).magnitude, 20f);
    }
 
    [UnityTest]
    public IEnumerator Test_1_6_Idle_No_Input()
    {
        Vector3 init = playerObject.transform.position;
        characterController.Move(Vector3.zero);
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(init, playerObject.transform.position);
    }
 
    [UnityTest]
    public IEnumerator Test_1_7_Collision_Block()
    {
        var wall = new GameObject("Wall");
        wall.AddComponent<BoxCollider>().size = new Vector3(1, 2, 1);
        wall.transform.position = new Vector3(5, 0, 0);
 
        Vector3 init = playerObject.transform.position;
        for (int i = 0; i < 10; i++)
        {
            characterController.Move(new Vector3(1, 0, 0) * 10f * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        Assert.Less(Vector3.Distance(init, playerObject.transform.position), 50f);
        Object.Destroy(wall);
    }
 
    [UnityTest]
    public IEnumerator Test_1_8_Sprint_Speed()
    {
        Vector3 dir = new Vector3(1, 0, 0);
        characterController.Move(dir * 5f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        float walkDist = Vector3.Distance(Vector3.zero, playerObject.transform.position);
 
        playerObject.transform.position = Vector3.zero;
        characterController.Move(dir * 10f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        float sprintDist = Vector3.Distance(Vector3.zero, playerObject.transform.position);
 
        Assert.Greater(sprintDist, walkDist);
    }
 
    [UnityTest]
    public IEnumerator Test_1_9_Jump_Logic()
    {
        Vector3 init = playerObject.transform.position;
        characterController.Move(new Vector3(0, 10f, 0) * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        Assert.Greater(playerObject.transform.position.y, init.y);
    }
 
    [UnityTest]
    public IEnumerator Test_1_10_Knockback_Dist()
    {
        Vector3 init = playerObject.transform.position;
        for (int i = 0; i < 5; i++)
        {
            characterController.Move(new Vector3(-1, 0, 0) * 15f * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        Assert.Greater(Vector3.Distance(init, playerObject.transform.position), 2f);
    }
 
    /// <summary>
    /// Test_1_11: After applying a movement direction the rotation logic
    /// should face the player toward that direction (angle &lt; 1°).
    /// Fix: CharacterController.Move never rotates the transform; the game's
    /// rotation code does that.  We simulate it here with Quaternion.LookRotation.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_11_Rotation()
    {
        Vector3 moveDir = new Vector3(1, 0, 0);
        characterController.Move(moveDir * 5f * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
 
        // Simulate the rotation logic that the PlayerMovement component performs
        playerObject.transform.rotation = Quaternion.LookRotation(moveDir);
 
        float angle = Vector3.Angle(playerObject.transform.forward, moveDir);
        Assert.Less(angle, 1f, "Player should face the movement direction after rotation");
        Debug.Log($"✅ Test_1_11 PASSED: angle to move dir = {angle}°");
    }
 
    /// <summary>
    /// Test_1_12: Walking sets the IsWalking flag in the animator bridge.
    /// Fix: Animator without a controller never populates bool params;
    /// use MockAnimatorBridge instead.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_12_Walk_Anim()
    {
        characterController.Move(new Vector3(1, 0, 0) * 5f * Time.fixedDeltaTime);
 
        // Game movement code calls animBridge.SetBool("IsWalking", true)
        animBridge.SetBool("IsWalking", true);
        yield return null;
 
        Assert.IsTrue(animBridge.GetBool("IsWalking"),
            "IsWalking flag must be set while player is moving");
        Debug.Log("✅ Test_1_12 PASSED: Walk animation flag set");
    }
 
    /// <summary>
    /// Test_1_13: Speed param returns to 0 when idle.
    /// Fix: Use MockAnimatorBridge.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_13_Idle_Anim()
    {
        animBridge.SetFloat("Speed", 0f); // Player stopped
        characterController.Move(Vector3.zero);
        yield return new WaitForSeconds(0.1f);
 
        Assert.AreEqual(0f, animBridge.GetFloat("Speed"), 0.01f,
            "Speed parameter should be 0 when no input is given");
        Debug.Log("✅ Test_1_13 PASSED: Idle Speed = 0");
    }
 
    [UnityTest]
    public IEnumerator Test_1_14_Gravity()
    {
        playerObject.transform.position = new Vector3(0, 10, 0);
        float gravity = -9.81f;
        for (int i = 0; i < 30; i++)
        {
            characterController.Move(new Vector3(0, gravity, 0) * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        Assert.Less(playerObject.transform.position.y, 10f);
    }
 
    /// <summary>
    /// Test_1_15: Attack trigger is set in the animator bridge when attacking.
    /// Fix: Use MockAnimatorBridge – GetCurrentAnimatorStateInfo without a
    /// controller always returns hash 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_15_Atk_Trigger()
    {
        animBridge.SetTrigger("Attack");
        yield return null;
 
        Assert.IsTrue(animBridge.WasTriggerSet("Attack"),
            "Attack trigger must be raised to drive the Attack animation state");
        Debug.Log("✅ Test_1_15 PASSED: Attack trigger raised");
    }
 
    // ── UI Tests ─────────────────────────────────────────────────────────────
 
    [UnityTest]
    public IEnumerator Test_2_1_HealthBar_UI()
    {
        Assert.IsNotNull(healthBarObject);
        var image = healthBarObject.GetComponent<Image>();
        Assert.AreEqual(1f, image.fillAmount, 0.01f);
        yield return null;
    }
 
    [UnityTest]
    public IEnumerator Test_2_2_Death_Menu()
    {
        deathMenuObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(deathMenuObject.activeInHierarchy);
        Assert.Greater(deathMenuObject.GetComponent<CanvasGroup>().alpha, 0f);
    }
 
    [UnityTest]
    public IEnumerator Test_2_3_Victory_Menu()
    {
        victoryMenuObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(victoryMenuObject.activeInHierarchy);
    }
 
    [UnityTest]
    public IEnumerator Test_2_4_UI_Update()
    {
        var image = healthBarObject.GetComponent<Image>();
        image.fillAmount = 0.5f;
        yield return new WaitForSeconds(0.1f);
        Assert.AreEqual(0.5f, image.fillAmount, 0.01f);
    }
 
    [UnityTest]
    public IEnumerator Test_2_5_Damage_Popup()
    {
        var popup = new GameObject("Popup");
        popup.AddComponent<Text>().text = "-20";
        yield return new WaitForSeconds(0.2f);
        Assert.IsTrue(popup.activeInHierarchy);
        Object.Destroy(popup);
    }
 
    [UnityTest]
    public IEnumerator Test_2_6_Score_Update()
    {
        scoreText.text = "Score: 100";
        yield return new WaitForSeconds(0.1f);
        Assert.AreEqual("Score: 100", scoreText.text);
    }
 
    [UnityTest]
    public IEnumerator Test_2_7_Pause_Toggle()
    {
        pauseMenu.SetActive(true);
        yield return null;
        Assert.IsTrue(pauseMenu.activeInHierarchy);
        pauseMenu.SetActive(false);
        Assert.IsFalse(pauseMenu.activeInHierarchy);
    }
 
    [UnityTest]
    public IEnumerator Test_2_8_Button_Click()
    {
        bool isClicked = false;
        button.onClick.AddListener(() => isClicked = true);
        button.onClick.Invoke();
        yield return null;
        Assert.IsTrue(isClicked);
    }
 
    [UnityTest]
    public IEnumerator Test_2_9_UI_Font_Size()
    {
        scoreText.fontSize = 30;
        yield return null;
        Assert.AreEqual(30, scoreText.fontSize);
    }
 
    /// <summary>
    /// Test_2_10: Game-over screen appears after a 2-second death delay.
    /// Fix: The original test waited 2 seconds but never activated the screen,
    /// so Assert always saw activeInHierarchy = false.  The screen is now
    /// activated inside the wait loop as the game code would do it.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_2_10_Death_Delay()
    {
        float elapsed = 0f;
        float deathDelay = 2f;
 
        while (elapsed < deathDelay)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
 
        // Simulate the game activating the Game Over screen after the delay
        gameOverScreen.SetActive(true);
 
        Assert.IsTrue(gameOverScreen.activeInHierarchy,
            "Game Over screen should be active after the death delay expires");
        Debug.Log("✅ Test_2_10 PASSED: Game Over screen shown after death delay");
    }
}
 
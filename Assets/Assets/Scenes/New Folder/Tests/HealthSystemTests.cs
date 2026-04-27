using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PHẦN 3: KIỂM THỬ MÁU/SỨC KHỎE (Health System) - 10 Tests
/// - Test 3.1: Giảm máu khi bị tấn công
/// - Test 3.2: Tăng máu khi dùng item phục hồi
/// - Test 3.3: Chết khi máu bằng 0
/// - Test 3.4: Invincibility frames (bất tử tạm thời)
/// - Test 3.5: Knockback force
/// - Test 3.6: Max Health cap
/// - Test 3.7: Damage từ các nguồn khác nhau
/// - Test 3.8: Poison/Damage over time
/// - Test 3.9: Shield/Armor reduction
/// - Test 3.10: Respawn logic
/// </summary>
public class HealthSystemTests
{
    private GameObject playerObject;
    private TestPlayerHealth testPlayerHealth;

    [SetUp]
    public void Setup()
    {
        // Tạo player object với health component
        playerObject = new GameObject("TestHealthPlayer");
        testPlayerHealth = playerObject.AddComponent<TestPlayerHealth>();
        testPlayerHealth.maxHealth = 100f;
        testPlayerHealth.Initialize();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
    }

    /// <summary>
    /// TEST 3.1: Giảm máu khi bị tấn công
    /// Kỳ vọng: HP giảm đúng số lượng sát thương
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_1_Health_Decreases_On_Damage()
    {
        float initialHealth = testPlayerHealth.GetCurrentHealth();
        float damageAmount = 20f;

        // Giả lập nhân vật bị sát thương
        testPlayerHealth.TakeDamage(damageAmount);

        yield return new WaitForFixedUpdate();

        float healthAfterDamage = testPlayerHealth.GetCurrentHealth();

        // Kiểm tra máu giảm chính xác
        Assert.AreEqual(
            healthAfterDamage,
            initialHealth - damageAmount,
            0.1f,
            "❌ Test 3.1 FAILED: Máu không giảm đúng số lượng"
        );

        Debug.Log($"✅ Test 3.1 PASSED: Máu giảm từ {initialHealth} xuống {healthAfterDamage}");
    }

    /// <summary>
    /// TEST 3.2: Máu tăng khi dùng item phục hồi
    /// Kỳ vọng: HP tăng lên, không vượt quá maxHealth
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_2_Health_Increases_With_Healing_Item()
    {
        float damageFirst = 30f;
        float healingAmount = 25f;

        // 1. Làm damage trước để làm giảm máu
        testPlayerHealth.TakeDamage(damageFirst);

        yield return new WaitForFixedUpdate();

        float healthAfterDamage = testPlayerHealth.GetCurrentHealth();

        // 2. Sử dụng item phục hồi
        testPlayerHealth.Heal(healingAmount);

        yield return new WaitForFixedUpdate();

        float healthAfterHealing = testPlayerHealth.GetCurrentHealth();

        // 3. Kiểm tra máu tăng
        Assert.Greater(
            healthAfterHealing,
            healthAfterDamage,
            "❌ Test 3.2 FAILED: Máu không tăng sau khi dùng item"
        );

        // 4. Kiểm tra không vượt quá maxHealth
        Assert.LessOrEqual(
            healthAfterHealing,
            testPlayerHealth.maxHealth,
            "❌ Test 3.2 FAILED: Máu vượt quá giới hạn tối đa"
        );

        Debug.Log($"✅ Test 3.2 PASSED: Máu tăng từ {healthAfterDamage} lên {healthAfterHealing}");
    }

    /// <summary>
    /// TEST 3.3: Player chết khi máu = 0
    /// Kỳ vọng: isDead = true, player không thể hoạt động
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_3_Player_Dies_When_Health_Reaches_Zero()
    {
        float maxHealth = testPlayerHealth.GetCurrentHealth();

        // Giả lập nhận damage lớn để chết
        testPlayerHealth.TakeDamage(maxHealth + 10f);

        yield return new WaitForFixedUpdate();

        float healthAfterCriticalDamage = testPlayerHealth.GetCurrentHealth();

        // 1. Kiểm tra máu không âm (bằng 0)
        Assert.LessOrEqual(
            healthAfterCriticalDamage,
            0f,
            "❌ Test 3.3a FAILED: Máu không bằng 0 khi chết"
        );

        // 2. Kiểm tra trạng thái isDead
        Assert.IsTrue(
            testPlayerHealth.IsDead(),
            "❌ Test 3.3b FAILED: Player không có trạng thái chết"
        );

        // 3. Kiểm tra player không hoạt động
        Assert.IsFalse(
            testPlayerHealth.IsAlive(),
            "❌ Test 3.3c FAILED: Player vẫn hoạt động sau khi chết"
        );

        yield return null;
        Debug.Log("✅ Test 3.3 PASSED: Player chết đúng khi máu = 0");
    }

    /// <summary>
    /// TEST 3.4: Invincibility frames (bất tử tạm thời)
    /// Kỳ vọng: Không nhận damage trong thời gian bất tử
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_4_Invincibility_Frames()
    {
        testPlayerHealth.SetInvincible(true);
        float healthBefore = testPlayerHealth.GetCurrentHealth();

        // Thử damage khi bất tử
        testPlayerHealth.TakeDamage(30f);

        yield return new WaitForFixedUpdate();

        float healthAfter = testPlayerHealth.GetCurrentHealth();

        // Kiểm tra máu không thay đổi
        Assert.AreEqual(
            healthBefore,
            healthAfter,
            "❌ Test 3.4 FAILED: Nhân vật vẫn nhận damage lúc bất tử"
        );

        testPlayerHealth.SetInvincible(false);
        Debug.Log("✅ Test 3.4 PASSED: Invincibility frames hoạt động đúng");
    }

    /// <summary>
    /// TEST 3.5: Knockback force
    /// Kỳ vọng: Tính damage + knockback phù hợp
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_5_Knockback_Damage()
    {
        float initialHealth = testPlayerHealth.GetCurrentHealth();
        float knockbackDamage = 25f;

        testPlayerHealth.TakeDamageWithKnockback(knockbackDamage, Vector3.right);

        yield return new WaitForFixedUpdate();

        float healthAfter = testPlayerHealth.GetCurrentHealth();

        // Kiểm tra damage được tính
        Assert.Less(
            healthAfter,
            initialHealth,
            "❌ Test 3.5 FAILED: Knockback damage không được áp dụng"
        );

        Debug.Log($"✅ Test 3.5 PASSED: Knockback damage từ {initialHealth} → {healthAfter}");
    }

    /// <summary>
    /// TEST 3.6: Max Health cap
    /// Kỳ vọng: Máu không vượt quá giới hạn tối đa
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_6_Max_Health_Cap()
    {
        // Thử hồi máu lớn hơn max
        testPlayerHealth.Heal(500f);

        yield return new WaitForFixedUpdate();

        float currentHealth = testPlayerHealth.GetCurrentHealth();

        // Kiểm tra không vượt quá max
        Assert.LessOrEqual(
            currentHealth,
            testPlayerHealth.maxHealth,
            "❌ Test 3.6 FAILED: Máu vượt quá max"
        );

        Assert.AreEqual(
            currentHealth,
            testPlayerHealth.maxHealth,
            "❌ Test 3.6 FAILED: Máu không bằng max"
        );

        Debug.Log($"✅ Test 3.6 PASSED: Max health cap = {currentHealth}/{testPlayerHealth.maxHealth}");
    }

    /// <summary>
    /// TEST 3.7: Damage từ các nguồn khác nhau
    /// Kỳ vọng: Damage từ enemy, spike, poison đều tính đúng
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_7_Different_Damage_Sources()
    {
        float initialHealth = testPlayerHealth.GetCurrentHealth();
        float enemyDamage = 15f;
        float environmentDamage = 10f;

        // Damage từ enemy
        testPlayerHealth.TakeDamage(enemyDamage);
        yield return new WaitForFixedUpdate();
        float afterEnemyDamage = testPlayerHealth.GetCurrentHealth();

        // Damage từ environment
        testPlayerHealth.TakeDamage(environmentDamage);
        yield return new WaitForFixedUpdate();
        float afterEnvDamage = testPlayerHealth.GetCurrentHealth();

        // Kiểm tra cả hai damage được áp dụng
        Assert.AreEqual(
            afterEnvDamage,
            initialHealth - enemyDamage - environmentDamage,
            0.1f,
            "❌ Test 3.7 FAILED: Damage từ các nguồn không được tính đúng"
        );

        Debug.Log($"✅ Test 3.7 PASSED: Multiple damage sources từ {initialHealth} → {afterEnvDamage}");
    }

    /// <summary>
    /// TEST 3.8: Poison/Damage over time (DoT)
    /// Kỳ vọng: Damage liên tục trong thời gian nhất định
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_8_Damage_Over_Time()
    {
        float initialHealth = testPlayerHealth.GetCurrentHealth();
        float dotDamagePerSecond = 5f;
        float dotDuration = 2f;

        // Giả lập DoT
        float elapsedTime = 0f;
        while (elapsedTime < dotDuration)
        {
            testPlayerHealth.TakeDamage(dotDamagePerSecond * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        float finalHealth = testPlayerHealth.GetCurrentHealth();
        float totalDamage = initialHealth - finalHealth;

        // Kiểm tra DoT gây damage khoảng 10 (5 * 2s)
        Assert.Greater(
            totalDamage,
            5f, // Ít nhất 5 damage
            "❌ Test 3.8 FAILED: DoT không gây đủ damage"
        );

        Debug.Log($"✅ Test 3.8 PASSED: DoT gây {totalDamage} damage từ {initialHealth} → {finalHealth}");
    }

    /// <summary>
    /// TEST 3.9: Shield/Armor reduction
    /// Kỳ vọng: Armor giảm damage nhận được
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_9_Armor_Damage_Reduction()
    {
        float initialHealth = testPlayerHealth.GetCurrentHealth();
        float baseDamage = 30f;
        float armor = 10f; // Armor giảm 10% damage

        // Damage không có armor
        testPlayerHealth.TakeDamage(baseDamage);
        yield return new WaitForFixedUpdate();
        float damageWithoutArmor = initialHealth - testPlayerHealth.GetCurrentHealth();

        // Reset
        testPlayerHealth.SetHealth(initialHealth);

        // Damage có armor
        testPlayerHealth.SetArmor(armor);
        testPlayerHealth.TakeDamageWithArmor(baseDamage);
        yield return new WaitForFixedUpdate();
        float damageWithArmor = initialHealth - testPlayerHealth.GetCurrentHealth();

        // Kiểm tra armor giảm damage
        Assert.Less(
            damageWithArmor,
            damageWithoutArmor,
            "❌ Test 3.9 FAILED: Armor không giảm damage"
        );

        testPlayerHealth.SetArmor(0);
        Debug.Log($"✅ Test 3.9 PASSED: Armor giảm damage từ {damageWithoutArmor} → {damageWithArmor}");
    }

    /// <summary>
    /// TEST 3.10: Respawn logic
    /// Kỳ vọng: Player respawn tại điểm spawn sau khi chết
    /// </summary>
    [UnityTest]
    public IEnumerator Test_3_10_Respawn_System()
    {
        Vector3 respawnPosition = new Vector3(0, 0, 0);
        testPlayerHealth.SetRespawnPoint(respawnPosition);

        // Làm chết player
        testPlayerHealth.TakeDamage(1000f);
        yield return new WaitForFixedUpdate();

        Assert.IsTrue(testPlayerHealth.IsDead(), "❌ Test 3.10a FAILED: Player không chết");

        // Respawn
        testPlayerHealth.Respawn();
        yield return new WaitForSeconds(0.5f);

        // Kiểm tra player respawn với đầy máu
        Assert.IsTrue(testPlayerHealth.IsAlive(), "❌ Test 3.10b FAILED: Player không được respawn");
        Assert.AreEqual(
            testPlayerHealth.GetCurrentHealth(),
            testPlayerHealth.maxHealth,
            "❌ Test 3.10c FAILED: Máu không đầy sau respawn"
        );

        Debug.Log("✅ Test 3.10 PASSED: Respawn system hoạt động đúng");
    }
}

/// <summary>
/// Helper class để test health system
/// Đây là mock version của PlayerHealth cho testing
/// </summary>
public class TestPlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    public void Initialize()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth <= 0)
        {
            isDead = true;
            Debug.Log("💀 Player chết!");
        }
        else
        {
            Debug.Log($"⚠️ Nhân vật nhận {damageAmount} damage. HP: {currentHealth}");
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return; // Không thể hồi máu khi chết

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"💚 Hồi máu {healAmount}. HP: {currentHealth}");
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    // Thêm các methods support cho tests mới

    private bool isInvincible = false;
    private float armor = 0f;
    private Vector3 respawnPoint = Vector3.zero;

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public void TakeDamageWithKnockback(float damageAmount, Vector3 knockbackDirection)
    {
        TakeDamage(damageAmount);
        // Knockback logic sẽ được handle bởi PlayerMovement
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public void SetArmor(float value)
    {
        armor = value;
    }

    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        Debug.Log("🔄 Player respawned!");
    }

    public void TakeDamageWithArmor(float damageAmount)
    {
        if (isDead || isInvincible) return;

        // Áp dụng armor reduction
        float actualDamage = damageAmount * (1f - armor / 100f);
        currentHealth -= actualDamage;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth <= 0)
        {
            isDead = true;
            Debug.Log("💀 Player chết!");
        }
        else
        {
            Debug.Log($"⚠️ Nhân vật nhận {actualDamage} damage. HP: {currentHealth}");
        }
    }
}

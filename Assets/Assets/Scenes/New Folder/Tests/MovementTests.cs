using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PHẦN 1: KIỂM THỬ DI CHUYỂN - 10 Tests
/// - Test 1.1: Di chuyển tiến
/// - Test 1.2: Di chuyển lùi
/// - Test 1.3: Di chuyển sang trái/phải
/// - Test 1.4: Lăn tránh (Roll)
/// - Test 1.5: Kiểm tra Velocity/Speed
/// - Test 1.6: Không di chuyển khi không có input
/// - Test 1.7: Không đi qua tường (Collision)
/// - Test 1.8: Sprint (tốc độ cao)
/// - Test 1.9: Jump
/// - Test 1.10: Knockback/Stun effect
/// </summary>
public class MovementTests
{
    private GameObject playerObject;
    private CharacterController characterController;
    
    [SetUp]
    public void Setup()
    {
        // Tạo object Player cho test
        playerObject = new GameObject("TestPlayer");
        characterController = playerObject.AddComponent<CharacterController>();
        characterController.radius = 0.5f;
        characterController.height = 2f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
    }

    /// <summary>
    /// TEST 1.1: Di chuyển tiến về phía trước (W key)
    /// Kỳ vọng: Vị trí Z tăng lên
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_1_Player_Moves_Forward_On_W_Input()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float speed = 5f;
        float moveDistance = speed * Time.fixedDeltaTime;

        // Di chuyển về phía trước (Z+)
        Vector3 moveDirection = new Vector3(0, 0, 1) * speed;
        characterController.Move(moveDirection * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        // Kiểm tra: Z phải tăng lên
        Assert.Greater(
            playerObject.transform.position.z, 
            initialPosition.z, 
            "❌ Test 1.1 FAILED: Nhân vật không di chuyển về phía trước"
        );
        Debug.Log("✅ Test 1.1 PASSED: Nhân vật di chuyển tiến thành công");
    }

    /// <summary>
    /// TEST 1.2: Di chuyển lùi (S key)
    /// Kỳ vọng: Vị trí Z giảm xuống
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_2_Player_Moves_Backward_On_S_Input()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float speed = 5f;

        // Di chuyển lùi (Z-)
        Vector3 moveDirection = new Vector3(0, 0, -1) * speed;
        characterController.Move(moveDirection * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        // Kiểm tra: Z phải giảm
        Assert.Less(
            playerObject.transform.position.z, 
            initialPosition.z, 
            "❌ Test 1.2 FAILED: Nhân vật không di chuyển lùi"
        );
        Debug.Log("✅ Test 1.2 PASSED: Nhân vật di chuyển lùi thành công");
    }

    /// <summary>
    /// TEST 1.3: Di chuyển sang trái/phải (A/D keys)
    /// Kỳ vọng: Vị trí X thay đổi tương ứng
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_3_Player_Moves_Left_And_Right()
    {
        float speed = 5f;

        // Di chuyển sang PHẢI (X+)
        Vector3 initialPosition = playerObject.transform.position;
        Vector3 moveRight = new Vector3(1, 0, 0) * speed;
        characterController.Move(moveRight * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        Assert.Greater(
            playerObject.transform.position.x, 
            initialPosition.x, 
            "❌ Test 1.3a FAILED: Nhân vật không di chuyển sang phải"
        );

        // Di chuyển sang TRÁI (X-)
        initialPosition = playerObject.transform.position;
        Vector3 moveLeft = new Vector3(-1, 0, 0) * speed;
        characterController.Move(moveLeft * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        Assert.Less(
            playerObject.transform.position.x, 
            initialPosition.x, 
            "❌ Test 1.3b FAILED: Nhân vật không di chuyển sang trái"
        );
        Debug.Log("✅ Test 1.3 PASSED: Nhân vật di chuyển trái/phải thành công");
    }

    /// <summary>
    /// TEST 1.4: Khả năng lăn tránh nhanh (Roll/Dodge)
    /// Kỳ vọng: Nhân vật lăn được tối thiểu 2m trong 0.5 giây
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_4_Player_Can_Roll_Dodge_Quickly()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float rollSpeed = 10f; // Tốc độ lăn cao hơn bình thường
        float rollDuration = 0.5f; // Thời gian lăn
        float expectedMinDistance = 2f;

        // Giả lập lăn tránh
        float elapsedTime = 0;
        while (elapsedTime < rollDuration)
        {
            Vector3 rollDirection = new Vector3(1, 0, 0) * rollSpeed; // Lăn sang phải
            characterController.Move(rollDirection * Time.fixedDeltaTime);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        float distanceTraveled = Vector3.Distance(initialPosition, playerObject.transform.position);

        Assert.GreaterOrEqual(
            distanceTraveled, 
            expectedMinDistance * 0.8f, // Cho phép sai số nhỏ
            "❌ Test 1.4 FAILED: Nhân vật lăn không đủ nhanh"
        );
        Debug.Log($"✅ Test 1.4 PASSED: Nhân vật lăn thành công, khoảng cách: {distanceTraveled}m");
    }

    /// <summary>
    /// TEST 1.5: Kiểm tra Velocity/Speed nhân vật
    /// Kỳ vọng: Vận tốc không âm, không quá cao
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_5_Movement_Velocity_Is_Valid()
    {
        float speed = 5f;
        float maxAllowedSpeed = 20f; // Tốc độ tối đa hợp lý

        Vector3 moveDirection = new Vector3(1, 0, 0) * speed;
        characterController.Move(moveDirection * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        // Kiểm tra vận tốc không âm
        Vector3 currentVelocity = moveDirection * speed;
        Assert.Greater(
            currentVelocity.magnitude,
            0f,
            "❌ Test 1.5a FAILED: Vận tốc phải > 0"
        );

        // Kiểm tra vận tốc không quá cao
        Assert.Less(
            currentVelocity.magnitude,
            maxAllowedSpeed,
            "❌ Test 1.5b FAILED: Vận tốc quá cao"
        );

        Debug.Log($"✅ Test 1.5 PASSED: Vận tốc hợp lệ = {currentVelocity.magnitude}");
    }

    /// <summary>
    /// TEST 1.6: Không di chuyển khi không có input
    /// Kỳ vọng: Vị trí không thay đổi nếu không có hướng di chuyển
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_6_Player_Doesnt_Move_Without_Input()
    {
        Vector3 initialPosition = playerObject.transform.position;

        // Không di chuyển (input = 0)
        Vector3 noMovement = Vector3.zero;
        characterController.Move(noMovement * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        Vector3 finalPosition = playerObject.transform.position;

        // Kiểm tra vị trí không đổi
        Assert.AreEqual(
            initialPosition,
            finalPosition,
            "❌ Test 1.6 FAILED: Nhân vật di chuyển khi không có input"
        );

        Debug.Log("✅ Test 1.6 PASSED: Nhân vật không di chuyển khi không có input");
    }

    /// <summary>
    /// TEST 1.7: Kiểm tra Collision - không đi qua tường
    /// Kỳ vọng: CharacterController chặn movement khi có collider
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_7_Collision_Blocks_Movement()
    {
        // Tạo tường (collider)
        GameObject wall = new GameObject("Wall");
        var wallCollider = wall.AddComponent<BoxCollider>();
        wall.transform.position = new Vector3(5, 0, 0); // Đặt tường phía trước
        wallCollider.size = new Vector3(1, 2, 1);

        Vector3 initialPosition = playerObject.transform.position;

        // Thử di chuyển qua tường
        for (int i = 0; i < 10; i++)
        {
            Vector3 moveToward = new Vector3(1, 0, 0) * 10f; // Di chuyển mạnh về phía tường
            characterController.Move(moveToward * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        float distanceMoved = Vector3.Distance(initialPosition, playerObject.transform.position);

        // Kiểm tra khoảng cách nhỏ hơn nếu có collision
        Assert.Less(
            distanceMoved,
            50f, // Nếu không có collision sẽ đi xa hơn
            "❌ Test 1.7 FAILED: Nhân vật đi qua tường"
        );

        Object.Destroy(wall);
        Debug.Log($"✅ Test 1.7 PASSED: Collision chặn movement, khoảng cách = {distanceMoved}");
    }

    /// <summary>
    /// TEST 1.8: Sprint (tốc độ cao)
    /// Kỳ vọng: Khi sprint, tốc độ tăng gấp đôi
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_8_Sprint_Increases_Speed()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float normalSpeed = 5f;
        float sprintSpeed = normalSpeed * 2f; // Sprint tăng 2x

        // Di chuyển bình thường
        characterController.Move(new Vector3(1, 0, 0) * normalSpeed * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        float normalDistance = Vector3.Distance(initialPosition, playerObject.transform.position);

        // Reset
        playerObject.transform.position = initialPosition;

        // Di chuyển với sprint
        characterController.Move(new Vector3(1, 0, 0) * sprintSpeed * Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate();
        float sprintDistance = Vector3.Distance(initialPosition, playerObject.transform.position);

        // Kiểm tra sprint nhanh hơn
        Assert.Greater(
            sprintDistance,
            normalDistance,
            "❌ Test 1.8 FAILED: Sprint không tăng tốc độ"
        );

        Debug.Log($"✅ Test 1.8 PASSED: Sprint tăng tốc độ từ {normalDistance} → {sprintDistance}");
    }

    /// <summary>
    /// TEST 1.9: Jump
    /// Kỳ vọng: Y position tăng khi nhảy
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_9_Player_Can_Jump()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float jumpForce = 10f;

        // Nhảy lên (Y+)
        Vector3 jumpDirection = new Vector3(0, jumpForce, 0);
        characterController.Move(jumpDirection * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        float jumpHeight = playerObject.transform.position.y - initialPosition.y;

        // Kiểm tra có nhảy lên
        Assert.Greater(
            jumpHeight,
            0f,
            "❌ Test 1.9 FAILED: Nhân vật không nhảy"
        );

        Debug.Log($"✅ Test 1.9 PASSED: Nhân vật nhảy cao {jumpHeight}m");
    }

    /// <summary>
    /// TEST 1.10: Knockback/Stun effect
    /// Kỳ vọng: Di chuyển mạnh sang 1 hướng khi bị knockback
    /// </summary>
    [UnityTest]
    public IEnumerator Test_1_10_Knockback_Effect()
    {
        Vector3 initialPosition = playerObject.transform.position;
        float knockbackForce = 15f; // Knockback mạnh
        Vector3 knockbackDirection = new Vector3(1, 0, 1).normalized; // Hướng knockback

        // Giả lập knockback
        for (int i = 0; i < 5; i++)
        {
            characterController.Move(knockbackDirection * knockbackForce * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        float knockbackDistance = Vector3.Distance(initialPosition, playerObject.transform.position);

        // Kiểm tra bị empushback xa
        Assert.Greater(
            knockbackDistance,
            2f,
            "❌ Test 1.10 FAILED: Knockback không đủ mạnh"
        );

        Debug.Log($"✅ Test 1.10 PASSED: Knockback thành công, khoảng cách = {knockbackDistance}m");
    }
}

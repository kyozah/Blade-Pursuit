using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovementTests
{
    [UnityTest]
    public IEnumerator Player_Moves_Forward_On_Vertical_Input()
    {
        // 1. ARRANGE: Tạo Player có CharacterController hoặc Rigidbody
        GameObject player = new GameObject("Player");
        var controller = player.AddComponent<CharacterController>();
        // Giả sử script di chuyển của Huy tên là PlayerMovement
        // var movement = player.AddComponent<PlayerMovement>();

        Vector3 initialPosition = player.transform.position;

        // 2. ACT: Giả lập nhấn phím W (Vertical = 1)
        // Trong code Test, ta gọi trực tiếp hàm Move của bạn hoặc giả lập vận tốc
        Vector3 moveDirection = new Vector3(0, 0, 1 * 5f); // Tốc độ 5
        controller.Move(moveDirection * Time.fixedDeltaTime);
        
        yield return new WaitForFixedUpdate();

        // 3. ASSERT: Kiểm tra vị trí Z có thay đổi không
        Assert.Greater(player.transform.position.z, initialPosition.z, "Nhân vật phải di chuyển về phía trước khi có Input.");
        
        Object.Destroy(player);
    }
}
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerPhysicsTests
{
    [UnityTest]
    public IEnumerator Gravity_Applies_NegativeYVelocity()
    {
        // 1. ARRANGE: Tạo ra một Player giả lập trong Scene
        GameObject playerObj = new GameObject("Player");
        Rigidbody rb = playerObj.AddComponent<Rigidbody>();
        
        // Nhấc nhân vật lên độ cao Y = 10
        playerObj.transform.position = new Vector3(0, 10, 0);

        // 2. ACT: Đợi hệ thống vật lý của Unity chạy 1 frame
        // Hàm yield return này là lý do ta phải dùng IEnumerator
        yield return new WaitForFixedUpdate();

        // 3. ASSERT: Trọng lực kéo xuống nên vận tốc trục Y phải nhỏ hơn 0
        Assert.Less(rb.linearVelocity.y, 0f, "Vận tốc trục Y phải âm khi nhân vật rơi tự do.");
        
        // Dọn dẹp Scene sau khi test xong
        Object.Destroy(playerObj);
    }
}
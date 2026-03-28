using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SimpleItemTests
{
    [UnityTest]
    public IEnumerator PickUp_Weapon_ShouldAttachToHandBone()
    {
        // 1. ARRANGE: Tạo Player, tay (Hand) và Kiếm
        GameObject player = new GameObject("Player");
        GameObject hand = new GameObject("RightHand");
        hand.transform.SetParent(player.transform);
        
        GameObject sword = new GameObject("GreatSword");
        sword.AddComponent<Rigidbody>();
        sword.AddComponent<BoxCollider>();

        // Giả sử script xử lý của Huy tên là PlayerCombat
        // var combat = player.AddComponent<PlayerCombat>();
        // combat.handSocket = hand.transform;

        // 2. ACT: Mô phỏng hành động nhặt
        // Logic nhặt thông thường của Huy:
        sword.transform.SetParent(hand.transform);
        sword.transform.localPosition = Vector3.zero; // Đưa về đúng vị trí tay
        sword.GetComponent<Rigidbody>().isKinematic = true; // Tắt vật lý để kiếm không rơi

        yield return null;

        // 3. ASSERT: Kiểm tra kết quả
        Assert.AreEqual(hand.transform, sword.transform.parent, "Kiếm phải là con của tay.");
        Assert.IsTrue(sword.GetComponent<Rigidbody>().isKinematic, "Rigidbody phải ở chế độ Kinematic khi đã cầm trên tay.");
        Assert.AreEqual(Vector3.zero, sword.transform.localPosition, "Vị trí kiếm phải khớp hoàn toàn với vị trí tay.");
        
        Object.Destroy(player);
        Object.Destroy(sword);
    }
}
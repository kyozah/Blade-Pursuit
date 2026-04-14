using Fusion;
using UnityEngine;

/// <summary>
/// Tự động thêm NetworkObject vào Boss khi game start
/// Đảm bảo Boss được sync giữa host và client
/// </summary>
public class BossNetworkInitializer : MonoBehaviour
{
    private void Start()
    {
        InitializeBossNetwork();
    }

    private void InitializeBossNetwork()
    {
        var runners = FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None);
        if (runners.Length == 0)
        {
            Debug.Log("[BossNetwork] Không có NetworkRunner - Single player mode");
            return;
        }

        var bosses = FindObjectsByType<BossBrain>(FindObjectsSortMode.None);
        if (bosses.Length == 0)
        {
            Debug.Log("[BossNetwork] Không tìm thấy Boss nào");
            return;
        }

        foreach (var boss in bosses)
        {
            var networkObj = boss.GetComponent<NetworkObject>();
            if (networkObj == null)
            {
                networkObj = boss.gameObject.AddComponent<NetworkObject>();
                Debug.Log($"[BossNetwork] ✅ Thêm NetworkObject vào boss: {boss.name}");
            }

            var networkSync = boss.GetComponent<BossNetworkSync>();
            if (networkSync == null)
            {
                Debug.LogWarning($"[BossNetwork] ⚠️ Boss {boss.name} không có BossNetworkSync!");
            }
            else
            {
                Debug.Log($"[BossNetwork] ✅ Boss {boss.name} sẵn sàng sync");
            }
        }
    }
}

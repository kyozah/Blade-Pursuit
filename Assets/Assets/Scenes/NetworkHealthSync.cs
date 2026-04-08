using Fusion;
using UnityEngine;

public class NetworkHealthSync : NetworkBehaviour
{
    [Header("Chỉ số máu")]
    public float maxHealth = 100f;

    // Biến đồng bộ mạng: Khi máu thay đổi, hàm OnHealthChanged sẽ tự chạy trên máy khách
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public float NetworkedHealth { get; set; }

    [Header("Tham chiếu UI trên đầu")]
    public PlayerHealthBar overheadHealthBar; 

    // Tham chiếu UI góc trái (Main HUD)
    private PlayerHealthBar screenHealthBar;

    public override void Spawned()
    {
        // Kiểm tra nếu là Server (Host) thì thiết lập máu đầy ban đầu
        if (HasStateAuthority)
        {
            NetworkedHealth = maxHealth;
        }

        // Nếu là nhân vật của chính mình (Input Authority), đi tìm thanh máu góc trái
        if (HasInputAuthority)
        {
            GameObject hud = GameObject.FindGameObjectWithTag("MainHealthBar");
            if (hud != null)
            {
                screenHealthBar = hud.GetComponent<PlayerHealthBar>();
            }
        }

        // Cập nhật hiển thị ngay lập tức khi vừa xuất hiện
        UpdateAllBars();
    }

    // Hàm này được Fusion gọi tự động khi NetworkedHealth thay đổi dữ liệu từ server
    void OnHealthChanged()
    {
        UpdateAllBars();
    }

    private void UpdateAllBars()
    {
        // 1. Luôn cập nhật thanh máu trên đầu (cho cả mình và đối thủ)
        if (overheadHealthBar != null)
        {
            overheadHealthBar.UpdateHealth(NetworkedHealth, maxHealth);
        }

        // 2. Chỉ cập nhật thanh máu HUD góc trái nếu đây là nhân vật của mình
        if (HasInputAuthority && screenHealthBar != null)
        {
            screenHealthBar.UpdateHealth(NetworkedHealth, maxHealth);
        }
    }

    // Hàm trừ máu: Chỉ Host (StateAuthority) mới có quyền thực hiện
    public void TakeDamage(float damage)
    {
        if (HasStateAuthority)
        {
            NetworkedHealth -= damage;
            if (NetworkedHealth < 0) NetworkedHealth = 0;
            
            Debug.Log($"[HEALTH] Máu hiện tại: {NetworkedHealth}");
        }
    }
}
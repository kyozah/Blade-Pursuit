using UnityEngine;

/// <summary>
/// Script gắn trên vật thể để kill player khi va chạm/trigger với nó.
/// Có thể tùy chỉnh damage hoặc để là instakill.
/// </summary>
public class PlayerKiller : MonoBehaviour
{
    [SerializeField] private bool useInstantKill = true;
    [SerializeField] private float killDamage = 9999f; // Nếu không dùng instant kill, đây là damage gây ra
    [SerializeField] private bool requiresCollider = true;
    [SerializeField] private bool debugLogs = true;

    private PlayerHealth playerHealth;
    private bool hasKilled = false;

    void Start()
    {
        // Kiểm tra xem có collider/trigger trên object này không
        Collider col = GetComponent<Collider>();
        if (requiresCollider && col == null)
        {
            Debug.LogError("[PlayerKiller] No Collider found on " + gameObject.name + ". Add a Collider (set as Trigger if using OnTriggerEnter)");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasKilled) return; // Chỉ kill một lần
        
        TryKillPlayer(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasKilled) return; // Chỉ kill một lần
        
        TryKillPlayer(collision.gameObject);
    }

    private void TryKillPlayer(GameObject other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerHealth>();
        }

        if (player != null)
        {
            KillPlayer(player);
        }
    }

    public void KillPlayer(PlayerHealth player)
    {
        if (player.IsDead())
        {
            if (debugLogs) Debug.Log("[PlayerKiller] Player already dead, ignoring");
            return;
        }

        hasKilled = true;

        if (useInstantKill)
        {
            // Set health to 0 directly
            if (debugLogs) Debug.Log("[PlayerKiller] ⚠️ INSTANT KILLING PLAYER!");
            
            // Dùng TakeDamage với damage rất cao để trigger death sequence đúng cách
            player.TakeDamage(killDamage, transform.position);
        }
        else
        {
            if (debugLogs) Debug.Log($"[PlayerKiller] Dealing {killDamage} damage to player");
            player.TakeDamage(killDamage, transform.position);
        }
    }

    // Public method để bế động gọi từ elsewhere
    public void KillPlayerManual()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            KillPlayer(playerHealth);
        }
        else
        {
            Debug.LogError("[PlayerKiller] PlayerHealth not found");
        }
    }

    // Reset state nếu scene được load lại
    public void ResetKill()
    {
        hasKilled = false;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class BossWeaponHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 30f;

    [Header("Hit Settings")]
    public string targetTag = "Player"; // Tag của Player
    public bool isActive = false;

    private HashSet<Collider> hitTargets = new HashSet<Collider>();
    private Collider hitboxCollider;
    private Transform bossTransform;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;

        BossBrain bossBrain = GetComponentInParent<BossBrain>();
        if (bossBrain != null)
        {
            bossTransform = bossBrain.transform;
        }
        else
        {
            bossTransform = transform.root; // Fallback
        }

        DisableHitbox();
    }

    public void EnableHitbox()
    {
        isActive = true;
        hitTargets.Clear(); // Reset để có thể hit lại
        hitboxCollider.enabled = true;
        Debug.Log($"⚔️ {gameObject.name} Hitbox ENABLED");
    }

    public void DisableHitbox()
    {
        isActive = false;
        hitboxCollider.enabled = false;
        Debug.Log($"🛡️ {gameObject.name} Hitbox DISABLED");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Check nếu đã hit target này rồi (tránh multi-hit)
        if (hitTargets.Contains(other)) return;

        // Check tag
        if (!other.CompareTag(targetTag)) return;

        // Tìm PlayerHealth (có thể ở parent hoặc chính object)
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            hitTargets.Add(other);

            // Gây damage - truyền vị trí boss để tính knockback
            playerHealth.TakeDamage(damage, bossTransform.position);

            Debug.Log($"💥 Boss hit {other.name} for {damage} damage!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Hit object with tag '{targetTag}' but no PlayerHealth found on {other.name}");
        }
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        if (isActive)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Đỏ trong suốt
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Xám trong suốt
        }

        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            Vector3 top = capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius);
            Vector3 bottom = capsule.center - Vector3.up * (capsule.height / 2 - capsule.radius);
            Gizmos.DrawWireSphere(top, capsule.radius);
            Gizmos.DrawWireSphere(bottom, capsule.radius);
        }
    }
}
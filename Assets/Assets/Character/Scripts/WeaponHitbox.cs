using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;
    public float knockbackForce = 10f;
    public float knockbackUpwardForce = 2f;

    [Header("Hit Detection")]
    [Tooltip("Delay trước khi hitbox active")]
    public float activationDelay = 0.05f;

    [Tooltip("Đẩy enemies đang overlap trước khi enable")]
    public bool pushOverlappingEnemies = true;

    [Tooltip("Lực đẩy enemies overlap")]
    public float pushForce = 15f;

    [Header("Tags")]
    [Tooltip("Tag của enemy thường")]
    public string enemyTag = "Enemy";

    [Tooltip("Tag của boss")]
    public string bossTag = "Boss";

    private bool canDealDamage = false;
    private List<Collider> hitEnemies = new List<Collider>();
    private Coroutine enableCoroutine;

    void Start()
    {
        Debug.Log($"⚔️ WeaponHitbox initialized: Damage={damage}, Knockback={knockbackForce}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (hitEnemies.Contains(other)) return;

        if (other.CompareTag(enemyTag))
        {
            DealDamageToEnemy(other);
        }
        else if (other.CompareTag(bossTag))
        {
            DealDamageToBoss(other);
        }
    }

    void DealDamageToEnemy(Collider enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy == null)
            enemy = enemyCollider.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            Vector3 playerForward = transform.root.forward;
            enemy.TakeDamage(damage, transform.root.position, playerForward);
            OverrideEnemyKnockback(enemy);
            hitEnemies.Add(enemyCollider);
            Debug.Log($"✅ Dealt {damage} damage to Enemy: {enemyCollider.gameObject.name}");
        }
        else
        {
            Debug.LogError($"❌ No Enemy component on {enemyCollider.gameObject.name}!");
        }
    }

    void DealDamageToBoss(Collider bossCollider)
    {
        // Tìm BossHealth trên chính object hoặc parent
        BossHealth bossHealth = bossCollider.GetComponent<BossHealth>();
        if (bossHealth == null)
            bossHealth = bossCollider.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            hitEnemies.Add(bossCollider);
            Debug.Log($"✅ Dealt {damage} damage to Boss: {bossCollider.gameObject.name}");
        }
        else
        {
            Debug.LogError($"❌ No BossHealth component on {bossCollider.gameObject.name}!");
        }
    }

    void OverrideEnemyKnockback(Enemy enemy)
    {
        enemy.knockbackForce = knockbackForce;
        enemy.knockbackUpwardForce = knockbackUpwardForce;
    }

    public void EnableDamage()
    {
        hitEnemies.Clear();

        if (enableCoroutine != null)
            StopCoroutine(enableCoroutine);

        enableCoroutine = StartCoroutine(EnableDamageDelayed());
    }

    IEnumerator EnableDamageDelayed()
    {
        if (pushOverlappingEnemies)
            PushOverlappingEnemies();

        if (activationDelay > 0)
            yield return new WaitForSeconds(activationDelay);

        canDealDamage = true;
        Debug.Log($"🗡️ Weapon damage ENABLED - Damage: {damage}");

        CheckForTargetsInHitbox();
    }

    void CheckForTargetsInHitbox()
    {
        Collider hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null) return;

        Collider[] overlapping = Physics.OverlapBox(
            hitboxCollider.bounds.center,
            hitboxCollider.bounds.extents,
            transform.rotation
        );

        // Find closest target to avoid hitting multiple bosses at the same time
        Collider closestTarget = null;
        float closestDistance = float.MaxValue;
        Vector3 weaponPos = transform.position;

        foreach (Collider col in overlapping)
        {
            if (hitEnemies.Contains(col)) continue;

            if (col.CompareTag(enemyTag) || col.CompareTag(bossTag))
            {
                float distance = Vector3.Distance(weaponPos, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = col;
                }
            }
        }

        // Only hit the closest target
        if (closestTarget != null)
        {
            if (closestTarget.CompareTag(enemyTag))
                DealDamageToEnemy(closestTarget);
            else if (closestTarget.CompareTag(bossTag))
                DealDamageToBoss(closestTarget);
        }
    }

    void PushOverlappingEnemies()
    {
        Collider hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null) return;

        Collider[] overlapping = Physics.OverlapBox(
            hitboxCollider.bounds.center,
            hitboxCollider.bounds.extents,
            transform.rotation
        );

        // Only push the closest target
        Collider closestTarget = null;
        float closestDistance = float.MaxValue;
        Vector3 weaponPos = transform.position;

        foreach (Collider col in overlapping)
        {
            if (col.CompareTag(enemyTag) || col.CompareTag(bossTag))
            {
                float distance = Vector3.Distance(weaponPos, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = col;
                }
            }
        }

        if (closestTarget != null)
        {
            Rigidbody targetRb = closestTarget.GetComponent<Rigidbody>();
            if (targetRb == null)
                targetRb = closestTarget.GetComponentInParent<Rigidbody>();

            if (targetRb != null)
            {
                Vector3 pushDirection = transform.root.forward;
                pushDirection.y = 0;
                pushDirection.Normalize();

                Vector3 pushVelocity = pushDirection * pushForce;
                pushVelocity.y = 1f;

                targetRb.WakeUp();
                targetRb.linearVelocity = pushVelocity;

                Debug.Log($"⚡ Pushed closest target: {closestTarget.gameObject.name}");
            }
        }
    }

    public void DisableDamage()
    {
        canDealDamage = false;

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }

        Debug.Log("🛡️ Weapon damage DISABLED");
    }

    void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = canDealDamage ? new Color(1f, 0f, 0f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
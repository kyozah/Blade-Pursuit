using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;
    public float knockbackForce = 10f;
    public float knockbackUpwardForce = 2f;

    [Header("Hit Detection")]
    public float activationDelay = 0.05f;
    public string enemyTag = "Enemy";
    public string bossTag = "Boss";

    [Header("VFX")]
    public GameObject hitVFXPrefab;
    public float vfxLifetime = 0.5f;

    private bool canDealDamage = false;
    private List<Collider> hitEnemies = new List<Collider>();
    private Coroutine enableCoroutine;
    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        Debug.Log($"⚔️ WeaponHitbox initialized: Damage={damage}, Knockback={knockbackForce}");
    }

    void Update()
    {
        if (!canDealDamage || boxCollider == null) return;

        Collider[] hits = Physics.OverlapBox(
            boxCollider.bounds.center,
            boxCollider.bounds.extents,
            transform.rotation
        );

        foreach (Collider col in hits)
        {
            if (hitEnemies.Contains(col)) continue;

            Debug.Log($"WeaponHitbox hit: {col.name}, tag: {col.tag}");

            if (col.CompareTag(enemyTag))
                DealDamageToEnemy(col);
            else if (col.CompareTag(bossTag))
                DealDamageToBoss(col);
        }
    }

    void DealDamageToEnemy(Collider enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy == null)
            enemy = enemyCollider.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            Vector3 attackerForward = transform.root.forward;
            enemy.TakeDamage(damage, transform.root.position, attackerForward);
            OverrideEnemyKnockback(enemy);
            hitEnemies.Add(enemyCollider);
            SpawnHitVFX(enemyCollider.ClosestPoint(transform.position));
            Debug.Log($"✅ Dealt {damage} damage to Enemy: {enemyCollider.name}");
        }
        else
        {
            Debug.LogError($"❌ No Enemy component on {enemyCollider.name}!");
        }
    }

    void DealDamageToBoss(Collider bossCollider)
    {
        Debug.Log($"DealDamageToBoss called on: {bossCollider.name}");

        // Cách 1: Lấy trực tiếp từ collider
        BossHealth bossHealth = bossCollider.GetComponent<BossHealth>();

        // Cách 2: Nếu không có, tìm trong parent
        if (bossHealth == null)
            bossHealth = bossCollider.GetComponentInParent<BossHealth>();

        // Cách 3: Tìm trong toàn bộ hierarchy (bao gồm cả child)
        if (bossHealth == null)
            bossHealth = bossCollider.GetComponentInChildren<BossHealth>();

        // Cách 4: Tìm bằng tag (fallback)
        if (bossHealth == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null)
                bossHealth = bossObj.GetComponentInChildren<BossHealth>();
        }

        if (bossHealth != null)
        {
            Debug.Log($"Found BossHealth! Current HP: {bossHealth.CurrentHP}, dealing {damage} damage");
            bossHealth.TakeDamage(damage);
            hitEnemies.Add(bossCollider);
            SpawnHitVFX(bossCollider.ClosestPoint(transform.position));
            Debug.Log($"✅ Dealt {damage} damage to Boss: {bossCollider.name}, New HP: {bossHealth.CurrentHP}");
        }
        else
        {
            Debug.LogError($"❌ No BossHealth found on {bossCollider.name} or its parents/children!");
            Debug.Log($"   Collider name: {bossCollider.name}");
            Debug.Log($"   Collider tag: {bossCollider.tag}");
            Debug.Log($"   Root object: {bossCollider.transform.root.name}");
        }
    }

    void SpawnHitVFX(Vector3 position)
    {
        if (hitVFXPrefab == null) return;

        GameObject vfx = Instantiate(hitVFXPrefab, position, Quaternion.identity);
        Destroy(vfx, vfxLifetime);
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
        if (activationDelay > 0)
            yield return new WaitForSeconds(activationDelay);

        canDealDamage = true;
        Debug.Log($"🗡️ Weapon damage ENABLED - Damage: {damage}");
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
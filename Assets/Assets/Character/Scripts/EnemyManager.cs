using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject skeletonPrefab;
    public GameObject ghoulPrefab;
    public GameObject tankPrefab;
    [Range(0f,1f)] public float skeletonWeight = 0.5f;
    [Range(0f,1f)] public float ghoulWeight = 0.3f;
    [Range(0f,1f)] public float tankWeight = 0.2f;

    [Tooltip("Toggle which enemy types are allowed to spawn from this manager.")]
    public bool allowSkeleton = true;
    public bool allowGhoul = true;
    public bool allowTank = true;

    [Header("Per-type Limits")]
    [Tooltip("Maximum number of Ghouls allowed to exist in this spawn zone at the same time. Set 0 for no ghouls.")]
    public int maxGhoulPerZone = 1;

    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnHeight = 0f;

    [Header("Attack Management")]
    public float attackCooldown = 3f; // Delay giữa các attack

    [Header("Debug")]
    public bool showDebugInfo = false;

    private List<Enemy> enemies = new List<Enemy>();
    private bool playerInZone = false;
    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private Enemy currentAttackingEnemy = null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has tag 'Player'");
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"EnemyManager '{gameObject.name}' missing Collider! Add a Collider and set 'IsTrigger' = true.");
        }
        else
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning($"EnemyManager '{gameObject.name}' Collider 'IsTrigger' = false. OnTriggerEnter won't fire.");
            }

            // If player already inside zone at start, spawn immediately
            if (player != null && col.bounds.Contains(player.position))
            {
                playerInZone = true;
                SpawnEnemies();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            SpawnEnemies();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            // Có thể despawn enemies nếu muốn, nhưng theo yêu cầu, giữ lại
        }
    }

    void SpawnEnemies()
    {
        // Remove destroyed/null entries before spawning
        enemies.RemoveAll(e => e == null);

        if (skeletonPrefab == null && ghoulPrefab == null && tankPrefab == null)
        {
            Debug.LogError($"EnemyManager '{gameObject.name}' has no prefabs assigned (skeleton/ghoul/tank). Assign at least one prefab.");
            return;
        }

        for (int i = enemies.Count; i < maxEnemies; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject prefab = GetRandomPrefab();
            if (prefab == null)
            {
                Debug.LogWarning($"EnemyManager '{gameObject.name}' no allowed prefabs to spawn (check 'Allow' toggles and assigned prefabs).");
                continue;
            }

            // Enforce Ghoul per-zone limit: if prefab chosen is Ghoul but limit reached, pick alternate
            if (prefab == ghoulPrefab && maxGhoulPerZone >= 0)
            {
                int existingGhouls = 0;
                foreach (var e in enemies)
                {
                    if (e == null) continue;
                    if (e is Ghoul) existingGhouls++;
                }
                if (existingGhouls >= maxGhoulPerZone)
                {
                    // Try to get an alternate prefab excluding Ghoul
                    prefab = GetRandomPrefabExcludingGhoul();
                    if (prefab == null)
                    {
                        // nothing else allowed to spawn, skip this slot
                        if (showDebugInfo) Debug.Log($"EnemyManager '{gameObject.name}': max ghouls reached ({existingGhouls}), skipping spawn.");
                        continue;
                    }
                }
            }

            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            if (enemyObj == null)
            {
                Debug.LogError($"Failed to instantiate prefab {prefab.name} at {spawnPos}");
                continue;
            }
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetManager(this);
                enemies.Add(enemy);
            }
            else
            {
                Debug.LogError($"Prefab '{prefab.name}' does not contain an 'Enemy' component.");
                Destroy(enemyObj);
            }
        }
    }

    GameObject GetRandomPrefabExcludingGhoul()
    {
        // Build a weighted list of allowed prefabs excluding ghoul
        var prefabs = new System.Collections.Generic.List<GameObject>();
        var weights = new System.Collections.Generic.List<float>();

        if (allowSkeleton && skeletonPrefab != null && skeletonWeight > 0f)
        {
            prefabs.Add(skeletonPrefab);
            weights.Add(skeletonWeight);
        }
        if (allowTank && tankPrefab != null && tankWeight > 0f)
        {
            prefabs.Add(tankPrefab);
            weights.Add(tankWeight);
        }

        if (prefabs.Count == 0) return null;

        float total = 0f;
        foreach (var w in weights) total += w;
        if (total <= 0f) return prefabs[0];

        float rnd = Random.value * total;
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (rnd <= weights[i]) return prefabs[i];
            rnd -= weights[i];
        }

        return prefabs[prefabs.Count - 1];
    }

    GameObject GetRandomPrefab()
    {
        // Build a weighted list of allowed prefabs
        var prefabs = new System.Collections.Generic.List<GameObject>();
        var weights = new System.Collections.Generic.List<float>();

        if (allowSkeleton && skeletonPrefab != null && skeletonWeight > 0f)
        {
            prefabs.Add(skeletonPrefab);
            weights.Add(skeletonWeight);
        }
        if (allowGhoul && ghoulPrefab != null && ghoulWeight > 0f)
        {
            prefabs.Add(ghoulPrefab);
            weights.Add(ghoulWeight);
        }
        if (allowTank && tankPrefab != null && tankWeight > 0f)
        {
            prefabs.Add(tankPrefab);
            weights.Add(tankWeight);
        }

        if (prefabs.Count == 0) return null;

        float total = 0f;
        foreach (var w in weights) total += w;
        if (total <= 0f) return prefabs[0];

        float rnd = Random.value * total;
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (rnd <= weights[i]) return prefabs[i];
            rnd -= weights[i];
        }

        return prefabs[prefabs.Count - 1];
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
        return spawnPos;
    }

    public bool CanAttack(Enemy enemy)
    {
        float cooldown = attackCooldown;
        if (enemy != null && enemy.attackCooldownOverride > 0f) cooldown = enemy.attackCooldownOverride;
        float sinceLast = Time.time - lastAttackTime;
        if (sinceLast < cooldown)
        {
            if ((enemy != null && enemy.showDebugInfo) || showDebugInfo)
                Debug.Log($"CanAttack('{(enemy!=null?enemy.gameObject.name:"<null>")}'): cooldown active ({sinceLast:F2}s/{cooldown}s)");
            return false;
        }
        if (currentAttackingEnemy != null && currentAttackingEnemy != enemy)
        {
            if ((enemy != null && enemy.showDebugInfo) || showDebugInfo)
                Debug.Log($"CanAttack('{(enemy!=null?enemy.gameObject.name:"<null>")}'): another enemy '{currentAttackingEnemy.gameObject.name}' is currently attacking.");
            return false;
        }
        return true;
    }

    public void StartAttack(Enemy enemy)
    {
        currentAttackingEnemy = enemy;
    }

    public void EndAttack(Enemy enemy)
    {
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
            lastAttackTime = Time.time; // start cooldown after attack finishes
        }
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public bool IsPlayerInZone()
    {
        return playerInZone;
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
        }
    }
}
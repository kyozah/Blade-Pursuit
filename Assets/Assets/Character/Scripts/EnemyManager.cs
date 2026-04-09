using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Fusion;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject skeletonPrefab;
    [FormerlySerializedAs("ghoulPrefab")] public GameObject flyPrefab;
    public GameObject tankPrefab;
    [Range(0f,1f)] public float skeletonWeight = 0.5f;
    [FormerlySerializedAs("ghoulWeight")] [Range(0f,1f)] public float flyWeight = 0.3f;
    [Range(0f,1f)] public float tankWeight = 0.2f;

    [Tooltip("Toggle which enemy types are allowed to spawn from this manager.")]
    public bool allowSkeleton = true;
    [FormerlySerializedAs("allowGhoul")] public bool allowFly = true;
    public bool allowTank = true;

    [Header("Per-type Limits")]
    [Tooltip("Maximum number of Flies allowed to exist in this spawn zone at the same time. Set 0 for no flies.")]
    [FormerlySerializedAs("maxGhoulPerZone")] public int maxFlyPerZone = 1;

    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnHeight = 0f;

    [Header("Attack Management")]
    public float attackCooldown = 2f; // Delay giữa các attack (default 2s)

    [Header("Debug")]
    public bool showDebugInfo = false;

    private List<Enemy> enemies = new List<Enemy>();
    private bool playerInZone = false;
    private bool hasSpawnedOnce = false; // ensure we only spawn enemies one time
    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private Enemy currentAttackingEnemy = null;
    private System.Random deterministicRng;

    void Start()
    {
        int seed = gameObject.name.GetHashCode()
            ^ Mathf.RoundToInt(transform.position.x * 100f)
            ^ Mathf.RoundToInt(transform.position.y * 100f)
            ^ Mathf.RoundToInt(transform.position.z * 100f);
        deterministicRng = new System.Random(seed);

        // Kiểm tra Collider ngay từ đầu
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"EnemyManager '{gameObject.name}' missing Collider! Add a Collider and set 'IsTrigger' = true.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"EnemyManager '{gameObject.name}' Collider 'IsTrigger' = false. OnTriggerEnter won't fire.");
        }
    }

    void Update()
    {
        // Nếu chưa tìm thấy player, hãy thử tìm mỗi Frame cho đến khi thấy
        if (player == null)
        {
            GameObject playerObj = FindPreferredPlayer();
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[EnemyManager] Đã tìm thấy Player mạng!");

                // Kiểm tra xem player có đang đứng sẵn trong vùng kích hoạt không
                Collider col = GetComponent<Collider>();
                if (col != null && col.bounds.Contains(player.position))
                {
                    playerInZone = true;
                    SpawnEnemies();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (!hasSpawnedOnce)
            {
                SpawnEnemies();
                hasSpawnedOnce = true;
            }
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
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null && !runner.IsServer)
            return;

        if (hasSpawnedOnce) return; // guard in case called elsewhere
        // Remove destroyed/null entries before spawning
        enemies.RemoveAll(e => e == null);

        if (skeletonPrefab == null && flyPrefab == null && tankPrefab == null)
        {
            Debug.LogError($"EnemyManager '{gameObject.name}' has no prefabs assigned (skeleton/fly/tank). Assign at least one prefab.");
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

            // Enforce Fly per-zone limit: if prefab chosen is Fly but limit reached, pick alternate
            if (prefab == flyPrefab && maxFlyPerZone >= 0)
            {
                int existingFlies = 0;
                foreach (var e in enemies)
                {
                    if (e == null) continue;
                    if (e is Fly) existingFlies++;
                }
                if (existingFlies >= maxFlyPerZone)
                {
                    // Try to get an alternate prefab excluding Fly
                    prefab = GetRandomPrefabExcludingFly();
                    if (prefab == null)
                    {
                        // nothing else allowed to spawn, skip this slot
                        if (showDebugInfo) Debug.Log($"EnemyManager '{gameObject.name}': max flies reached ({existingFlies}), skipping spawn.");
                        continue;
                    }
                }
            }

            GameObject enemyObj = SpawnEnemyObject(prefab, spawnPos, runner);
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

    GameObject GetRandomPrefabExcludingFly()
    {
        // Build a weighted list of allowed prefabs excluding fly
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

        float rnd = (float)deterministicRng.NextDouble() * total;
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
        if (allowFly && flyPrefab != null && flyWeight > 0f)
        {
            prefabs.Add(flyPrefab);
            weights.Add(flyWeight);
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

        float rnd = (float)deterministicRng.NextDouble() * total;
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (rnd <= weights[i]) return prefabs[i];
            rnd -= weights[i];
        }

        return prefabs[prefabs.Count - 1];
    }

    Vector3 GetRandomSpawnPosition()
    {
        float angle = (float)deterministicRng.NextDouble() * Mathf.PI * 2f;
        float radius = Mathf.Sqrt((float)deterministicRng.NextDouble()) * spawnRadius;
        Vector2 randomCircle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
        return spawnPos;
    }

    private GameObject FindPreferredPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length == 0)
            return null;

        GameObject fallback = null;
        int bestAuthorityId = int.MaxValue;

        foreach (var p in players)
        {
            if (p == null) continue;

            var netObj = p.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                int id = netObj.InputAuthority.PlayerId;
                if (id < bestAuthorityId)
                {
                    bestAuthorityId = id;
                    fallback = p;
                }
                continue;
            }

            if (fallback == null)
                fallback = p;
        }

        return fallback;
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

    private GameObject SpawnEnemyObject(GameObject prefab, Vector3 spawnPos, NetworkRunner runner)
    {
        if (runner == null)
            return Instantiate(prefab, spawnPos, Quaternion.identity);

        var netObjPrefab = prefab.GetComponent<NetworkObject>();
        if (netObjPrefab == null)
        {
            Debug.LogError($"Enemy prefab '{prefab.name}' thiếu NetworkObject. Thêm NetworkObject + EnemyNetworkSync để đồng bộ online.");
            return null;
        }

        var spawned = runner.Spawn(netObjPrefab, spawnPos, Quaternion.identity, default);
        return spawned != null ? spawned.gameObject : null;
    }
}
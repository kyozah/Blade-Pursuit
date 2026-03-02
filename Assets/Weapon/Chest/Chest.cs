using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    public bool isOpen = false;

    [Header("Animation")]
    public Animator animator;
    public string openTrigger = "Open";
    public float openAnimationDuration = 1f;

    [Header("Loot Settings")]
    public List<GameObject> lootItems = new List<GameObject>();
    public Transform lootSpawnPoint;
    public float lootSpreadRadius = 0.8f;

    [Header("VFX & SFX")]
    public GameObject openVFX;
    public AudioClip openSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (lootSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("LootSpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = Vector3.up * 0.5f;
            lootSpawnPoint = spawnObj.transform;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (isOpen) return;
        OpenChest();
    }

    void OpenChest()
    {
        isOpen = true;

        if (animator != null)
            animator.SetTrigger(openTrigger);

        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        if (openVFX != null)
            Instantiate(openVFX, lootSpawnPoint.position, Quaternion.identity);

        StartCoroutine(SpawnLootAfterDelay());
    }

    IEnumerator SpawnLootAfterDelay()
    {
        yield return new WaitForSeconds(openAnimationDuration * 0.5f);
        SpawnLoot();
    }

    void SpawnLoot()
    {
        if (lootItems.Count == 0)
        {
            Debug.LogWarning("Chest has no loot!");
            return;
        }

        float angleStep = 360f / lootItems.Count;

        for (int i = 0; i < lootItems.Count; i++)
        {
            if (lootItems[i] == null) continue;

            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * lootSpreadRadius;

            // ✅ Dùng Y của lootSpawnPoint hoàn toàn — kéo SpawnPoint lên/xuống trong scene để chỉnh
            Vector3 spawnPos = new Vector3(
                lootSpawnPoint.position.x + offset.x,
                lootSpawnPoint.position.y,
                lootSpawnPoint.position.z + offset.z
            );

            Instantiate(lootItems[i], spawnPos, Quaternion.identity);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Hit Detection")]
    public float activationDelay = 0.05f;

    public bool pushOverlappingEnemies = true;
    public float pushForce = 15f;

    private bool canDealDamage = false;
    private List<Collider> hitTargets = new List<Collider>();
    private Coroutine enableCoroutine;

    void OnTriggerEnter(Collider other)
    {
        TryDealDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryDealDamage(other);
    }

    void TryDealDamage(Collider other)
    {
        if (!canDealDamage) return;
        if (hitTargets.Contains(other)) return;

        // ================= ENEMY =================
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector3 playerForward = transform.root.forward;
                enemy.TakeDamage(damage, transform.root.position, playerForward);

                hitTargets.Add(other);
            }
        }

        // ================= BOSS =================
        else if (other.CompareTag("Boss"))
        {
            BossHealth bossHealth = other.GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damage);
                hitTargets.Add(other);
            }
        }
    }

    public void EnableDamage()
    {
        hitTargets.Clear();

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
    }

    public void DisableDamage()
    {
        canDealDamage = false;

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }
    }

    void PushOverlappingEnemies()
    {
        Collider hitbox = GetComponent<Collider>();
        if (hitbox == null) return;

        Collider[] overlapping = Physics.OverlapBox(
            hitbox.bounds.center,
            hitbox.bounds.extents,
            transform.rotation
        );

        foreach (Collider col in overlapping)
        {
            if (!col.CompareTag("Enemy")) continue;

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 dir = transform.root.forward;
            dir.y = 0;
            dir.Normalize();

            rb.linearVelocity = dir * pushForce + Vector3.up;
        }
    }
}

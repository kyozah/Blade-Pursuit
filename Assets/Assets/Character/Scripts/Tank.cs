using UnityEngine;
using System.Collections;

public class Tank : Enemy
{
    [Header("Tank Attack Timing")]
    [Tooltip("Time (seconds) from attack start until damage is applied during the animation")]
    public float attackHitDelay = 1f; // damage applied at 1s into animation

    protected override void Start()
    {
        // Configure Tank-specific stats before base.Start so health is initialized correctly
        maxHealth = 200f;
        attackDamage = 15f;
        attackDelay = 1.5f; // total attack animation duration / cooldown window
        moveSpeed = 4f; // very slow movement (doubled)
        detectionRange = 20f;
        attackRange = 5.5f;
        knockbackForce = 5f;
        knockbackUpwardForce = 1f;
        gameObject.name = "Tank";
        showDebugInfo = true; // enable debug logs by default for troubleshooting

        base.Start();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Run only in edit mode - do not override user customizations
        if (Application.isPlaying) return;

        bool changed = false;
        // Align OnValidate defaults with values set in Start()
        if (Mathf.Approximately(maxHealth, 100f)) { maxHealth = 200f; changed = true; }
        if (Mathf.Approximately(attackDamage, 5f)) { attackDamage = 15f; changed = true; }
        if (Mathf.Approximately(attackDelay, 1f)) { attackDelay = 1.5f; changed = true; }
        if (Mathf.Approximately(moveSpeed, 10f)) { moveSpeed = 4f; changed = true; }
        if (Mathf.Approximately(detectionRange, 15f)) { detectionRange = 20f; changed = true; }
        if (Mathf.Approximately(attackRange, 2f)) { attackRange = 5.5f; changed = true; }
        if (Mathf.Approximately(knockbackForce, 10f)) { knockbackForce = 5f; changed = true; }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    protected override IEnumerator PerformAttack()
    {
        // Clamp the hit delay so it doesn't exceed the attack period
        float hitDelay = Mathf.Clamp(attackHitDelay, 0f, attackDelay);

        if (showDebugInfo) Debug.Log($"{gameObject.name} attack started. Waiting {hitDelay:F2}s to apply hit (attackDelay={attackDelay:F2})");

        // Wait until the moment in the animation where hit should register
        yield return new WaitForSeconds(hitDelay);

        // Apply damage at hit frame
        if (showDebugInfo) Debug.Log($"{gameObject.name} applying attack damage at time {Time.time:F2}");
        ApplyAttackDamage();

        // Optionally wait out rest of animation before ending attack
        float remaining = attackDelay - hitDelay;
        if (remaining > 0f)
        {
            if (showDebugInfo) Debug.Log($"{gameObject.name} waiting remaining {remaining:F2}s to finish attack animation");
            yield return new WaitForSeconds(remaining);
        }

        // After attack, transition to retreat using protected helper
        FinishAttackAndRetreat();
    }
}

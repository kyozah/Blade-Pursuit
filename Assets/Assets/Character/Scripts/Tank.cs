using UnityEngine;
using System.Collections;

public class Tank : Enemy
{
    [Header("Tank Attack Timing")]
    [Tooltip("Time (seconds) from attack start until damage is applied during the animation")]
    public float attackHitDelay = 1f;

    protected override void Start()
    {
        maxHealth = 75f;
        attackDamage = 15f;
        attackDelay = 1.5f;
        moveSpeed = 4f;
        detectionRange = 20f;
        attackRange = 4.5f;
        knockbackForce = 5f;
        knockbackUpwardForce = 1f;
        gameObject.name = "Tank";
        showDebugInfo = true;

        base.Start();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;

        bool changed = false;
        if (Mathf.Approximately(maxHealth, 100f)) { maxHealth = 75f; changed = true; }
        if (Mathf.Approximately(attackDamage, 5f)) { attackDamage = 15f; changed = true; }
        if (Mathf.Approximately(attackDelay, 1f)) { attackDelay = 1.5f; changed = true; }
        if (Mathf.Approximately(moveSpeed, 10f)) { moveSpeed = 4f; changed = true; }
        if (Mathf.Approximately(detectionRange, 15f)) { detectionRange = 20f; changed = true; }
        if (Mathf.Approximately(attackRange, 2f)) { attackRange = 4.5f; changed = true; }
        if (Mathf.Approximately(knockbackForce, 10f)) { knockbackForce = 5f; changed = true; }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    protected override IEnumerator PerformAttack()
    {
        float hitDelay = Mathf.Clamp(attackHitDelay, 0f, attackDelay);

        if (showDebugInfo) Debug.Log($"{gameObject.name} attack started. Waiting {hitDelay:F2}s to apply hit");

        yield return new WaitForSeconds(hitDelay);

        if (showDebugInfo) Debug.Log($"{gameObject.name} applying attack damage at time {Time.time:F2}");
        ApplyAttackDamage();

        float remaining = attackDelay - hitDelay;
        if (remaining > 0f)
        {
            if (showDebugInfo) Debug.Log($"{gameObject.name} waiting remaining {remaining:F2}s to finish attack animation");
            yield return new WaitForSeconds(remaining);
        }

        FinishAttackAndChase();
    }
}
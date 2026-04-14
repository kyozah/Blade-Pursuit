using UnityEngine;

public class Fly : Enemy
{
    protected override void Start()
    {
        // Set Fly-specific stats before base initialization
        maxHealth = 1f;
        attackDamage = 8f;
        attackDelay = 0.5f;
        moveSpeed = 10f;
        detectionRange = 100f;
        attackRange = 1.5f;
        attackCooldownOverride = 1f;
        gameObject.name = "Fly";

        invertFacing = true;
        rotationSpeed = 1440f;

        base.Start();
    }

    protected override void StartAttack()
    {
        FacePlayerInstant();
        base.StartAttack();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;

        bool changed = false;
        if (Mathf.Approximately(maxHealth, 100f)) { maxHealth = 1f; changed = true; }
        if (Mathf.Approximately(attackDamage, 5f)) { attackDamage = 8f; changed = true; }
        if (Mathf.Approximately(attackDelay, 1f)) { attackDelay = 0.5f; changed = true; }
        if (Mathf.Approximately(moveSpeed, 10f)) { moveSpeed = 10f; }
        if (Mathf.Approximately(detectionRange, 15f)) { detectionRange = 100f; changed = true; }
        if (Mathf.Approximately(attackRange, 2f)) { attackRange = 1.5f; changed = true; }
        if (Mathf.Approximately(attackCooldownOverride, -1f)) { attackCooldownOverride = 1f; changed = true; }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
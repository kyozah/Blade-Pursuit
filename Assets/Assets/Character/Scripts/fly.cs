using UnityEngine;

public class Fly : Enemy
{
    protected override void Start()
    {
        // Set Fly-specific stats before base initialization
        maxHealth = 1f;            // dies from a single hit
        attackDamage = 8f;
        attackDelay = 0.5f;       // fast attack
        moveSpeed = 10f;          // very fast movement (doubled) - can catch sprinting player
        detectionRange = 100f;     // larger detection so it can chase while player runs
        attackRange = 1.5f;
        attackCooldownOverride = 1f; // Fly attacks once per second max
        gameObject.name = "Fly";

        // Fly model faces tail by default in art; invert facing so it looks head-first at player
        invertFacing = true;
        rotationSpeed = 1440f; // very fast turn for instant facing during attacks

        base.Start();
    }

    protected override void StartAttack()
    {
        // Face player immediately before performing the attack so model's front points at target
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
        if (Mathf.Approximately(moveSpeed, 10f)) { moveSpeed = 10f; /* keep fast */ }
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

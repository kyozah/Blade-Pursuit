using UnityEngine;

public class Skeleton : Enemy
{
    protected override void Start()
    {
        // Use base initialization and keep default stats
        base.Start();
        gameObject.name = "Skeleton";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        // Ensure prefab name is easy to read in editor
        if (string.IsNullOrEmpty(gameObject.name) || gameObject.name.StartsWith("Enemy"))
        {
            gameObject.name = "Skeleton";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

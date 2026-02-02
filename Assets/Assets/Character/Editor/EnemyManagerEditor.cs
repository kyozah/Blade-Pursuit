using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerEditor : Editor
{
    private bool showSpawnConfig = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Edit Allowed Spawns"))
        {
            showSpawnConfig = !showSpawnConfig;
        }

        if (showSpawnConfig)
        {
            EditorGUI.indentLevel++;
            EnemyManager mgr = (EnemyManager)target;

            EditorGUI.BeginChangeCheck();
            mgr.allowSkeleton = EditorGUILayout.Toggle("Allow Skeleton", mgr.allowSkeleton);
            mgr.allowGhoul = EditorGUILayout.Toggle("Allow Ghoul", mgr.allowGhoul);
            mgr.allowTank = EditorGUILayout.Toggle("Allow Tank", mgr.allowTank);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(mgr);
            }

            EditorGUI.indentLevel--;
        }
    }
}

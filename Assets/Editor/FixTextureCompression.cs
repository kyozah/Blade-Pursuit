using UnityEditor;
using UnityEngine;

public static class FixTextureCompression
{
    [MenuItem("Assets/Texture Tools/Fix DXT5 Errors (Set Selected to RGBA32)")]
    public static void FixSelectedTextures()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("No textures selected.");
            return;
        }

        int fixedCount = 0;
        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            // Only operate on textures
            Debug.Log($"[FixTextureCompression] Processing: {path}");

            // Make sure sprite/textures remain readable state as original
            bool wasReadable = ti.isReadable;

            ti.textureType = ti.textureType; // keep current type

            // Set to uncompressed format for Standalone/platforms to avoid DXT5 constraint
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            // Also explicitly set platform settings for Standalone (and WebGL as example)
            var settings = ti.GetPlatformTextureSettings("Standalone");
            settings.overridden = true;
            settings.format = TextureImporterFormat.RGBA32;
            settings.maxTextureSize = Mathf.Max(settings.maxTextureSize, 32);
            ti.SetPlatformTextureSettings(settings);

            // Reapply readable flag to original
            ti.isReadable = wasReadable;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            fixedCount++;
        }

        Debug.Log($"[FixTextureCompression] Done. Fixed {fixedCount} textures.");
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ResizeTexturesToMultipleOf4
{
    [MenuItem("Assets/Texture Tools/Resize Selected to Multiple of 4 (Create _m4 versions)")]
    public static void ResizeSelectedToMultipleOf4()
    {
        ProcessSelection(false);
    }

    [MenuItem("Assets/Texture Tools/Resize Selected to Multiple of 4 + Set DXT5 (Create _m4 versions)")]
    public static void ResizeSelectedToMultipleOf4AndSetDXT5()
    {
        ProcessSelection(true);
    }

    static void ProcessSelection(bool setDxt5)
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("No textures selected.");
            return;
        }

        int created = 0;
        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            // Load texture (ensure readable)
            bool prevReadable = ti.isReadable;
            if (!prevReadable)
            {
                ti.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning($"Could not load texture at {path}");
                if (!prevReadable)
                {
                    ti.isReadable = prevReadable;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                continue;
            }

            int w = tex.width;
            int h = tex.height;
            int newW = Mathf.Max(4, Mathf.RoundToInt(w / 4f) * 4);
            int newH = Mathf.Max(4, Mathf.RoundToInt(h / 4f) * 4);

            if (newW == w && newH == h)
            {
                Debug.Log($"Already multiple-of-4: {path} ({w}x{h}) - skipping resize");
            }
            else
            {
                // Render scaled texture into RenderTexture
                var rt = RenderTexture.GetTemporary(newW, newH, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(tex, rt);

                var oldActive = RenderTexture.active;
                RenderTexture.active = rt;

                var newTex = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
                newTex.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
                newTex.Apply();

                RenderTexture.active = oldActive;
                RenderTexture.ReleaseTemporary(rt);

                // Save to new file next to original
                string assetDir = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string newFilename = filename + "_m4.png";
                string newRelPath = Path.Combine(assetDir, newFilename).Replace("\\", "/");
                string diskPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), newRelPath);

                try
                {
                    File.WriteAllBytes(diskPath, newTex.EncodeToPNG());
                    AssetDatabase.ImportAsset(newRelPath, ImportAssetOptions.ForceUpdate);
                    Debug.Log($"Created resized texture: {newRelPath} ({newW}x{newH})");
                    created++;

                    if (setDxt5)
                    {
                        var newTi = AssetImporter.GetAtPath(newRelPath) as TextureImporter;
                        if (newTi != null)
                        {
                            newTi.textureCompression = TextureImporterCompression.Compressed;
                            var settings = newTi.GetPlatformTextureSettings("Standalone");
                            settings.overridden = true;
                            settings.format = TextureImporterFormat.DXT5;
                            newTi.SetPlatformTextureSettings(settings);
                            AssetDatabase.ImportAsset(newRelPath, ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to write resized texture {newRelPath}: {ex.Message}");
                }

                // cleanup
                Object.DestroyImmediate(newTex);
            }

            // restore readable flag on original
            if (!prevReadable)
            {
                ti.isReadable = prevReadable;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        Debug.Log($"Resize process complete. Created {created} new textures.");
    }
}
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class SpriteAtlasBuilder
{
    private const string AtlasRoot = "Assets/Art/Atlases";

    [InitializeOnLoadMethod]
    private static void AutoCreateAtlases()
    {
        EnsureAtlas(
            "Characters",
            new[]
            {
                "Assets/Art/characters/Characters"
            },
            maxTextureSize: 4096,
            includeInBuild: true,
            filterMode: FilterMode.Point);

        EnsureAtlas(
            "Tilemaps",
            new[]
            {
                "Assets/Art/tilemaps"
            },
            maxTextureSize: 4096,
            includeInBuild: true,
            filterMode: FilterMode.Point);

        EnsureAtlas(
            "Environment",
            new[]
            {
                "Assets/SimpleTopdownTileset"
            },
            maxTextureSize: 4096,
            includeInBuild: true,
            filterMode: FilterMode.Point);
    }

    [MenuItem("Tools/Dev/Art/Build Sprite Atlases")]
    private static void BuildAtlasesMenu()
    {
        AutoCreateAtlases();
        var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { AtlasRoot });
        var atlases = atlasGuids
            .Select(guid => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(a => a != null)
            .ToArray();

        if (atlases.Length == 0)
        {
            Debug.LogWarning("No sprite atlases found to pack.");
            return;
        }

        SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);
        Debug.Log($"Packed {atlases.Length} sprite atlases.");
    }

    private static void EnsureAtlas(
        string atlasName,
        string[] packableFolders,
        int maxTextureSize,
        bool includeInBuild,
        FilterMode filterMode)
    {
        if (!AssetDatabase.IsValidFolder(AtlasRoot))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Art", "Atlases"));
            AssetDatabase.Refresh();
        }

        string atlasPath = $"{AtlasRoot}/{atlasName}.spriteatlas";
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
        bool created = false;

        if (atlas == null)
        {
            atlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(atlas, atlasPath);
            created = true;
        }

        atlas.SetIncludeInBuild(includeInBuild);

        var packSettings = atlas.GetPackingSettings();
        packSettings.enableRotation = false;
        packSettings.enableTightPacking = false;
        packSettings.padding = 2;
        atlas.SetPackingSettings(packSettings);

        var texSettings = atlas.GetTextureSettings();
        texSettings.generateMipMaps = false;
        texSettings.readable = false;
        texSettings.filterMode = filterMode;
        texSettings.sRGB = true;
        atlas.SetTextureSettings(texSettings);

        var platform = atlas.GetPlatformSettings("DefaultTexturePlatform");
        platform.maxTextureSize = Mathf.Max(1024, maxTextureSize);
        atlas.SetPlatformSettings(platform);

        var packables = packableFolders
            .Select(path => AssetDatabase.LoadAssetAtPath<Object>(path))
            .Where(obj => obj != null)
            .ToArray();

        if (packables.Length > 0)
        {
            atlas.Remove(packables);
            atlas.Add(packables);
        }

        EditorUtility.SetDirty(atlas);
        AssetDatabase.SaveAssets();

        if (created)
        {
            Debug.Log($"Created sprite atlas: {atlasPath}");
        }
    }
}

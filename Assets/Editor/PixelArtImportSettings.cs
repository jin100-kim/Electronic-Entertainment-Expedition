#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PixelArtImportSettings
{
    private const int CharacterPpu = 32;

    [MenuItem("Tools/PixelArt/Set Character PPU 32")]
    public static void SetCharacterPpu()
    {
        ApplyPpu("Assets/Art/characters/Characters", CharacterPpu);
    }

    private static void ApplyPpu(string rootPath, int ppu)
    {
        if (string.IsNullOrEmpty(rootPath))
        {
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { rootPath });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
    }
}
#endif

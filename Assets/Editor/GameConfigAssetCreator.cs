using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GameConfigAssetCreator
{
    private const string ResourceFolder = "Assets/Resources";
    private const string AssetPath = "Assets/Resources/GameConfig.asset";
    private const string StageAssetPath = "Assets/Resources/StageConfig_Default.asset";
    private const string DifficultyAssetPath = "Assets/Resources/DifficultyConfig_Default.asset";

    static GameConfigAssetCreator()
    {
        EnsureAsset();
    }

    private static void EnsureAsset()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetPath);
        if (asset != null)
        {
            EnsureStageAndDifficulty(asset);
            return;
        }

        if (!AssetDatabase.IsValidFolder(ResourceFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        asset = ScriptableObject.CreateInstance<GameConfig>();
        AssetDatabase.CreateAsset(asset, AssetPath);
        EnsureStageAndDifficulty(asset);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureStageAndDifficulty(GameConfig asset)
    {
        if (asset == null)
        {
            return;
        }

        var stage = AssetDatabase.LoadAssetAtPath<StageConfig>(StageAssetPath);
        if (stage == null)
        {
            stage = ScriptableObject.CreateInstance<StageConfig>();
            AssetDatabase.CreateAsset(stage, StageAssetPath);
        }

        var difficulty = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(DifficultyAssetPath);
        if (difficulty == null)
        {
            difficulty = ScriptableObject.CreateInstance<DifficultyConfig>();
            AssetDatabase.CreateAsset(difficulty, DifficultyAssetPath);
        }

        bool dirty = false;
        if (asset.defaultStage == null)
        {
            asset.defaultStage = stage;
            dirty = true;
        }

        if (asset.defaultDifficulty == null)
        {
            asset.defaultDifficulty = difficulty;
            dirty = true;
        }

        if (dirty)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ForestMapBuilder
{
    private const int DefaultTileSize = 16;
    private const int SimpleTopdownCols = 30;
    private const string SimpleTopdownTilesetPath = "Assets/SimpleTopdownTileset/Tileset.png";
    private const string SimpleTopdownGeneratedRoot = "Assets/SimpleTopdownTileset/GeneratedTiles";
    private const string SimpleTopdownSpritePrefix = "SimpleTopdown";
    private const string BasicSolidTexturePath = SimpleTopdownGeneratedRoot + "/Basic/BasicSolid.png";
    private const string BasicSolidTilePath = SimpleTopdownGeneratedRoot + "/Basic/BasicSolid.asset";
    private static readonly Color BasicSolidColor = new Color(0.18f, 0.22f, 0.2f, 1f);
    private const int PreviewCaptureSize = 2048;
    private static readonly string[] PreviewScenePaths =
    {
        "Assets/Scenes/ForestOpenWorld.unity",
        "Assets/Scenes/DesertOpenWorld.unity",
        "Assets/Scenes/SnowOpenWorld.unity",
        "Assets/Scenes/BasicOpenWorld.unity"
    };

    private static readonly MapConfig ForestConfig = new MapConfig
    {
        Theme = MapTheme.Forest,
        TilesetTexturePath = SimpleTopdownTilesetPath,
        TilesFolder = SimpleTopdownGeneratedRoot + "/Forest",
        ScenePath = "Assets/Scenes/ForestOpenWorld.unity",
        SpritePrefix = SimpleTopdownSpritePrefix,
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    private static readonly MapConfig DesertConfig = new MapConfig
    {
        Theme = MapTheme.Desert,
        TilesetTexturePath = SimpleTopdownTilesetPath,
        TilesFolder = SimpleTopdownGeneratedRoot + "/Desert",
        ScenePath = "Assets/Scenes/DesertOpenWorld.unity",
        SpritePrefix = SimpleTopdownSpritePrefix,
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    private static readonly MapConfig SnowConfig = new MapConfig
    {
        Theme = MapTheme.Snow,
        TilesetTexturePath = SimpleTopdownTilesetPath,
        TilesFolder = SimpleTopdownGeneratedRoot + "/Snow",
        ScenePath = "Assets/Scenes/SnowOpenWorld.unity",
        SpritePrefix = SimpleTopdownSpritePrefix,
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    private static readonly MapConfig BasicConfig = new MapConfig
    {
        Theme = MapTheme.Forest,
        TilesetTexturePath = SimpleTopdownTilesetPath,
        TilesFolder = SimpleTopdownGeneratedRoot + "/Basic",
        ScenePath = "Assets/Scenes/BasicOpenWorld.unity",
        SpritePrefix = SimpleTopdownSpritePrefix,
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    [MenuItem("Tools/Map/Build Forest Open World")]
    private static void BuildForestOpenWorld()
    {
        RunWhenNotPlaying(() => BuildOpenWorld(ForestConfig));
    }

    [MenuItem("Tools/Map/Build Desert Open World")]
    private static void BuildDesertOpenWorld()
    {
        RunWhenNotPlaying(() => BuildOpenWorld(DesertConfig));
    }

    [MenuItem("Tools/Map/Build Snow Open World")]
    private static void BuildSnowOpenWorld()
    {
        RunWhenNotPlaying(() => BuildOpenWorld(SnowConfig));
    }

    [MenuItem("Tools/Map/Build Basic Open World")]
    private static void BuildBasicOpenWorld()
    {
        RunWhenNotPlaying(() => BuildBasicOpenWorldClean(BasicConfig));
    }

    [MenuItem("Tools/Map/Build All Open Worlds")]
    private static void BuildAllOpenWorlds()
    {
        RunWhenNotPlaying(() =>
        {
            BuildOpenWorld(ForestConfig);
            BuildOpenWorld(DesertConfig);
            BuildOpenWorld(SnowConfig);
            BuildBasicOpenWorldClean(BasicConfig);
        });
    }

    [MenuItem("Tools/Map/Capture OpenWorld Previews")]
    private static void CaptureOpenWorldPreviews()
    {
        RunWhenNotPlaying(CaptureOpenWorldPreviewsInternal);
    }

    private static void RunWhenNotPlaying(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            EditorApplication.delayCall += () => RunWhenNotPlaying(action);
            return;
        }

        action.Invoke();
    }

    private static void BuildOpenWorld(MapConfig config)
    {
        EnsureTilesetImported(config);

        var sprites = LoadSprites(config.TilesetTexturePath);
        if (sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {config.TilesetTexturePath}. Is the tileset imported?");
            return;
        }

        var selection = SelectTiles(config.Theme, config.TilesetTexturePath, sprites);
        if (selection.GroundTiles == null || selection.GroundTiles.Length == 0
            || selection.ObstacleTiles == null || selection.ObstacleTiles.Length == 0)
        {
            Debug.LogError("Failed to select tile palettes from the tileset.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = Path.GetFileNameWithoutExtension(config.ScenePath);

        var gridGo = new GameObject("Grid", typeof(Grid));
        gridGo.transform.position = Vector3.zero;

        var groundMap = CreateTilemap(gridGo, "Ground", -5, true);
        var detailMap = CreateTilemap(gridGo, "Details", -4, true);
        var propsMap = CreateTilemap(gridGo, "Props", -3, true);
        var obstacleMap = CreateTilemap(gridGo, "Obstacles", -2, true);
        var collisionMap = CreateTilemap(gridGo, "Collision", -2, false);

        SetupCollision(collisionMap);

        string prefix = config.Theme.ToString();
        var groundTiles = EnsureTileAssets($"{prefix}_Ground", selection.GroundTiles, Tile.ColliderType.None, config.TilesFolder);
        var detailTiles = EnsureTileAssets($"{prefix}_Detail", selection.DetailTiles, Tile.ColliderType.None, config.TilesFolder);
        var propsTiles = EnsureTileAssets($"{prefix}_Props", selection.PropsTiles, Tile.ColliderType.None, config.TilesFolder);
        var obstacleTiles = EnsureTileAssets($"{prefix}_Obstacle", selection.ObstacleTiles, Tile.ColliderType.Sprite, config.TilesFolder);

        var size = ResolveMapSize();
        int seedBase = ((int)config.Theme + 1) * 7331;
        var richness = ResolveRichness(config.Theme);
        int obstacleCount = Mathf.Max(12, Mathf.RoundToInt((size.Width + size.Height) * richness.ObstaclePerimeterFactor));
        FillGroundPattern(groundMap, groundTiles, size.Width, size.Height, seedBase, config.Theme);
        ScatterDetails(detailMap, detailTiles, size.Width, size.Height, density: richness.DetailDensity, seed: seedBase + 101);
        var propsPalette = propsTiles.Length > 0 ? propsTiles : detailTiles.Concat(obstacleTiles).Distinct().ToArray();
        ScatterProps(propsMap, propsPalette, size.Width, size.Height, density: richness.PropsDensity, seed: seedBase + 151);
        PlaceObstacles(obstacleMap, collisionMap, obstacleTiles, size.Width, size.Height, obstacleCount, seedBase + 211);
        CarveMainPaths(groundMap, detailMap, propsMap, obstacleMap, collisionMap, groundTiles, size.Width, size.Height, seedBase + 307);
        ClearSpawnArea(detailMap, propsMap, obstacleMap, collisionMap, radius: 4);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, config.ScenePath);
        Debug.Log($"{scene.name} scene saved: {config.ScenePath}");
    }

    private static void BuildBasicOpenWorldClean(MapConfig config)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = Path.GetFileNameWithoutExtension(config.ScenePath);

        var gridGo = new GameObject("Grid", typeof(Grid));
        gridGo.transform.position = Vector3.zero;

        var groundMap = CreateTilemap(gridGo, "Ground", -5, true);
        var collisionMap = CreateTilemap(gridGo, "Collision", -2, false);
        SetupCollision(collisionMap);

        var size = ResolveMapSize();
        var tile = GetOrCreateSolidGroundTile(BasicSolidTexturePath, BasicSolidTilePath, BasicSolidColor);
        FillGround(groundMap, tile, size.Width, size.Height);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, config.ScenePath);
        Debug.Log($"{scene.name} scene saved (clean basic): {config.ScenePath}");
    }

    private static void CaptureOpenWorldPreviewsInternal()
    {
        string outputDir = Path.Combine(Application.dataPath, "_Debug/MapCaptures");
        Directory.CreateDirectory(outputDir);

        var config = GameConfig.LoadDefault();
        Vector2 half = config != null ? config.game.mapHalfSize : new Vector2(24f, 24f);
        float orthoSize = Mathf.Max(half.x, half.y) + 2f;

        for (int i = 0; i < PreviewScenePaths.Length; i++)
        {
            string scenePath = PreviewScenePaths[i];
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Capture skipped, scene not found: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            CaptureScenePreview(scenePath, scene.name, outputDir, orthoSize);
        }

        AssetDatabase.Refresh();
        Debug.Log($"OpenWorld preview capture complete: {outputDir}");
    }

    private static void CaptureScenePreview(string scenePath, string sceneName, string outputDir, float orthoSize)
    {
        var cameraGo = new GameObject("__MapPreviewCaptureCamera__", typeof(Camera));
        try
        {
            var cam = cameraGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.cullingMask = ~0;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 500f;
            cam.allowMSAA = false;
            cam.allowHDR = false;
            cam.transform.position = new Vector3(0f, 0f, -100f);
            cam.transform.rotation = Quaternion.identity;

            var rt = new RenderTexture(PreviewCaptureSize, PreviewCaptureSize, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(PreviewCaptureSize, PreviewCaptureSize, TextureFormat.RGBA32, false);
            try
            {
                cam.targetTexture = rt;
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                cam.Render();
                tex.ReadPixels(new Rect(0, 0, PreviewCaptureSize, PreviewCaptureSize), 0, 0);
                tex.Apply(false, false);
                RenderTexture.active = prev;

                string outputPath = Path.Combine(outputDir, sceneName + ".png");
                File.WriteAllBytes(outputPath, tex.EncodeToPNG());
                Debug.Log($"Captured preview: {scenePath} -> {outputPath}");
            }
            finally
            {
                cam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraGo);
        }
    }

    private static TileBase GetOrCreateSolidGroundTile(string texturePath, string tilePath, Color color)
    {
        string folder = Path.GetDirectoryName(texturePath)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, folder.Replace("Assets/", string.Empty)));
            AssetDatabase.Refresh();
        }

        if (!File.Exists(texturePath))
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            File.WriteAllBytes(texturePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, tilePath);
        }

        tile.sprite = sprite;
        tile.color = Color.white;
        tile.colliderType = Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();
        return tile;
    }

    private static void EnsureTilesetImported(MapConfig config)
    {
        var importer = AssetImporter.GetAtPath(config.TilesetTexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Tileset texture not found: {config.TilesetTexturePath}");
            return;
        }

        int tileSize = Mathf.Max(1, config.TileSize);
        int spacing = Mathf.Max(0, config.Spacing);
        int margin = Mathf.Max(0, config.Margin);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = tileSize;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.isReadable = true;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.crunchedCompression = false;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.TilesetTexturePath);
        if (texture == null)
        {
            Debug.LogError($"Unable to load texture: {config.TilesetTexturePath}");
            return;
        }

        int cols = (texture.width - 2 * margin + spacing) / (tileSize + spacing);
        int rows = (texture.height - 2 * margin + spacing) / (tileSize + spacing);

        var metas = new List<SpriteMetaData>(cols * rows);
        int index = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var rect = new Rect(
                    margin + x * (tileSize + spacing),
                    margin + (rows - 1 - y) * (tileSize + spacing),
                    tileSize,
                    tileSize);

                metas.Add(new SpriteMetaData
                {
                    name = $"{config.SpritePrefix}_{index:D3}",
                    rect = rect,
                    alignment = (int)SpriteAlignment.Center
                });
                index++;
            }
        }

#pragma warning disable CS0618
        importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();
    }

    private static Sprite[] LoadSprites(string texturePath)
    {
        return AssetDatabase
            .LoadAllAssetRepresentationsAtPath(texturePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    private static TileSelection SelectTiles(MapTheme theme, string texturePath, Sprite[] sprites)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            return default;
        }

        var pixels = texture.GetPixels32();
        int texWidth = texture.width;
        int texHeight = texture.height;

        var stats = new List<TileStat>();
        foreach (var sprite in sprites)
        {
            var rect = sprite.textureRect;
            stats.Add(AnalyzeTile(sprite, rect, pixels, texWidth, texHeight, ExtractSpriteIndex(sprite.name)));
        }

        if (string.Equals(texturePath, SimpleTopdownTilesetPath, StringComparison.OrdinalIgnoreCase))
        {
            var tuned = SelectSimpleTopdownTiles(theme, stats);
            if (tuned.GroundTiles != null && tuned.GroundTiles.Length > 0
                && tuned.ObstacleTiles != null && tuned.ObstacleTiles.Length > 0)
            {
                return tuned;
            }
        }

        var opaqueCandidates = stats.Where(s => s.OpaqueRatio >= 0.92f).ToList();
        if (opaqueCandidates.Count == 0)
        {
            opaqueCandidates = stats.ToList();
        }

        var semiTransparentCandidates = stats
            .Where(s => s.OpaqueRatio >= 0.18f && s.OpaqueRatio < 0.92f)
            .ToList();

        Func<TileStat, float> groundScore;
        Func<TileStat, float> detailScore;
        Func<TileStat, float> obstacleScore;

        switch (theme)
        {
            case MapTheme.Desert:
                groundScore = s => (s.Warmth * 1.6f) + (s.Brightness * 0.35f) - (s.Contrast * 0.25f);
                detailScore = s => (s.Warmth * 1.2f) + (s.Contrast * 0.9f);
                obstacleScore = s => (s.Contrast * 1.4f) + (s.Warmth * 0.55f) - (s.Brightness * 0.15f);
                break;
            case MapTheme.Snow:
                groundScore = s => (s.Brightness * 1.6f) - (s.Saturation * 220f) - (s.Contrast * 0.15f);
                detailScore = s => (s.Brightness * 0.8f) + (s.Contrast * 1.25f);
                obstacleScore = s => (s.Contrast * 1.35f) + ((1f - s.Saturation) * 120f);
                break;
            default:
                groundScore = s => (s.GreenBias * 2f) + (s.Brightness * 0.22f) - (s.Contrast * 0.25f);
                detailScore = s => (s.GreenBias * 1.15f) + (s.Contrast * 1.1f);
                obstacleScore = s => (s.Contrast * 1.5f) - (s.GreenBias * 0.35f);
                break;
        }

        var groundTiles = opaqueCandidates
            .OrderByDescending(groundScore)
            .Select(s => s.Sprite)
            .Distinct()
            .Take(4)
            .ToList();

        if (groundTiles.Count == 0 && stats.Count > 0)
        {
            groundTiles.Add(stats[0].Sprite);
        }

        var detailTiles = opaqueCandidates
            .Where(s => !groundTiles.Contains(s.Sprite))
            .OrderByDescending(detailScore)
            .Select(s => s.Sprite)
            .Distinct()
            .Take(4)
            .ToList();

        if (detailTiles.Count == 0)
        {
            detailTiles.AddRange(groundTiles.Take(2));
        }

        var obstacleSource = semiTransparentCandidates.Count > 0
            ? semiTransparentCandidates
            : stats.Where(s => !groundTiles.Contains(s.Sprite)).ToList();

        var obstacleTiles = obstacleSource
            .OrderByDescending(obstacleScore)
            .Select(s => s.Sprite)
            .Distinct()
            .Take(5)
            .ToList();

        if (obstacleTiles.Count == 0)
        {
            obstacleTiles.AddRange(groundTiles.Take(1));
        }

        return new TileSelection
        {
            GroundTiles = groundTiles.ToArray(),
            DetailTiles = detailTiles.ToArray(),
            PropsTiles = detailTiles.ToArray(),
            ObstacleTiles = obstacleTiles.ToArray()
        };
    }

    private static TileSelection SelectSimpleTopdownTiles(MapTheme theme, IReadOnlyList<TileStat> stats)
    {
        if (stats == null || stats.Count == 0)
        {
            return default;
        }

        int colMin;
        int colMax;
        switch (theme)
        {
            case MapTheme.Desert:
                colMin = 10;
                colMax = 19;
                break;
            case MapTheme.Snow:
                colMin = 20;
                colMax = 29;
                break;
            default:
                colMin = 0;
                colMax = 9;
                break;
        }

        bool InBand(TileStat s, int rowMin, int rowMax)
        {
            if (s.Index < 0)
            {
                return false;
            }

            int col = s.Index % SimpleTopdownCols;
            int row = s.Index / SimpleTopdownCols;
            return col >= colMin && col <= colMax && row >= rowMin && row <= rowMax;
        }

        // Row 0 contains utility/edge sprites in this pack, so skip it for map ground/detail.
        var topBand = stats.Where(s => InBand(s, 1, 9)).ToList();
        var bottomBand = stats.Where(s => InBand(s, 10, 19)).ToList();
        if (topBand.Count == 0)
        {
            return default;
        }

        Func<TileStat, float> groundScore;
        Func<TileStat, float> detailScore;
        switch (theme)
        {
            case MapTheme.Desert:
                groundScore = s => (s.Warmth * 1.4f) + (s.Brightness * 0.9f) - (s.Contrast * 0.45f) - (s.EdgeDelta * 0.9f);
                detailScore = s => (s.Warmth * 1.1f) + (s.Contrast * 0.95f);
                break;
            case MapTheme.Snow:
                groundScore = s => (s.Brightness * 1.2f) + ((1f - s.Saturation) * 120f) - (s.Contrast * 0.4f) - (s.EdgeDelta * 0.9f);
                detailScore = s => (s.Brightness * 0.9f) + (s.Contrast * 0.8f);
                break;
            default:
                groundScore = s => (s.GreenBias * 1.3f) + (s.Brightness * 1.0f) - (s.Contrast * 0.45f) - (s.EdgeDelta * 0.9f);
                detailScore = s => (s.GreenBias * 0.7f) + (s.Contrast * 1.0f);
                break;
        }

        // Curated flat-ground anchor for this specific tileset.
        // Keep one fully filled tile as the base to avoid repeated edge artifacts.
        int[] curatedGroundIndices = GetSimpleTopdownGroundIndices(theme);
        var groundTiles = ResolveSpritesByIndices(stats, curatedGroundIndices).Take(4).ToList();
        if (groundTiles.Count == 0)
        {
            var groundPool = topBand
                .Where(s => s.OpaqueRatio >= 0.9f
                    && s.Contrast <= 72f
                    && s.Brightness >= 120f
                    && s.EdgeDelta <= 34f
                    && s.BorderOpaqueRatio >= 0.82f)
                .ToList();
            if (groundPool.Count < 4)
            {
                groundPool = topBand
                    .Where(s => s.OpaqueRatio >= 0.85f
                        && s.Brightness >= 95f
                        && s.EdgeDelta <= 48f
                        && s.BorderOpaqueRatio >= 0.72f)
                    .ToList();
            }

            groundTiles = groundPool
                .OrderByDescending(groundScore)
                .Select(s => s.Sprite)
                .Distinct()
                .Take(4)
                .ToList();
        }

        if (groundTiles.Count == 0)
        {
            groundTiles.Add(topBand.OrderByDescending(groundScore).First().Sprite);
        }

        int[] curatedDetailIndices = GetSimpleTopdownDetailIndices(theme);
        var detailTiles = ResolveSpritesByIndices(stats, curatedDetailIndices).Take(4).ToList();
        if (detailTiles.Count == 0)
        {
            detailTiles = topBand
                .Where(s => !groundTiles.Contains(s.Sprite) && s.OpaqueRatio >= 0.7f && s.Contrast >= 18f)
                .OrderByDescending(detailScore)
                .Select(s => s.Sprite)
                .Distinct()
                .Take(2)
                .ToList();
        }

        int[] curatedObstacleIndices = GetSimpleTopdownObstacleIndices(theme);
        var curatedObstacleTiles = ResolveSpritesByIndices(stats, curatedObstacleIndices);
        int[] curatedPropsIndices = GetSimpleTopdownPropsIndices(theme);
        var curatedPropsTiles = ResolveSpritesByIndices(stats, curatedPropsIndices);

        var obstaclePool = bottomBand
            .Where(s => s.OpaqueRatio >= 0.12f && s.Contrast >= 20f)
            .ToList();
        if (obstaclePool.Count == 0)
        {
            obstaclePool = topBand
                .Where(s => !groundTiles.Contains(s.Sprite) && s.Contrast >= 28f)
                .ToList();
        }

        var obstacleTiles = obstaclePool
            .OrderByDescending(s => s.Contrast * 1.2f + s.OpaqueRatio * 100f)
            .Select(s => s.Sprite)
            .Distinct()
            .Take(3)
            .ToList();

        if (curatedObstacleTiles.Count > 0)
        {
            obstacleTiles = curatedObstacleTiles.Take(3).ToList();
        }
        else if (obstacleTiles.Count == 0)
        {
            obstacleTiles.Add(groundTiles[0]);
        }

        return new TileSelection
        {
            GroundTiles = groundTiles.ToArray(),
            DetailTiles = detailTiles.ToArray(),
            PropsTiles = curatedPropsTiles.Count > 0 ? curatedPropsTiles.ToArray() : detailTiles.ToArray(),
            ObstacleTiles = obstacleTiles.ToArray()
        };
    }

    private static int[] GetSimpleTopdownGroundIndices(MapTheme theme)
    {
        // Flat ground-only anchors. Keep this to fully filled tiles
        // so we avoid repeated edge/corner silhouettes on open fields.
        int colOffset = 0;
        switch (theme)
        {
            case MapTheme.Desert:
                colOffset = 10;
                break;
            case MapTheme.Snow:
                colOffset = 20;
                break;
        }

        return new[]
        {
            211 + colOffset
        };
    }

    private static int[] GetSimpleTopdownDetailIndices(MapTheme theme)
    {
        int colOffset = 0;
        switch (theme)
        {
            case MapTheme.Desert:
                colOffset = 10;
                break;
            case MapTheme.Snow:
                colOffset = 20;
                break;
        }

        // Small doodads only; avoid edge/corner transition tiles for scatter details.
        return new[]
        {
            154 + colOffset,
            155 + colOffset
        };
    }

    private static int[] GetSimpleTopdownPropsIndices(MapTheme theme)
    {
        switch (theme)
        {
            case MapTheme.Desert:
                return new[]
                {
                    163, 164, 165,
                    128, 129,
                    410, 411, 414, 415,
                    443, 444, 447,
                    457, 458
                };
            case MapTheme.Snow:
                return new[]
                {
                    173, 174, 175,
                    128, 129,
                    412, 413,
                    476, 477,
                    536, 537,
                    566, 567
                };
            default:
                return new[]
                {
                    153, 154, 155,
                    128, 129,
                    410, 411, 414, 415,
                    443, 444, 447,
                    475, 535, 565
                };
        }
    }

    private static int[] GetSimpleTopdownObstacleIndices(MapTheme theme)
    {
        // Neutral props that read cleanly in all themes.
        return new[]
        {
            128,
            129
        };
    }

    private static List<Sprite> ResolveSpritesByIndices(IReadOnlyList<TileStat> stats, IReadOnlyList<int> indices)
    {
        var result = new List<Sprite>();
        if (stats == null || indices == null || indices.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < indices.Count; i++)
        {
            int index = indices[i];
            for (int s = 0; s < stats.Count; s++)
            {
                var stat = stats[s];
                if (stat.Index != index || stat.Sprite == null)
                {
                    continue;
                }

                if (!result.Contains(stat.Sprite))
                {
                    result.Add(stat.Sprite);
                }
                break;
            }
        }

        return result;
    }

    private static TileStat AnalyzeTile(Sprite sprite, Rect rect, Color32[] pixels, int texWidth, int texHeight, int spriteIndex)
    {
        int xMin = Mathf.Clamp(Mathf.FloorToInt(rect.xMin), 0, Mathf.Max(0, texWidth - 1));
        int yMin = Mathf.Clamp(Mathf.FloorToInt(rect.yMin), 0, Mathf.Max(0, texHeight - 1));
        int xMax = Mathf.Clamp(Mathf.CeilToInt(rect.xMax), xMin + 1, texWidth);
        int yMax = Mathf.Clamp(Mathf.CeilToInt(rect.yMax), yMin + 1, texHeight);
        int width = Mathf.Max(1, xMax - xMin);
        int height = Mathf.Max(1, yMax - yMin);

        int count = width * height;
        int opaque = 0;
        long sumR = 0;
        long sumG = 0;
        long sumB = 0;
        float edgeBrightnessSum = 0f;
        float centerBrightnessSum = 0f;
        int edgeOpaque = 0;
        int centerOpaque = 0;
        int borderPixelCount = 0;
        int borderOpaqueCount = 0;
        int borderThickness = Mathf.Max(1, Mathf.Min(width, height) / 5);

        for (int y = 0; y < height; y++)
        {
            int row = (yMin + y) * texWidth;
            for (int x = 0; x < width; x++)
            {
                var c = pixels[row + xMin + x];
                sumR += c.r;
                sumG += c.g;
                sumB += c.b;
                bool isBorder = x < borderThickness || y < borderThickness || x >= width - borderThickness || y >= height - borderThickness;
                if (isBorder)
                {
                    borderPixelCount++;
                    if (c.a > 10)
                    {
                        borderOpaqueCount++;
                    }
                }
                if (c.a > 10)
                {
                    opaque++;
                    float bright = c.r + c.g + c.b;
                    if (isBorder)
                    {
                        edgeBrightnessSum += bright;
                        edgeOpaque++;
                    }
                    else
                    {
                        centerBrightnessSum += bright;
                        centerOpaque++;
                    }
                }
            }
        }

        float avgR = sumR / (float)count;
        float avgG = sumG / (float)count;
        float avgB = sumB / (float)count;
        float greenBias = avgG - (avgR + avgB) * 0.5f;
        float brightness = avgR + avgG + avgB;
        float warmth = (avgR + avgG) - avgB * 0.5f;
        float maxChannel = Mathf.Max(avgR, Mathf.Max(avgG, avgB));
        float minChannel = Mathf.Min(avgR, Mathf.Min(avgG, avgB));
        float contrast = maxChannel - minChannel;
        float edgeAvgBrightness = edgeOpaque > 0 ? edgeBrightnessSum / edgeOpaque : brightness;
        float centerAvgBrightness = centerOpaque > 0 ? centerBrightnessSum / centerOpaque : brightness;
        float edgeDelta = Mathf.Abs(edgeAvgBrightness - centerAvgBrightness);
        float borderOpaqueRatio = borderPixelCount > 0 ? borderOpaqueCount / (float)borderPixelCount : 0f;
        Color.RGBToHSV(new Color(avgR / 255f, avgG / 255f, avgB / 255f), out _, out float saturation, out _);

        return new TileStat
        {
            Sprite = sprite,
            Index = spriteIndex,
            GreenBias = greenBias,
            Brightness = brightness,
            Warmth = warmth,
            OpaqueRatio = opaque / (float)count,
            Contrast = contrast,
            Saturation = saturation,
            EdgeDelta = edgeDelta,
            BorderOpaqueRatio = borderOpaqueRatio
        };
    }

    private static int ExtractSpriteIndex(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            return -1;
        }

        int sep = spriteName.LastIndexOf('_');
        if (sep >= 0 && sep + 1 < spriteName.Length
            && int.TryParse(spriteName.Substring(sep + 1), out int underscored))
        {
            return underscored;
        }

        int end = spriteName.Length - 1;
        while (end >= 0 && char.IsDigit(spriteName[end]))
        {
            end--;
        }

        if (end < spriteName.Length - 1)
        {
            var digits = spriteName.Substring(end + 1);
            if (int.TryParse(digits, out int trailing))
            {
                return trailing;
            }
        }

        return -1;
    }

    private static TileBase[] EnsureTileAssets(string namePrefix, IReadOnlyList<Sprite> sprites, Tile.ColliderType colliderType, string tilesFolder)
    {
        if (sprites == null || sprites.Count == 0)
        {
            return Array.Empty<TileBase>();
        }

        if (!AssetDatabase.IsValidFolder(tilesFolder))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, tilesFolder.Replace("Assets/", string.Empty)));
            AssetDatabase.Refresh();
        }

        var result = new List<TileBase>(sprites.Count);
        for (int i = 0; i < sprites.Count; i++)
        {
            var sprite = sprites[i];
            if (sprite == null)
            {
                continue;
            }

            string path = $"{tilesFolder}/{namePrefix}_{i:D2}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = colliderType;
            EditorUtility.SetDirty(tile);
            result.Add(tile);
        }

        AssetDatabase.SaveAssets();
        return result.ToArray();
    }

    private static Tilemap CreateTilemap(GameObject gridGo, string name, int sortingOrder, bool render)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(gridGo.transform, false);

        var renderer = go.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        renderer.enabled = render;

        return go.GetComponent<Tilemap>();
    }

    private static void SetupCollision(Tilemap collisionMap)
    {
        var collider = collisionMap.gameObject.AddComponent<TilemapCollider2D>();
        collider.compositeOperation = Collider2D.CompositeOperation.Merge;

        var body = collisionMap.gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;

        collisionMap.gameObject.AddComponent<CompositeCollider2D>();
    }

    private static (int Width, int Height) ResolveMapSize()
    {
        int halfX = 24;
        int halfY = 24;

        var config = GameConfig.LoadDefault();
        if (config != null)
        {
            halfX = Mathf.RoundToInt(config.game.mapHalfSize.x);
            halfY = Mathf.RoundToInt(config.game.mapHalfSize.y);
        }

        return (Mathf.Max(8, halfX * 2), Mathf.Max(8, halfY * 2));
    }

    private static MapRichness ResolveRichness(MapTheme theme)
    {
        switch (theme)
        {
            case MapTheme.Desert:
                return new MapRichness
                {
                    DetailDensity = 0.034f,
                    PropsDensity = 0.062f,
                    ObstaclePerimeterFactor = 0.28f
                };
            case MapTheme.Snow:
                return new MapRichness
                {
                    DetailDensity = 0.032f,
                    PropsDensity = 0.058f,
                    ObstaclePerimeterFactor = 0.26f
                };
            default:
                return new MapRichness
                {
                    DetailDensity = 0.040f,
                    PropsDensity = 0.068f,
                    ObstaclePerimeterFactor = 0.32f
                };
        }
    }

    private static void FillGround(Tilemap map, TileBase tile, int width, int height)
    {
        int halfX = width / 2;
        int halfY = height / 2;
        var bounds = new BoundsInt(-halfX, -halfY, 0, width, height, 1);
        var tiles = new TileBase[width * height];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile;
        }
        map.SetTilesBlock(bounds, tiles);
    }

    private static void FillGroundPattern(Tilemap map, TileBase[] palette, int width, int height, int seed, MapTheme theme)
    {
        if (palette == null || palette.Length == 0)
        {
            return;
        }

        FillGround(map, palette[0], width, height);

        int halfX = width / 2;
        int halfY = height / 2;
        float sx = seed * 0.0013f;
        float sy = seed * 0.0017f;
        const float warpFreq = 0.031f;
        const float coarseFreq = 0.053f;
        const float mediumFreq = 0.107f;
        const float fineFreq = 0.217f;
        const float macroFreq = 0.014f;
        const float warpStrength = 9f;

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                float warpX = (Mathf.PerlinNoise((x + sx * 0.6f) * warpFreq, (y - sy * 0.5f) * warpFreq) - 0.5f) * warpStrength;
                float warpY = (Mathf.PerlinNoise((x - sx * 0.4f) * warpFreq, (y + sy * 0.7f) * warpFreq) - 0.5f) * warpStrength;
                float wx = x + warpX;
                float wy = y + warpY;

                float coarse = Mathf.PerlinNoise((wx + sx) * coarseFreq, (wy + sy) * coarseFreq);
                float medium = Mathf.PerlinNoise((wx - sy * 0.7f) * mediumFreq, (wy + sx * 0.5f) * mediumFreq);
                float fine = Mathf.PerlinNoise((wx + sx * 0.3f) * fineFreq, (wy - sy * 0.3f) * fineFreq);
                float macro = Mathf.PerlinNoise((x + sx * 1.2f) * macroFreq, (y + sy * 1.1f) * macroFreq);
                float grain = Hash01(x, y, seed) * 0.18f - 0.09f;
                float mixed = coarse * 0.45f + medium * 0.34f + fine * 0.15f + macro * 0.06f + grain;
                mixed = Mathf.Clamp01(mixed);

                int index = 0;
                if (palette.Length <= 1)
                {
                    index = 0;
                }
                else if (palette.Length == 2)
                {
                    float t1 = Mathf.Lerp(0.68f, 0.77f, macro);
                    index = mixed > t1 ? 1 : 0;
                }
                else if (palette.Length == 3)
                {
                    float t1 = Mathf.Lerp(0.58f, 0.69f, macro);
                    float t2 = Mathf.Lerp(0.80f, 0.90f, macro);
                    if (mixed > t2) index = 2;
                    else if (mixed > t1) index = 1;
                    else index = 0;
                }
                else
                {
                    float t1 = Mathf.Lerp(0.56f, 0.66f, macro);
                    float t2 = Mathf.Lerp(0.76f, 0.86f, macro);
                    float t3 = Mathf.Lerp(0.90f, 0.96f, macro);
                    if (mixed > t3) index = 3;
                    else if (mixed > t2) index = 2;
                    else if (mixed > t1) index = 1;
                    else index = 0;
                }

                // Rare local variation break-up to avoid visible contour lines.
                if (palette.Length > 2 && Hash01(x + 19, y - 37, seed * 3 + 11) > 0.94f)
                {
                    index = Mathf.Min(palette.Length - 1, index + 1);
                }

                var pos = new Vector3Int(x, y, 0);
                map.SetTile(pos, palette[index]);
                map.SetTileFlags(pos, TileFlags.None);
                map.SetTransformMatrix(pos, BuildGroundTransform(x, y, seed));
                map.SetColor(pos, BuildGroundTint(x, y, seed, theme));
            }
        }
    }

    private static Matrix4x4 BuildGroundTransform(int x, int y, int seed)
    {
        int rotStep = Mathf.FloorToInt(Hash01(x + 17, y - 29, seed * 5 + 7) * 4f) % 4;
        bool flipX = Hash01(x - 41, y + 73, seed * 11 + 3) > 0.5f;
        float angle = rotStep * 90f;
        var scale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
        return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), scale);
    }

    private static Color BuildGroundTint(int x, int y, int seed, MapTheme theme)
    {
        float n0 = Hash01(x + 11, y - 17, seed * 13 + 5) * 2f - 1f;
        float n1 = Hash01(x - 23, y + 31, seed * 7 + 19) * 2f - 1f;
        float luma = Mathf.Clamp(1f + n0 * 0.05f + n1 * 0.03f, 0.9f, 1.1f);

        Color baseTint;
        switch (theme)
        {
            case MapTheme.Desert:
                baseTint = new Color(1.04f, 1f, 0.96f, 1f);
                break;
            case MapTheme.Snow:
                baseTint = new Color(0.96f, 0.99f, 1.04f, 1f);
                break;
            default:
                baseTint = new Color(0.97f, 1.03f, 0.97f, 1f);
                break;
        }

        var tinted = baseTint * luma;
        tinted.a = 1f;
        return tinted;
    }

    private static void ScatterDetails(Tilemap map, TileBase[] detailPalette, int width, int height, float density, int seed)
    {
        if (map == null || detailPalette == null || detailPalette.Length == 0)
        {
            return;
        }

        var rng = new System.Random(seed);
        int halfX = width / 2;
        int halfY = height / 2;
        float sx = seed * 0.0021f;
        float sy = seed * 0.0011f;
        const float clusterFreq = 0.062f;
        const float noiseFreq = 0.19f;
        float threshold = Mathf.Lerp(0.90f, 0.62f, Mathf.Clamp01(density * 14f));

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                // Keep center readable for combat and avoid visual clutter at spawn.
                if (Mathf.Abs(x) <= 2 && Mathf.Abs(y) <= 2)
                {
                    continue;
                }

                float cluster = Mathf.PerlinNoise((x + sx * 0.4f) * clusterFreq, (y + sy * 0.6f) * clusterFreq);
                if (cluster < 0.34f)
                {
                    continue;
                }

                float noise = Mathf.PerlinNoise((x + sx) * noiseFreq, (y + sy) * noiseFreq);
                float localThreshold = threshold + (0.56f - cluster) * 0.16f;
                bool byNoise = noise > localThreshold;
                bool byRandom = rng.NextDouble() < density * 0.24f * Mathf.Lerp(0.8f, 1.35f, cluster);
                if (byNoise || byRandom)
                {
                    map.SetTile(new Vector3Int(x, y, 0), PickPaletteTile(detailPalette, x, y, seed));
                }
            }
        }
    }

    private static void ScatterProps(Tilemap map, TileBase[] propsPalette, int width, int height, float density, int seed)
    {
        if (map == null || propsPalette == null || propsPalette.Length == 0 || density <= 0f)
        {
            return;
        }

        var rng = new System.Random(seed);
        int halfX = width / 2;
        int halfY = height / 2;
        float sx = seed * 0.0019f;
        float sy = seed * 0.0013f;
        const float macroFreq = 0.055f;
        const float noiseFreq = 0.17f;
        float threshold = Mathf.Lerp(0.92f, 0.60f, Mathf.Clamp01(density * 10f));

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                if (Mathf.Abs(x) <= 4 && Mathf.Abs(y) <= 4)
                {
                    continue;
                }

                float macro = Mathf.PerlinNoise((x + sx * 0.6f) * macroFreq, (y + sy * 0.5f) * macroFreq);
                if (macro < 0.30f)
                {
                    continue;
                }

                float noise = Mathf.PerlinNoise((x - sx) * noiseFreq, (y + sy) * noiseFreq);
                bool byNoise = noise > threshold + (0.54f - macro) * 0.14f;
                bool byRandom = rng.NextDouble() < density * 0.14f * Mathf.Lerp(0.8f, 1.2f, macro);
                if (byNoise || byRandom)
                {
                    map.SetTile(new Vector3Int(x, y, 0), PickPaletteTile(propsPalette, x, y, seed));
                }
            }
        }
    }

    private static void PlaceObstacles(Tilemap obstacles, Tilemap collision, TileBase[] obstaclePalette, int width, int height, int count, int seed)
    {
        if (obstacles == null || collision == null || obstaclePalette == null || obstaclePalette.Length == 0)
        {
            return;
        }

        var rng = new System.Random(seed);
        int halfX = width / 2;
        int halfY = height / 2;
        var occupied = new HashSet<Vector3Int>();

        void PlaceAt(int x, int y)
        {
            if (Mathf.Abs(x) <= 4 && Mathf.Abs(y) <= 4)
            {
                return;
            }

            var pos = new Vector3Int(x, y, 0);
            if (!occupied.Add(pos))
            {
                return;
            }

            var tile = PickPaletteTile(obstaclePalette, x, y, seed);
            obstacles.SetTile(pos, tile);
            collision.SetTile(pos, tile);
        }

        int clusterCount = Mathf.Clamp(count / 5, 4, 18);
        for (int c = 0; c < clusterCount; c++)
        {
            int cx = rng.Next(-halfX + 5, halfX - 5);
            int cy = rng.Next(-halfY + 5, halfY - 5);
            int radius = rng.Next(1, 4);

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    float d = Mathf.Sqrt(x * x + y * y);
                    if (d > radius + 0.15f)
                    {
                        continue;
                    }

                    if (rng.NextDouble() < (0.9d - d * 0.22d))
                    {
                        PlaceAt(cx + x, cy + y);
                    }
                }
            }
        }

        while (occupied.Count < count)
        {
            int x = rng.Next(-halfX + 4, halfX - 4);
            int y = rng.Next(-halfY + 4, halfY - 4);
            if (rng.NextDouble() > 0.22d)
            {
                PlaceAt(x, y);
            }
        }
    }

    private static void CarveMainPaths(
        Tilemap ground,
        Tilemap details,
        Tilemap props,
        Tilemap obstacles,
        Tilemap collision,
        TileBase[] groundPalette,
        int width,
        int height,
        int seed)
    {
        if (ground == null || groundPalette == null || groundPalette.Length == 0)
        {
            return;
        }

        var baseGround = groundPalette[0];
        int halfX = width / 2;
        int halfY = height / 2;
        int pathRadius = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(width, height) * 0.045f), 2, 4);
        float sx = seed * 0.0017f;
        float sy = seed * 0.0023f;

        for (int x = -halfX + 1; x < halfX; x++)
        {
            float wave = Mathf.PerlinNoise((x + sx) * 0.08f, sy * 0.11f);
            int yCenter = Mathf.RoundToInt(Mathf.Lerp(-2f, 2f, wave));
            CarveCell(ground, details, props, obstacles, collision, baseGround, x, yCenter, pathRadius);
        }

        for (int y = -halfY + 1; y < halfY; y++)
        {
            float wave = Mathf.PerlinNoise((y - sy) * 0.08f, sx * 0.13f);
            int xCenter = Mathf.RoundToInt(Mathf.Lerp(-2f, 2f, wave));
            CarveCell(ground, details, props, obstacles, collision, baseGround, xCenter, y, pathRadius);
        }

        // One diagonal connector improves movement flow between quadrants.
        int length = Mathf.Min(halfX, halfY) - 2;
        for (int i = -length; i <= length; i++)
        {
            int x = i;
            int y = Mathf.RoundToInt(i * 0.6f);
            CarveCell(ground, details, props, obstacles, collision, baseGround, x, y, Mathf.Max(1, pathRadius - 1));
        }
    }

    private static void CarveCell(
        Tilemap ground,
        Tilemap details,
        Tilemap props,
        Tilemap obstacles,
        Tilemap collision,
        TileBase baseGround,
        int cx,
        int cy,
        int radius)
    {
        int r = Mathf.Max(1, radius);
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                float d = Mathf.Sqrt(x * x + y * y);
                if (d > r + 0.12f)
                {
                    continue;
                }

                var pos = new Vector3Int(cx + x, cy + y, 0);
                ground.SetTile(pos, baseGround);
                if (details != null)
                {
                    details.SetTile(pos, null);
                }

                if (props != null)
                {
                    props.SetTile(pos, null);
                }

                if (obstacles != null)
                {
                    obstacles.SetTile(pos, null);
                }

                if (collision != null)
                {
                    collision.SetTile(pos, null);
                }
            }
        }
    }

    private static TileBase PickPaletteTile(IReadOnlyList<TileBase> palette, int x, int y, int seed)
    {
        if (palette == null || palette.Count == 0)
        {
            return null;
        }

        if (palette.Count == 1)
        {
            return palette[0];
        }

        float n = Hash01(x, y, seed);
        int index = Mathf.Clamp(Mathf.FloorToInt(n * palette.Count), 0, palette.Count - 1);
        return palette[index];
    }

    private static float Hash01(int x, int y, int seed)
    {
        uint h = 2166136261u;
        h = (h ^ (uint)(x * 374761393)) * 16777619u;
        h = (h ^ (uint)(y * 668265263)) * 16777619u;
        h = (h ^ (uint)(seed * 2147483647)) * 16777619u;
        h ^= h >> 13;
        h *= 1274126177u;
        h ^= h >> 16;
        return (h & 0x00FFFFFF) / 16777215f;
    }

    private static void ClearSpawnArea(Tilemap details, Tilemap props, Tilemap obstacles, Tilemap collision, int radius)
    {
        int r = Mathf.Max(1, radius);
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (details != null)
                {
                    details.SetTile(pos, null);
                }

                if (props != null)
                {
                    props.SetTile(pos, null);
                }

                if (obstacles != null)
                {
                    obstacles.SetTile(pos, null);
                }

                if (collision != null)
                {
                    collision.SetTile(pos, null);
                }
            }
        }
    }

    private struct TileSelection
    {
        public Sprite[] GroundTiles;
        public Sprite[] DetailTiles;
        public Sprite[] PropsTiles;
        public Sprite[] ObstacleTiles;
    }

    private struct MapConfig
    {
        public MapTheme Theme;
        public string TilesetTexturePath;
        public string TilesFolder;
        public string ScenePath;
        public string SpritePrefix;
        public int TileSize;
        public int Spacing;
        public int Margin;
    }

    private struct MapRichness
    {
        public float DetailDensity;
        public float PropsDensity;
        public float ObstaclePerimeterFactor;
    }

    private struct TileStat
    {
        public Sprite Sprite;
        public int Index;
        public float GreenBias;
        public float Brightness;
        public float Warmth;
        public float OpaqueRatio;
        public float Contrast;
        public float Saturation;
        public float EdgeDelta;
        public float BorderOpaqueRatio;
    }
}

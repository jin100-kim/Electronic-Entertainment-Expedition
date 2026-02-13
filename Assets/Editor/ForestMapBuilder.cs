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

    private static readonly MapConfig ForestConfig = new MapConfig
    {
        Theme = MapTheme.Forest,
        TilesetTexturePath = "Assets/Art/Tilesets/ForestPlains/tileset_plains_070419_zp.png",
        TilesFolder = "Assets/Art/Tilesets/ForestPlains/Tiles",
        ScenePath = "Assets/Scenes/ForestOpenWorld.unity",
        SpritePrefix = "ForestPlains",
        TileSize = 16,
        Spacing = 2,
        Margin = 1
    };

    private static readonly MapConfig DesertConfig = new MapConfig
    {
        Theme = MapTheme.Desert,
        TilesetTexturePath = "Assets/Art/Tilesets/Desert/desert_tileset.png",
        TilesFolder = "Assets/Art/Tilesets/Desert/Tiles",
        ScenePath = "Assets/Scenes/DesertOpenWorld.unity",
        SpritePrefix = "Desert",
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    private static readonly MapConfig SnowConfig = new MapConfig
    {
        Theme = MapTheme.Snow,
        TilesetTexturePath = "Assets/Art/Tilesets/Snow/snow_tileset.png",
        TilesFolder = "Assets/Art/Tilesets/Snow/Tiles",
        ScenePath = "Assets/Scenes/SnowOpenWorld.unity",
        SpritePrefix = "Snow",
        TileSize = 16,
        Spacing = 0,
        Margin = 0
    };

    [MenuItem("Tools/Map/Build Forest Open World")]
    private static void BuildForestOpenWorld()
    {
        BuildOpenWorld(ForestConfig);
    }

    [MenuItem("Tools/Map/Build Desert Open World")]
    private static void BuildDesertOpenWorld()
    {
        BuildOpenWorld(DesertConfig);
    }

    [MenuItem("Tools/Map/Build Snow Open World")]
    private static void BuildSnowOpenWorld()
    {
        BuildOpenWorld(SnowConfig);
    }

    [MenuItem("Tools/Map/Build All Open Worlds")]
    private static void BuildAllOpenWorlds()
    {
        BuildOpenWorld(ForestConfig);
        BuildOpenWorld(DesertConfig);
        BuildOpenWorld(SnowConfig);
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
        if (selection.Ground == null || selection.Obstacle == null)
        {
            Debug.LogError("Failed to select ground/obstacle tiles from the tileset.");
            return;
        }

        string prefix = config.Theme.ToString();
        var groundTile = EnsureTileAsset($"{prefix}_Ground", selection.Ground, Tile.ColliderType.None, config.TilesFolder);
        var detailTile = EnsureTileAsset($"{prefix}_Detail", selection.Detail ?? selection.Ground, Tile.ColliderType.None, config.TilesFolder);
        var obstacleTile = EnsureTileAsset($"{prefix}_Obstacle", selection.Obstacle, Tile.ColliderType.Sprite, config.TilesFolder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = Path.GetFileNameWithoutExtension(config.ScenePath);

        var gridGo = new GameObject("Grid", typeof(Grid));
        gridGo.transform.position = Vector3.zero;

        var groundMap = CreateTilemap(gridGo, "Ground", -5, true);
        var detailMap = CreateTilemap(gridGo, "Details", -4, true);
        var obstacleMap = CreateTilemap(gridGo, "Obstacles", -3, true);
        var collisionMap = CreateTilemap(gridGo, "Collision", -2, false);

        SetupCollision(collisionMap);

        var size = ResolveMapSize();
        int seedBase = ((int)config.Theme + 1) * 7331;
        int obstacleCount = Mathf.Max(24, Mathf.RoundToInt((size.Width + size.Height) * 0.55f));

        FillGroundPattern(groundMap, groundTile, detailTile, size.Width, size.Height, seedBase);
        ScatterDetails(detailMap, detailTile, size.Width, size.Height, density: 0.055f, seed: seedBase + 101);
        PlaceObstacles(obstacleMap, collisionMap, obstacleTile, size.Width, size.Height, obstacleCount, seedBase + 211);
        ClearSpawnArea(detailMap, obstacleMap, collisionMap, radius: 4);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, config.ScenePath);
        Debug.Log($"{scene.name} scene saved: {config.ScenePath}");
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

        var stats = new List<TileStat>();
        foreach (var sprite in sprites)
        {
            var rect = sprite.textureRect;
            stats.Add(AnalyzeTile(sprite, rect, pixels, texWidth));
        }

        var opaqueCandidates = stats
            .Where(s => s.OpaqueRatio >= 0.9f)
            .ToList();

        if (opaqueCandidates.Count == 0)
        {
            opaqueCandidates = stats.ToList();
        }

        TileStat ground;
        TileStat detail;
        TileStat obstacle;

        switch (theme)
        {
            case MapTheme.Desert:
                ground = opaqueCandidates.OrderByDescending(s => s.Warmth).FirstOrDefault();
                detail = opaqueCandidates.OrderByDescending(s => s.Warmth).Skip(1).FirstOrDefault();
                obstacle = opaqueCandidates.Where(s => s.Sprite != ground.Sprite).OrderBy(s => s.Brightness).FirstOrDefault();
                break;
            case MapTheme.Snow:
                ground = opaqueCandidates.OrderByDescending(s => s.Brightness).FirstOrDefault();
                detail = opaqueCandidates.OrderByDescending(s => s.Brightness).Skip(1).FirstOrDefault();
                obstacle = opaqueCandidates.Where(s => s.Sprite != ground.Sprite).OrderBy(s => s.Brightness).FirstOrDefault();
                break;
            default:
                ground = opaqueCandidates.OrderByDescending(s => s.GreenBias).FirstOrDefault();
                detail = opaqueCandidates.OrderByDescending(s => s.GreenBias).Skip(1).FirstOrDefault();
                obstacle = opaqueCandidates.Where(s => s.Sprite != ground.Sprite).OrderBy(s => s.GreenBias).FirstOrDefault();
                break;
        }

        if (detail.Sprite == null)
        {
            detail = ground;
        }

        if (obstacle.Sprite == null)
        {
            obstacle = stats.FirstOrDefault(s => s.Sprite != ground.Sprite);
        }

        return new TileSelection
        {
            Ground = ground.Sprite,
            Detail = detail.Sprite,
            Obstacle = obstacle.Sprite
        };
    }

    private static TileStat AnalyzeTile(Sprite sprite, Rect rect, Color32[] pixels, int texWidth)
    {
        int xMin = Mathf.RoundToInt(rect.x);
        int yMin = Mathf.RoundToInt(rect.y);
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        int count = width * height;
        int opaque = 0;
        long sumR = 0;
        long sumG = 0;
        long sumB = 0;

        for (int y = 0; y < height; y++)
        {
            int row = (yMin + y) * texWidth;
            for (int x = 0; x < width; x++)
            {
                var c = pixels[row + xMin + x];
                sumR += c.r;
                sumG += c.g;
                sumB += c.b;
                if (c.a > 10)
                {
                    opaque++;
                }
            }
        }

        float avgR = sumR / (float)count;
        float avgG = sumG / (float)count;
        float avgB = sumB / (float)count;
        float greenBias = avgG - (avgR + avgB) * 0.5f;
        float brightness = avgR + avgG + avgB;
        float warmth = (avgR + avgG) - avgB * 0.5f;

        return new TileStat
        {
            Sprite = sprite,
            GreenBias = greenBias,
            Brightness = brightness,
            Warmth = warmth,
            OpaqueRatio = opaque / (float)count
        };
    }

    private static Tile EnsureTileAsset(string name, Sprite sprite, Tile.ColliderType colliderType, string tilesFolder)
    {
        if (!AssetDatabase.IsValidFolder(tilesFolder))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, tilesFolder.Replace("Assets/", string.Empty)));
            AssetDatabase.Refresh();
        }

        string path = $"{tilesFolder}/{name}.asset";
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
        AssetDatabase.SaveAssets();
        return tile;
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

    private static void FillGroundPattern(Tilemap map, TileBase primary, TileBase secondary, int width, int height, int seed)
    {
        FillGround(map, primary, width, height);
        if (secondary == null || secondary == primary)
        {
            return;
        }

        int halfX = width / 2;
        int halfY = height / 2;
        float sx = seed * 0.0013f;
        float sy = seed * 0.0017f;
        const float coarseFreq = 0.085f;
        const float fineFreq = 0.215f;

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                float coarse = Mathf.PerlinNoise((x + sx) * coarseFreq, (y + sy) * coarseFreq);
                float fine = Mathf.PerlinNoise((x - sy) * fineFreq, (y + sx) * fineFreq);
                float mixed = coarse * 0.72f + fine * 0.28f;

                if (mixed > 0.62f)
                {
                    map.SetTile(new Vector3Int(x, y, 0), secondary);
                }
            }
        }
    }

    private static void ScatterDetails(Tilemap map, TileBase detail, int width, int height, float density, int seed)
    {
        var rng = new System.Random(seed);
        int halfX = width / 2;
        int halfY = height / 2;
        float sx = seed * 0.0021f;
        float sy = seed * 0.0011f;
        const float noiseFreq = 0.18f;
        float threshold = 1f - Mathf.Clamp01(density) * 1.35f;

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                // Keep center readable for combat and avoid visual clutter at spawn.
                if (Mathf.Abs(x) <= 2 && Mathf.Abs(y) <= 2)
                {
                    continue;
                }

                float noise = Mathf.PerlinNoise((x + sx) * noiseFreq, (y + sy) * noiseFreq);
                bool byNoise = noise > threshold;
                bool byRandom = rng.NextDouble() < density * 0.22f;
                if (byNoise || byRandom)
                {
                    map.SetTile(new Vector3Int(x, y, 0), detail);
                }
            }
        }
    }

    private static void PlaceObstacles(Tilemap obstacles, Tilemap collision, TileBase tile, int width, int height, int count, int seed)
    {
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

            obstacles.SetTile(pos, tile);
            collision.SetTile(pos, tile);
        }

        // Build a mostly closed outer ring with random gaps to create entrances.
        for (int x = -halfX + 2; x < halfX - 2; x++)
        {
            if (rng.NextDouble() > 0.18d)
            {
                PlaceAt(x, -halfY + 2);
            }
            if (rng.NextDouble() > 0.18d)
            {
                PlaceAt(x, halfY - 3);
            }
        }
        for (int y = -halfY + 3; y < halfY - 3; y++)
        {
            if (rng.NextDouble() > 0.18d)
            {
                PlaceAt(-halfX + 2, y);
            }
            if (rng.NextDouble() > 0.18d)
            {
                PlaceAt(halfX - 3, y);
            }
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

    private static void ClearSpawnArea(Tilemap details, Tilemap obstacles, Tilemap collision, int radius)
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
        public Sprite Ground;
        public Sprite Detail;
        public Sprite Obstacle;
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

    private struct TileStat
    {
        public Sprite Sprite;
        public float GreenBias;
        public float Brightness;
        public float Warmth;
        public float OpaqueRatio;
    }
}

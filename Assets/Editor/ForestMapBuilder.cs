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
        FillGround(groundMap, groundTile, size.Width, size.Height);
        ScatterDetails(detailMap, detailTile, size.Width, size.Height, density: 0.04f, seed: 1234);
        PlaceObstacles(obstacleMap, collisionMap, obstacleTile, size.Width, size.Height, count: 14, seed: 4321);

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

        importer.spritesheet = metas.ToArray();
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
        collider.usedByComposite = true;

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

    private static void ScatterDetails(Tilemap map, TileBase detail, int width, int height, float density, int seed)
    {
        var rng = new System.Random(seed);
        int halfX = width / 2;
        int halfY = height / 2;

        for (int y = -halfY; y < halfY; y++)
        {
            for (int x = -halfX; x < halfX; x++)
            {
                if (rng.NextDouble() < density)
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

        var perimeter = new List<Vector3Int>();
        for (int x = -halfX + 2; x < halfX - 2; x++)
        {
            perimeter.Add(new Vector3Int(x, -halfY + 2, 0));
            perimeter.Add(new Vector3Int(x, halfY - 3, 0));
        }
        for (int y = -halfY + 3; y < halfY - 3; y++)
        {
            perimeter.Add(new Vector3Int(-halfX + 2, y, 0));
            perimeter.Add(new Vector3Int(halfX - 3, y, 0));
        }

        count = Mathf.Min(count, perimeter.Count);
        for (int i = 0; i < count; i++)
        {
            int index = rng.Next(perimeter.Count);
            var pos = perimeter[index];
            perimeter.RemoveAt(index);

            obstacles.SetTile(pos, tile);
            collision.SetTile(pos, tile);
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

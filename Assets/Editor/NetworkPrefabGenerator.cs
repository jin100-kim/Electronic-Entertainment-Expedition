using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[InitializeOnLoad]
public static class NetworkPrefabGenerator
{
    private const string ResourcesRoot = "Assets/Resources";
    private const string PrefabFolder = "Assets/Resources/NetcodePrefabs";

    static NetworkPrefabGenerator()
    {
        EditorApplication.delayCall += EnsurePrefabs;
    }

    [MenuItem("Tools/Netcode/Generate Network Prefabs")]
    private static void EnsurePrefabsMenu()
    {
        EnsurePrefabs();
    }

    private static void EnsurePrefabs()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        EnsureFolder(ResourcesRoot);
        EnsureFolder(PrefabFolder);

        CreateIfMissing("Enemy", BuildEnemyPrefab);
        CreateIfMissing("XP", BuildXpPrefab);
        CreateIfMissing("Coin", BuildCoinPrefab);
        CreateIfMissing("Projectile", BuildProjectilePrefab);
        CreateIfMissing("Laser", BuildLaserPrefab);
        CreateIfMissing("Boomerang", BuildBoomerangPrefab);
        CreateIfMissing("Drone", BuildDronePrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path);
        string name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    private static void CreateIfMissing(string name, System.Func<GameObject> builder)
    {
        string path = $"{PrefabFolder}/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            return;
        }

        var go = builder();
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static GameObject BuildEnemyPrefab()
    {
        var go = new GameObject("Enemy");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<EnemyController>();
        go.AddComponent<EnemyVisuals>();
        go.AddComponent<EnemyTier>();
        go.AddComponent<EnemyNetState>();
        return go;
    }

    private static GameObject BuildXpPrefab()
    {
        var go = new GameObject("XP");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        go.AddComponent<ExperiencePickup>();
        return go;
    }

    private static GameObject BuildCoinPrefab()
    {
        var go = new GameObject("Coin");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        go.AddComponent<CoinPickup>();
        return go;
    }

    private static GameObject BuildProjectilePrefab()
    {
        var go = new GameObject("Projectile");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<Projectile>();
        return go;
    }

    private static GameObject BuildLaserPrefab()
    {
        var go = new GameObject("Laser");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        go.AddComponent<Projectile>();
        return go;
    }

    private static GameObject BuildBoomerangPrefab()
    {
        var go = new GameObject("Boomerang");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<BoomerangProjectile>();
        return go;
    }

    private static GameObject BuildDronePrefab()
    {
        var go = new GameObject("Drone");
        go.AddComponent<NetworkObject>();
        go.AddComponent<NetworkTransform>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<NetworkColor>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        go.AddComponent<DroneProjectile>();
        return go;
    }
}

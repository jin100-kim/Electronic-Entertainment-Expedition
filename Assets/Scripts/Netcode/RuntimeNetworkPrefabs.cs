using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public static class RuntimeNetworkPrefabs
{
    private static bool _registered;
    private static GameObject _enemyPrefab;
    private static GameObject _xpPrefab;
    private static GameObject _coinPrefab;
    private static GameObject _projectilePrefab;
    private static GameObject _laserPrefab;
    private static GameObject _boomerangPrefab;
    private static GameObject _dronePrefab;

    private const string EnemyPath = "NetcodePrefabs/Enemy";
    private const string XpPath = "NetcodePrefabs/XP";
    private const string CoinPath = "NetcodePrefabs/Coin";
    private const string ProjectilePath = "NetcodePrefabs/Projectile";
    private const string LaserPath = "NetcodePrefabs/Laser";
    private const string BoomerangPath = "NetcodePrefabs/Boomerang";
    private const string DronePath = "NetcodePrefabs/Drone";

    public static void EnsureRegistered()
    {
        if (_registered)
        {
            return;
        }

        var manager = NetworkManager.Singleton;
        if (manager == null)
        {
            return;
        }

        _enemyPrefab = LoadPrefab(EnemyPath);
        _xpPrefab = LoadPrefab(XpPath);
        _coinPrefab = LoadPrefab(CoinPath);
        _projectilePrefab = LoadPrefab(ProjectilePath);
        _laserPrefab = LoadPrefab(LaserPath);
        _boomerangPrefab = LoadPrefab(BoomerangPath);
        _dronePrefab = LoadPrefab(DronePath);

        RegisterPrefab(manager, _enemyPrefab);
        RegisterPrefab(manager, _xpPrefab);
        RegisterPrefab(manager, _coinPrefab);
        RegisterPrefab(manager, _projectilePrefab);
        RegisterPrefab(manager, _laserPrefab);
        RegisterPrefab(manager, _boomerangPrefab);
        RegisterPrefab(manager, _dronePrefab);

        _registered = true;
    }

    public static GameObject InstantiateEnemy()
    {
        EnsureRegistered();
        return CreateInstance(_enemyPrefab);
    }

    public static GameObject InstantiateXp()
    {
        EnsureRegistered();
        return CreateInstance(_xpPrefab);
    }

    public static GameObject InstantiateCoin()
    {
        EnsureRegistered();
        return CreateInstance(_coinPrefab);
    }

    public static GameObject InstantiateProjectile()
    {
        EnsureRegistered();
        return CreateInstance(_projectilePrefab);
    }

    public static GameObject InstantiateLaser()
    {
        EnsureRegistered();
        return CreateInstance(_laserPrefab);
    }

    public static GameObject InstantiateBoomerang()
    {
        EnsureRegistered();
        return CreateInstance(_boomerangPrefab);
    }

    public static GameObject InstantiateDrone()
    {
        EnsureRegistered();
        return CreateInstance(_dronePrefab);
    }

    private static GameObject CreateInstance(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        var instance = Object.Instantiate(prefab);
        instance.SetActive(true);
        return instance;
    }

    private static void RegisterPrefab(NetworkManager manager, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (manager.NetworkConfig != null && manager.NetworkConfig.Prefabs != null)
        {
            if (manager.NetworkConfig.Prefabs.Contains(prefab))
            {
                return;
            }
        }

        manager.AddNetworkPrefab(prefab);
    }

    private static GameObject LoadPrefab(string path)
    {
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[RuntimeNetworkPrefabs] Missing prefab at Resources/{path}.prefab");
        }
        return prefab;
    }
}

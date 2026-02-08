using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Start Mode")]
    [SerializeField]
    private bool autoStartLocal = false;

    [SerializeField]
    private bool showNetworkUI = true;

    [Header("Spawn")]
    [SerializeField]
    private float spawnInterval = 2f;

    [SerializeField]
    private int maxEnemies = 20;

    [SerializeField]
    private float spawnRadius = 8f;

    [Header("Difficulty")]
    [SerializeField]
    private float minSpawnInterval = 0.4f;

    [SerializeField]
    private float spawnIntervalDecayPerSec = 0.01f;

    [SerializeField]
    private int maxEnemiesPerMinute = 10;

    [SerializeField]
    private float monsterLevelInterval = 60f;

    [SerializeField]
    private float enemyHealthPerLevel = 0.15f;

    [SerializeField]
    private float enemyDamagePerLevel = 0.10f;

    [SerializeField]
    private float enemySpeedPerLevel = 0.05f;

    [SerializeField]
    private float enemyXpPerLevel = 0f;

    [Header("Player")]
    [SerializeField]
    private Vector3 localSpawnPosition = Vector3.zero;

    [Header("Map Bounds")]
    [SerializeField]
    private Vector2 mapHalfSize = new Vector2(12f, 12f);

    [Header("Upgrades")]
    [SerializeField]
    private int maxUpgradeLevel = 10;

    [SerializeField]
    private int maxWeaponSlots = 5;

    [SerializeField]
    private int maxStatSlots = 5;

    [SerializeField]
    private float damageMult = 1f;

    [SerializeField]
    private float fireRateMult = 1f;

    [SerializeField]
    private float rangeMult = 1f;

    [SerializeField]
    private float sizeMult = 1f;

    [SerializeField]
    private float lifetimeMult = 1f;

    [SerializeField]
    private int projectileCount = 1;

    [SerializeField]
    private int projectilePierceBonus = 0;

    [SerializeField]
    private float weaponDamageMult = 1f;

    [SerializeField]
    private WeaponStatsData gunStats = new WeaponStatsData
    {
        displayName = "총",
        level = 1,
        unlocked = true,
        damageMult = 1f,
        fireRateMult = 1.2f,
        rangeMult = 1f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData boomerangStats = new WeaponStatsData
    {
        displayName = "부메랑",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.8f,
        rangeMult = 0.7f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData novaStats = new WeaponStatsData
    {
        displayName = "노바",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.6f,
        rangeMult = 0.5f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData shotgunStats = new WeaponStatsData
    {
        displayName = "샷건",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.7f,
        rangeMult = 0.75f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData laserStats = new WeaponStatsData
    {
        displayName = "레이저",
        level = 0,
        unlocked = false,
        damageMult = 1.1f,
        fireRateMult = 0.8f,
        rangeMult = 1.4f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData chainStats = new WeaponStatsData
    {
        displayName = "체인 라이트닝",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.75f,
        rangeMult = 1.1f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData droneStats = new WeaponStatsData
    {
        displayName = "드론",
        level = 0,
        unlocked = false,
        damageMult = 0.8f,
        fireRateMult = 0.5f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData shurikenStats = new WeaponStatsData
    {
        displayName = "수리검",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.9f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData frostStats = new WeaponStatsData
    {
        displayName = "빙결 구체",
        level = 0,
        unlocked = false,
        damageMult = 0.85f,
        fireRateMult = 0.8f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData lightningStats = new WeaponStatsData
    {
        displayName = "번개",
        level = 0,
        unlocked = false,
        damageMult = 1.0f,
        fireRateMult = 0.7f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private float moveSpeedMult = 1f;

    [SerializeField]
    private float xpGainMult = 1f;

    [SerializeField]
    private float magnetRangeMult = 1f;

    [SerializeField]
    private float magnetSpeedMult = 1f;

    [SerializeField]
    private float magnetRangeStep = 0.5f;

    [SerializeField]
    private float magnetSpeedStep = 0.5f;

    [SerializeField]
    private float regenPerSecond = 0f;

    [Header("Drops")]
    [SerializeField]
    private float coinDropChance = 0.06f;

    [SerializeField]
    private int coinAmount = 1;

    [Header("Upgrade Levels")]
    [SerializeField]
    private int damageLevel = 0;

    [SerializeField]
    private int fireRateLevel = 0;

    [SerializeField]
    private int moveSpeedLevel = 0;

    [SerializeField]
    private int healthReinforceLevel = 0;

    [SerializeField]
    private int rangeLevel = 0;

    [SerializeField]
    private int xpGainLevel = 0;

    [SerializeField]
    private int sizeLevel = 0;

    [SerializeField]
    private int magnetLevel = 0;

    [SerializeField]
    private int pierceLevel = 0;

    [SerializeField]
    private int projectileCountLevel = 0;

    [Header("Start Weapon")]
    [SerializeField]
    private bool requireStartWeaponChoice = true;

    [SerializeField]
    private StartWeapon startWeapon = StartWeapon.Gun;

    [Header("Start Character Preview")]
    [SerializeField]
    private float startPreviewScale = 0.8f;

    [SerializeField]
    private int startPreviewSortingOrder = 5000;

    public Vector2 MapHalfSize => mapHalfSize;
    public int MonsterLevel => Mathf.Max(1, 1 + Mathf.FloorToInt(ElapsedTime / Mathf.Max(1f, monsterLevelInterval)));
    public bool IsWaitingStartWeaponChoice => _waitingStartWeaponChoice;
    public bool IsGameplayActive => _gameStarted && !_waitingStartWeaponChoice;

    private bool StraightUnlocked => gunStats != null && gunStats.unlocked && gunStats.level > 0;

    public bool IsGameOver { get; private set; }
    public float ElapsedTime { get; private set; }
    public Health PlayerHealth { get; private set; }
    public Experience PlayerExperience { get; private set; }
    public int CoinCount => _coinCount;
    public int KillCount => _killCount;

    private EnemySpawner _spawner;
    private float _baseEnemyMoveSpeed;
    private float _baseEnemyDamage;
    private float _baseEnemyMaxHealth;
    private int _baseEnemyXp;
    private bool _cachedSpawnerBase;
    private AutoAttack _attack;
    private PlayerController _player;
    private bool _gameStarted;

    private bool _choosingUpgrade;
    private readonly List<UpgradeOption> _options = new List<UpgradeOption>();
    private readonly Dictionary<string, int> _upgradeCounts = new Dictionary<string, int>();
    private readonly List<string> _upgradeOrder = new List<string>();
    private bool _waitingStartWeaponChoice;
    private bool _autoPlayEnabled;
    private float _autoUpgradeStartTime = -1f;
    private GameObject[] _startPreviews;
    private Camera _cachedCamera;
    private bool _rerollAvailable;

    private const string CoinPrefKey = "CoinCount";
    private int _coinCount;
    private int _killCount;

    private enum StartWeapon
    {
        Gun,
        Boomerang,
        Nova
    }

    private void Awake()
    {
        ApplyRuntimeDefaults();
        Instance = this;
        _coinCount = PlayerPrefs.GetInt(CoinPrefKey, 0);
    }

    private void ApplyRuntimeDefaults()
    {
        autoStartLocal = false;
        showNetworkUI = true;

        spawnInterval = 2f;
        maxEnemies = 20;
        spawnRadius = 8f;

        minSpawnInterval = 0.4f;
        spawnIntervalDecayPerSec = 0.01f;
        maxEnemiesPerMinute = 10;
        monsterLevelInterval = 60f;
        enemyHealthPerLevel = 0.15f;
        enemyDamagePerLevel = 0.10f;
        enemySpeedPerLevel = 0.05f;
        enemyXpPerLevel = 0f;

        localSpawnPosition = Vector3.zero;
        mapHalfSize = new Vector2(12f, 12f);

        maxUpgradeLevel = 10;
        maxWeaponSlots = 5;
        maxStatSlots = 5;

        damageMult = 1f;
        fireRateMult = 1f;
        rangeMult = 1f;
        sizeMult = 1f;
        lifetimeMult = 1f;
        projectileCount = 1;
        projectilePierceBonus = 0;
        weaponDamageMult = 1f;

        ApplyWeaponDefaults();

        moveSpeedMult = 1f;
        xpGainMult = 1f;
        magnetRangeMult = 1f;
        magnetSpeedMult = 1f;
        magnetRangeStep = 1f;
        magnetSpeedStep = 1f;
        regenPerSecond = 0f;

        coinDropChance = 0.06f;
        coinAmount = 1;

        damageLevel = 0;
        fireRateLevel = 0;
        moveSpeedLevel = 0;
        healthReinforceLevel = 0;
        rangeLevel = 0;
        xpGainLevel = 0;
        sizeLevel = 0;
        magnetLevel = 0;
        pierceLevel = 0;
        projectileCountLevel = 0;

        requireStartWeaponChoice = true;
        startWeapon = StartWeapon.Gun;

        startPreviewScale = 0.8f;
        startPreviewSortingOrder = 5000;
    }

    private void ApplyWeaponDefaults()
    {
        ApplyWeaponDefaults(gunStats, "총", 1, true, 1f, 1.2f, 1f, 0);
        ApplyWeaponDefaults(boomerangStats, "부메랑", 0, false, 1f, 0.8f, 0.7f, 0);
        ApplyWeaponDefaults(novaStats, "노바", 0, false, 1f, 0.6f, 0.5f, 0);
        ApplyWeaponDefaults(shotgunStats, "샷건", 0, false, 0.9f, 0.7f, 0.75f, 0);
        ApplyWeaponDefaults(laserStats, "레이저", 0, false, 1.1f, 0.8f, 1.4f, 0);
        ApplyWeaponDefaults(chainStats, "체인 라이트닝", 0, false, 0.9f, 0.75f, 1.1f, 0);
        ApplyWeaponDefaults(droneStats, "드론", 0, false, 0.8f, 0.5f, 1.0f, 0);
        ApplyWeaponDefaults(shurikenStats, "수리검", 0, false, 0.9f, 0.9f, 1.0f, 0);
        ApplyWeaponDefaults(frostStats, "빙결 구체", 0, false, 0.85f, 0.8f, 1.0f, 0);
        ApplyWeaponDefaults(lightningStats, "번개", 0, false, 1.0f, 0.7f, 1.0f, 0);
    }

    private static void ApplyWeaponDefaults(WeaponStatsData stats, string displayName, int level, bool unlocked, float damage, float rate, float range, int bonusProjectiles)
    {
        if (stats == null)
        {
            return;
        }

        stats.displayName = displayName;
        stats.level = level;
        stats.unlocked = unlocked;
        stats.damageMult = damage;
        stats.fireRateMult = rate;
        stats.rangeMult = range;
        stats.bonusProjectiles = bonusProjectiles;
    }

    private void Start()
    {
        if (!showNetworkUI)
        {
            DisableNetworkUI();
        }

        if (gunStats != null)
        {
            if (requireStartWeaponChoice)
            {
                gunStats.unlocked = false;
                gunStats.level = 0;
            }
            else
            {
                gunStats.unlocked = true;
                gunStats.level = Mathf.Max(1, gunStats.level);
            }
        }

        if (requireStartWeaponChoice)
        {
            ResetWeaponToLocked(shotgunStats);
            ResetWeaponToLocked(laserStats);
            ResetWeaponToLocked(chainStats);
            ResetWeaponToLocked(droneStats);
            ResetWeaponToLocked(shurikenStats);
            ResetWeaponToLocked(frostStats);
            ResetWeaponToLocked(lightningStats);
            ResetWeaponToLocked(boomerangStats);
            ResetWeaponToLocked(novaStats);
        }

        if (autoStartLocal)
        {
            if (requireStartWeaponChoice)
            {
                _waitingStartWeaponChoice = true;
            }
            else
            {
                StartLocalGame();
            }
        }

        EnsureCameraFollow();
        EnsureMinimap();
        EnsureMapBorder();
    }

    private void Update()
    {
        if (!_gameStarted || IsGameOver)
        {
            return;
        }

        if (_choosingUpgrade && _autoPlayEnabled)
        {
            if (_autoUpgradeStartTime < 0f)
            {
                _autoUpgradeStartTime = Time.unscaledTime;
            }

            if (Time.unscaledTime - _autoUpgradeStartTime >= 1f)
            {
                int index = PickAutoUpgradeIndex();
                if (index >= 0)
                {
                    ApplyUpgrade(index);
                }
            }
        }
        else
        {
            _autoUpgradeStartTime = -1f;
        }

        if (!_choosingUpgrade)
        {
            ElapsedTime += Time.deltaTime;
            ApplyDifficultyScaling();
        }
    }

    public static void StartNetworkGame()
    {
        if (Instance == null || Instance._gameStarted)
        {
            return;
        }

        Instance.StartNetworkSession();
    }

    private void StartNetworkSession()
    {
        bool isNetworked = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        if (!isNetworked && requireStartWeaponChoice && !_waitingStartWeaponChoice)
        {
            _waitingStartWeaponChoice = true;
            _gameStarted = false;
            return;
        }

        _gameStarted = true;

        if (isNetworked)
        {
            StartCoroutine(WaitForOwnerPlayer());
        }
        else
        {
            StartLocalGame();
        }
    }

    private IEnumerator WaitForOwnerPlayer()
    {
        PlayerController ownerPlayer = null;
        while (ownerPlayer == null)
        {
            var players = FindObjectsOfType<PlayerController>();
            foreach (var p in players)
            {
                if (p != null && p.IsOwner)
                {
                    ownerPlayer = p;
                    break;
                }
            }

            yield return null;
        }

        SetupPlayer(ownerPlayer);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SetupSpawner(ownerPlayer.transform);
        }
    }

    private void StartLocalGame()
    {
        _gameStarted = true;
        var player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            player = CreateLocalPlayer(localSpawnPosition);
        }

        SetupPlayer(player);
        SetupSpawner(player.transform);
    }

    private void SetupPlayer(PlayerController player)
    {
        _player = player;
        _player.SetMoveSpeedMultiplier(moveSpeedMult);
        _player.SetAutoPlay(_autoPlayEnabled);
        ApplyPlayerVisuals(startWeapon);

        PlayerHealth = player.GetComponent<Health>();
        if (PlayerHealth != null)
        {
            PlayerHealth.ResetHealth();
            PlayerHealth.OnDied += OnPlayerDied;
        }

        PlayerExperience = player.GetComponent<Experience>();
        if (PlayerExperience == null)
        {
            PlayerExperience = player.gameObject.AddComponent<Experience>();
        }

        PlayerExperience.SetXpMultiplier(xpGainMult);
        PlayerExperience.SetMagnetMultiplier(magnetRangeMult, magnetSpeedMult);
        PlayerExperience.OnLevelUp += OnLevelUp;

        _attack = player.GetComponent<AutoAttack>();
        if (_attack == null)
        {
            _attack = player.gameObject.AddComponent<AutoAttack>();
        }

        ApplyAttackStats();
        PlayerHealth?.SetRegenPerSecond(regenPerSecond);
    }

    private void ApplyPlayerVisuals(StartWeapon weapon)
    {
        if (_player == null)
        {
            return;
        }

        var visuals = _player.GetComponent<PlayerVisuals>();
        if (visuals == null)
        {
            visuals = _player.gameObject.AddComponent<PlayerVisuals>();
        }

        switch (weapon)
        {
            case StartWeapon.Boomerang:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.Warrior);
                break;
            case StartWeapon.Nova:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.DemonLord);
                break;
            default:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.Mage);
                break;
        }
    }

    private void SetupSpawner(Transform target)
    {
        _spawner = GetComponent<EnemySpawner>();
        if (_spawner == null)
        {
            _spawner = gameObject.AddComponent<EnemySpawner>();
        }

        _spawner.Target = target;
        _spawner.SpawnInterval = spawnInterval;
        _spawner.MaxEnemies = maxEnemies;
        _spawner.SpawnRadius = spawnRadius;

        CacheSpawnerBaseStats();
        ApplyDifficultyScaling();
    }

    private PlayerController CreateLocalPlayer(Vector3 position)
    {
        var go = new GameObject("LocalPlayer");
        go.transform.position = position;

        if (go.GetComponent<NetworkObject>() == null)
        {
            go.AddComponent<NetworkObject>();
        }

        if (go.GetComponent<NetworkTransform>() == null)
        {
            go.AddComponent<NetworkTransform>();
        }

        var controller = go.AddComponent<PlayerController>();
        return controller;
    }

    private void OnPlayerDied()
    {
        IsGameOver = true;
        if (_spawner != null)
        {
            _spawner.enabled = false;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        ShowUpgradeChoices();
    }

    private void ShowUpgradeChoices()
    {
        if (_choosingUpgrade)
        {
            return;
        }

        BuildUpgradeOptions(true);

        _choosingUpgrade = true;
        Time.timeScale = 0f;
        _autoUpgradeStartTime = _autoPlayEnabled ? Time.unscaledTime : -1f;
    }

    private void BuildUpgradeOptions(bool resetReroll)
    {
        _options.Clear();

        if (damageLevel < maxUpgradeLevel && CanOfferNewStat(damageLevel))
        {
            _options.Add(new UpgradeOption("공격력 +10%", () => BuildPercentStatText("공격력", damageMult, damageMult + 0.10f), () => { damageMult += 0.10f; damageLevel += 1; }));
        }
        if (fireRateLevel < maxUpgradeLevel && CanOfferNewStat(fireRateLevel))
        {
            _options.Add(new UpgradeOption("공격속도 +10%", () => BuildPercentStatText("공격속도", fireRateMult, fireRateMult + 0.10f), () => { fireRateMult += 0.10f; fireRateLevel += 1; }));
        }
        if (moveSpeedLevel < maxUpgradeLevel && CanOfferNewStat(moveSpeedLevel))
        {
            _options.Add(new UpgradeOption("이동속도 +10%", () => BuildPercentStatText("이동속도", moveSpeedMult, moveSpeedMult + 0.10f), () => { moveSpeedMult += 0.10f; moveSpeedLevel += 1; }));
        }
        if (healthReinforceLevel < maxUpgradeLevel && CanOfferNewStat(healthReinforceLevel))
        {
            _options.Add(new UpgradeOption("체력 강화", BuildHealthReinforceText, () =>
            {
                if (PlayerHealth != null)
                {
                    PlayerHealth.AddMaxHealth(25f, true);
                    PlayerHealth.Heal(PlayerHealth.MaxHealth);
                }
                regenPerSecond += 0.5f;
                healthReinforceLevel += 1;
            }));
        }
        if (rangeLevel < maxUpgradeLevel && CanOfferNewStat(rangeLevel))
        {
            _options.Add(new UpgradeOption("사거리 +15%", () => BuildPercentStatText("사거리", rangeMult, rangeMult + 0.15f), () => { rangeMult += 0.15f; rangeLevel += 1; }));
        }
        if (xpGainLevel < maxUpgradeLevel && CanOfferNewStat(xpGainLevel))
        {
            _options.Add(new UpgradeOption("경험치 +10%", () => BuildPercentStatText("경험치 획득", xpGainMult, xpGainMult + 0.10f), () => { xpGainMult += 0.10f; xpGainLevel += 1; }));
        }
        if (magnetLevel < maxUpgradeLevel && CanOfferNewStat(magnetLevel))
        {
            _options.Add(new UpgradeOption("경험치 자석", BuildMagnetUpgradeText, () =>
            {
                magnetRangeMult += magnetRangeStep;
                magnetSpeedMult += magnetSpeedStep;
                magnetLevel += 1;
            }));
        }
        if (sizeLevel < maxUpgradeLevel && CanOfferNewStat(sizeLevel))
        {
            _options.Add(new UpgradeOption("투사체 크기 +25%", () => BuildPercentStatText("투사체 크기", sizeMult, sizeMult + 0.25f), () => { sizeMult += 0.25f; sizeLevel += 1; }));
        }
        if (projectileCountLevel < maxUpgradeLevel && CanOfferNewStat(projectileCountLevel))
        {
            _options.Add(new UpgradeOption("투사체 수", BuildProjectileCountText, () =>
            {
                projectileCountLevel += 1;
                if (projectileCountLevel % 2 == 0)
                {
                    projectileCount += 1;
                }
            }));
        }
        if (pierceLevel < maxUpgradeLevel && CanOfferNewStat(pierceLevel))
        {
            _options.Add(new UpgradeOption("관통 +1", () => BuildValueStatText("관통", projectilePierceBonus, projectilePierceBonus + 1), () => { projectilePierceBonus += 1; pierceLevel += 1; }));
        }
        AddWeaponChoice(gunStats, BuildStraightUpgradeText, UnlockStraight, LevelUpStraightWeapon);
        AddWeaponChoice(boomerangStats, BuildBoomerangUpgradeText, UnlockBoomerang, LevelUpBoomerangWeapon);
        AddWeaponChoice(novaStats, BuildNovaUpgradeText, UnlockNova, LevelUpNovaWeapon);
        AddWeaponChoice(shotgunStats, BuildShotgunUpgradeText, UnlockShotgun, LevelUpShotgunWeapon);
        AddWeaponChoice(laserStats, BuildLaserUpgradeText, UnlockLaser, LevelUpLaserWeapon);
        AddWeaponChoice(chainStats, BuildChainUpgradeText, UnlockChain, LevelUpChainWeapon);
        AddWeaponChoice(droneStats, BuildDroneUpgradeText, UnlockDrone, LevelUpDroneWeapon);
        AddWeaponChoice(shurikenStats, BuildShurikenUpgradeText, UnlockShuriken, LevelUpShurikenWeapon);
        AddWeaponChoice(frostStats, BuildFrostUpgradeText, UnlockFrost, LevelUpFrostWeapon);
        AddWeaponChoice(lightningStats, BuildLightningUpgradeText, UnlockLightning, LevelUpLightningWeapon);

        if (_options.Count == 0)
        {
            _options.Add(new UpgradeOption("HP 회복 (100%)", () => "현재 체력을 모두 회복합니다.", () =>
            {
                if (PlayerHealth != null)
                {
                    PlayerHealth.Heal(PlayerHealth.MaxHealth);
                }
            }));
        }

        for (int i = _options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = _options[i];
            _options[i] = _options[j];
            _options[j] = temp;
        }

        if (_options.Count > 4)
        {
            _options.RemoveRange(4, _options.Count - 4);
        }

        if (resetReroll)
        {
            _rerollAvailable = true;
        }
    }

    private void ApplyUpgrade(int index)
    {
        if (!_choosingUpgrade || index < 0 || index >= _options.Count)
        {
            return;
        }

        var opt = _options[index];
        opt.Apply?.Invoke();
        TrackUpgrade(opt.Title);

        _choosingUpgrade = false;
        Time.timeScale = 1f;
        _autoUpgradeStartTime = -1f;

        // apply updated stats
        _player?.SetMoveSpeedMultiplier(moveSpeedMult);
        PlayerExperience?.SetXpMultiplier(xpGainMult);
        PlayerExperience?.SetMagnetMultiplier(magnetRangeMult, magnetSpeedMult);
        PlayerHealth?.SetRegenPerSecond(regenPerSecond);
        ApplyAttackStats();
    }

    public struct UpgradeIconData
    {
        public string Key;
        public int Level;
        public bool IsWeapon;

        public UpgradeIconData(string key, int level, bool isWeapon)
        {
            Key = key;
            Level = level;
            IsWeapon = isWeapon;
        }
    }

    public void GetUpgradeIconData(List<UpgradeIconData> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        AddWeaponIcon(results, gunStats);
        AddWeaponIcon(results, boomerangStats);
        AddWeaponIcon(results, novaStats);
        AddWeaponIcon(results, shotgunStats);
        AddWeaponIcon(results, laserStats);
        AddWeaponIcon(results, chainStats);
        AddWeaponIcon(results, droneStats);
        AddWeaponIcon(results, shurikenStats);
        AddWeaponIcon(results, frostStats);
        AddWeaponIcon(results, lightningStats);

        AddStatIcon(results, "공격력", damageLevel);
        AddStatIcon(results, "공격속도", fireRateLevel);
        AddStatIcon(results, "이동속도", moveSpeedLevel);
        AddStatIcon(results, "체력강화", healthReinforceLevel);
        AddStatIcon(results, "사거리", rangeLevel);
        AddStatIcon(results, "경험치", xpGainLevel);
        AddStatIcon(results, "자석", magnetLevel);
        AddStatIcon(results, "투사체크기", sizeLevel);
        AddStatIcon(results, "투사체수", projectileCountLevel);
        AddStatIcon(results, "관통", pierceLevel);
    }

    private static void AddWeaponIcon(List<UpgradeIconData> results, WeaponStatsData stats)
    {
        if (stats == null || !stats.unlocked || stats.level <= 0)
        {
            return;
        }

        results.Add(new UpgradeIconData(stats.displayName, stats.level, true));
    }

    private static void AddStatIcon(List<UpgradeIconData> results, string key, int level)
    {
        if (level <= 0)
        {
            return;
        }

        results.Add(new UpgradeIconData(key, level, false));
    }

    public void RegisterKill(Vector3 position)
    {
        _killCount += 1;
        TrySpawnCoin(position);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _coinCount += amount;
        PlayerPrefs.SetInt(CoinPrefKey, _coinCount);
        PlayerPrefs.Save();
    }

    private void TrackUpgrade(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return;
        }

        if (!_upgradeCounts.ContainsKey(title))
        {
            _upgradeCounts[title] = 0;
            _upgradeOrder.Add(title);
        }

        _upgradeCounts[title] += 1;
    }

    private void TrySpawnCoin(Vector3 position)
    {
        if (coinDropChance <= 0f)
        {
            return;
        }

        if (Random.value > coinDropChance)
        {
            return;
        }

        SpawnCoin(position);
    }

    private void SpawnCoin(Vector3 position)
    {
        var go = new GameObject("Coin");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(40);
        renderer.color = new Color(1f, 0.85f, 0.2f, 1f);
        renderer.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.12f;

        var pickup = go.AddComponent<CoinPickup>();
        pickup.SetAmount(coinAmount);
    }

    private int PickAutoUpgradeIndex()
    {
        if (_options == null || _options.Count == 0)
        {
            return -1;
        }

        int bestScore = int.MinValue;
        var bestIndices = new List<int>();
        for (int i = 0; i < _options.Count; i++)
        {
            int score = ScoreUpgradeOption(_options[i]);
            if (score > bestScore)
            {
                bestScore = score;
                bestIndices.Clear();
                bestIndices.Add(i);
            }
            else if (score == bestScore)
            {
                bestIndices.Add(i);
            }
        }

        if (bestIndices.Count == 0)
        {
            return -1;
        }

        int pick = Random.Range(0, bestIndices.Count);
        return bestIndices[pick];
    }

    private int ScoreUpgradeOption(UpgradeOption option)
    {
        if (option == null || string.IsNullOrEmpty(option.Title))
        {
            return 0;
        }

        string title = option.Title;
        if (title.Contains("무기:"))
        {
            return 1;
        }

        return 0;
    }

    private void AddWeaponChoice(WeaponStatsData stats, System.Func<string> upgradeText, System.Action unlockAction, System.Action levelUpAction)
    {
        if (stats == null)
        {
            return;
        }
        if (stats.level >= maxUpgradeLevel)
        {
            return;
        }
        if (!stats.unlocked && !CanOfferNewWeapon(stats))
        {
            return;
        }

        _options.Add(new UpgradeOption(
            $"무기: {stats.displayName}",
            () => stats.unlocked && stats.level > 0 ? upgradeText() : BuildWeaponAcquireText(stats),
            () =>
            {
                if (stats.unlocked && stats.level > 0)
                {
                    levelUpAction?.Invoke();
                }
                else
                {
                    unlockAction?.Invoke();
                }
            }));
    }

    private bool CanOfferNewWeapon(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return false;
        }
        if (stats.unlocked && stats.level > 0)
        {
            return true;
        }
        if (maxWeaponSlots <= 0)
        {
            return true;
        }

        return GetUnlockedWeaponCount() < maxWeaponSlots;
    }

    private bool CanOfferNewStat(int currentLevel)
    {
        if (currentLevel > 0)
        {
            return true;
        }
        if (maxStatSlots <= 0)
        {
            return true;
        }

        return GetUnlockedStatCount() < maxStatSlots;
    }

    private int GetUnlockedWeaponCount()
    {
        int count = 0;
        if (gunStats != null && gunStats.unlocked && gunStats.level > 0) count++;
        if (boomerangStats != null && boomerangStats.unlocked && boomerangStats.level > 0) count++;
        if (novaStats != null && novaStats.unlocked && novaStats.level > 0) count++;
        if (shotgunStats != null && shotgunStats.unlocked && shotgunStats.level > 0) count++;
        if (laserStats != null && laserStats.unlocked && laserStats.level > 0) count++;
        if (chainStats != null && chainStats.unlocked && chainStats.level > 0) count++;
        if (droneStats != null && droneStats.unlocked && droneStats.level > 0) count++;
        if (shurikenStats != null && shurikenStats.unlocked && shurikenStats.level > 0) count++;
        if (frostStats != null && frostStats.unlocked && frostStats.level > 0) count++;
        if (lightningStats != null && lightningStats.unlocked && lightningStats.level > 0) count++;
        return count;
    }

    private int GetUnlockedStatCount()
    {
        int count = 0;
        if (damageLevel > 0) count++;
        if (fireRateLevel > 0) count++;
        if (moveSpeedLevel > 0) count++;
        if (healthReinforceLevel > 0) count++;
        if (rangeLevel > 0) count++;
        if (xpGainLevel > 0) count++;
        if (sizeLevel > 0) count++;
        if (magnetLevel > 0) count++;
        if (projectileCountLevel > 0) count++;
        if (pierceLevel > 0) count++;
        return count;
    }

    private void ApplyAttackStats()
    {
        if (_attack == null)
        {
            return;
        }

        _attack.ApplyStats(damageMult, fireRateMult, rangeMult, sizeMult, lifetimeMult, projectileCount, projectilePierceBonus, weaponDamageMult);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Straight, gunStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Boomerang, boomerangStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Nova, novaStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Shotgun, shotgunStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Laser, laserStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.ChainLightning, chainStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Drone, droneStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Shuriken, shurikenStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.FrostOrb, frostStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Lightning, lightningStats);
    }

    private void ApplyDifficultyScaling()
    {
        if (_spawner == null)
        {
            return;
        }

        CacheSpawnerBaseStats();

        float newInterval = Mathf.Max(minSpawnInterval, spawnInterval - ElapsedTime * spawnIntervalDecayPerSec);
        _spawner.SpawnInterval = newInterval;

        int extra = Mathf.FloorToInt((ElapsedTime / 60f) * maxEnemiesPerMinute);
        _spawner.MaxEnemies = maxEnemies + extra;

        int level = MonsterLevel;
        float levelFactor = Mathf.Max(0f, level - 1f);
        _spawner.EnemyMoveSpeed = _baseEnemyMoveSpeed * (1f + enemySpeedPerLevel * levelFactor);
        _spawner.EnemyDamage = _baseEnemyDamage * (1f + enemyDamagePerLevel * levelFactor);
        _spawner.EnemyMaxHealth = _baseEnemyMaxHealth * (1f + enemyHealthPerLevel * levelFactor);
        _spawner.EnemyXpReward = Mathf.Max(1, Mathf.RoundToInt(_baseEnemyXp * (1f + enemyXpPerLevel * levelFactor)));
    }

    private void CacheSpawnerBaseStats()
    {
        if (_spawner == null || _cachedSpawnerBase)
        {
            return;
        }

        _baseEnemyMoveSpeed = _spawner.EnemyMoveSpeed;
        _baseEnemyDamage = _spawner.EnemyDamage;
        _baseEnemyMaxHealth = _spawner.EnemyMaxHealth;
        _baseEnemyXp = _spawner.EnemyXpReward;
        _cachedSpawnerBase = true;
    }

    private void DisableNetworkUI()
    {
        var ui = FindObjectOfType<NetworkStartUI>();
        if (ui != null)
        {
            ui.gameObject.SetActive(false);
        }

        var canvas = GameObject.Find("NetworkStartCanvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private void EnsureCameraFollow()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        if (cam.GetComponent<CameraFollow>() == null)
        {
            cam.gameObject.AddComponent<CameraFollow>();
        }
    }

    private void EnsureMinimap()
    {
        if (GetComponent<Minimap>() == null)
        {
            gameObject.AddComponent<Minimap>();
        }
    }

    private void EnsureMapBorder()
    {
        var border = GetComponent<MapBorder>();
        if (border == null)
        {
            border = gameObject.AddComponent<MapBorder>();
        }

        border.SetBounds(mapHalfSize);
    }

    public Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, -mapHalfSize.x, mapHalfSize.x),
            Mathf.Clamp(position.y, -mapHalfSize.y, mapHalfSize.y),
            position.z);
    }

    private void LevelUpStraightWeapon()
    {
        if (gunStats == null)
        {
            return;
        }
        if (gunStats.level >= maxUpgradeLevel)
        {
            return;
        }

        gunStats.level += 1;
        gunStats.damageMult += 0.20f;
        gunStats.fireRateMult += 0.08f;

        if (gunStats.level % 3 == 0)
        {
            gunStats.bonusProjectiles += 1;
        }

        if (gunStats.level % 4 == 0)
        {
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpBoomerangWeapon()
    {
        if (boomerangStats == null)
        {
            return;
        }
        if (boomerangStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!boomerangStats.unlocked)
        {
            UnlockBoomerang();
        }

        boomerangStats.level += 1;
        boomerangStats.damageMult += 0.20f;
        boomerangStats.fireRateMult += 0.10f;

        if (boomerangStats.level % 4 == 0)
        {
            boomerangStats.bonusProjectiles += 1;
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpNovaWeapon()
    {
        if (novaStats == null)
        {
            return;
        }
        if (novaStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!novaStats.unlocked)
        {
            UnlockNova();
        }

        novaStats.level += 1;
        novaStats.damageMult += 0.20f;
        novaStats.fireRateMult += 0.12f;

        if (novaStats.level % 3 == 0)
        {
            novaStats.bonusProjectiles += 2;
        }
    }

    private void LevelUpShotgunWeapon()
    {
        if (shotgunStats == null)
        {
            return;
        }
        if (shotgunStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!shotgunStats.unlocked)
        {
            UnlockShotgun();
        }

        shotgunStats.level += 1;
        shotgunStats.damageMult += 0.18f;
        shotgunStats.fireRateMult += 0.05f;

        if (shotgunStats.level % 2 == 0)
        {
            shotgunStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpLaserWeapon()
    {
        if (laserStats == null)
        {
            return;
        }
        if (laserStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!laserStats.unlocked)
        {
            UnlockLaser();
        }

        laserStats.level += 1;
        laserStats.damageMult += 0.20f;
        laserStats.fireRateMult += 0.04f;

        if (laserStats.level % 3 == 0)
        {
            laserStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpChainWeapon()
    {
        if (chainStats == null)
        {
            return;
        }
        if (chainStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!chainStats.unlocked)
        {
            UnlockChain();
        }

        chainStats.level += 1;
        chainStats.damageMult += 0.18f;
        chainStats.fireRateMult += 0.05f;

        if (chainStats.level % 2 == 0)
        {
            chainStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpDroneWeapon()
    {
        if (droneStats == null)
        {
            return;
        }
        if (droneStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!droneStats.unlocked)
        {
            UnlockDrone();
        }

        droneStats.level += 1;
        droneStats.damageMult += 0.15f;
        droneStats.fireRateMult += 0.04f;

        if (droneStats.level % 3 == 0)
        {
            droneStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpShurikenWeapon()
    {
        if (shurikenStats == null)
        {
            return;
        }
        if (shurikenStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!shurikenStats.unlocked)
        {
            UnlockShuriken();
        }

        shurikenStats.level += 1;
        shurikenStats.damageMult += 0.18f;
        shurikenStats.fireRateMult += 0.06f;

        if (shurikenStats.level % 3 == 0)
        {
            shurikenStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpFrostWeapon()
    {
        if (frostStats == null)
        {
            return;
        }
        if (frostStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!frostStats.unlocked)
        {
            UnlockFrost();
        }

        frostStats.level += 1;
        frostStats.damageMult += 0.16f;
        frostStats.fireRateMult += 0.05f;

        if (frostStats.level % 3 == 0)
        {
            frostStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpLightningWeapon()
    {
        if (lightningStats == null)
        {
            return;
        }
        if (lightningStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!lightningStats.unlocked)
        {
            UnlockLightning();
        }

        lightningStats.level += 1;
        lightningStats.damageMult += 0.20f;
        lightningStats.fireRateMult += 0.06f;

        if (lightningStats.level % 2 == 0)
        {
            lightningStats.bonusProjectiles += 1;
        }
    }

    private void UnlockBoomerang()
    {
        if (boomerangStats == null)
        {
            return;
        }

        boomerangStats.unlocked = true;
        if (boomerangStats.level < 1)
        {
            boomerangStats.level = 1;
        }
    }

    private void UnlockNova()
    {
        if (novaStats == null)
        {
            return;
        }

        novaStats.unlocked = true;
        if (novaStats.level < 1)
        {
            novaStats.level = 1;
        }
    }

    private void UnlockShotgun()
    {
        if (shotgunStats == null)
        {
            return;
        }

        shotgunStats.unlocked = true;
        if (shotgunStats.level < 1)
        {
            shotgunStats.level = 1;
        }
    }

    private void UnlockLaser()
    {
        if (laserStats == null)
        {
            return;
        }

        laserStats.unlocked = true;
        if (laserStats.level < 1)
        {
            laserStats.level = 1;
        }
    }

    private void UnlockChain()
    {
        if (chainStats == null)
        {
            return;
        }

        chainStats.unlocked = true;
        if (chainStats.level < 1)
        {
            chainStats.level = 1;
        }
    }

    private void UnlockDrone()
    {
        if (droneStats == null)
        {
            return;
        }

        droneStats.unlocked = true;
        if (droneStats.level < 1)
        {
            droneStats.level = 1;
        }
    }

    private void UnlockShuriken()
    {
        if (shurikenStats == null)
        {
            return;
        }

        shurikenStats.unlocked = true;
        if (shurikenStats.level < 1)
        {
            shurikenStats.level = 1;
        }
    }

    private void UnlockFrost()
    {
        if (frostStats == null)
        {
            return;
        }

        frostStats.unlocked = true;
        if (frostStats.level < 1)
        {
            frostStats.level = 1;
        }
    }

    private void UnlockLightning()
    {
        if (lightningStats == null)
        {
            return;
        }

        lightningStats.unlocked = true;
        if (lightningStats.level < 1)
        {
            lightningStats.level = 1;
        }
    }

    private void UnlockStraight()
    {
        if (gunStats == null)
        {
            return;
        }

        gunStats.unlocked = true;
        if (gunStats.level < 1)
        {
            gunStats.level = 1;
        }
    }

    private string BuildWeaponAcquireText(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        int currentLevel = stats.level;
        int nextLevel = Mathf.Max(1, currentLevel + 1);
        float dmg = stats.damageMult;
        float rate = stats.fireRateMult;
        int baseProjectiles = GetBaseProjectileCount(stats);
        return $"{stats.displayName}\n레벨 {currentLevel} -> {nextLevel}\n피해량 {dmg:0.##} -> {dmg:0.##}\n속도 {rate:0.##} -> {rate:0.##}\n투사체 {baseProjectiles} -> {baseProjectiles}\n관통 0 -> 0";
    }

    private string BuildStraightUpgradeText()
    {
        if (gunStats == null)
        {
            return string.Empty;
        }

        int nextLevel = gunStats.level + 1;
        float nextDamage = gunStats.damageMult + 0.20f;
        int currentProjectile = 1 + gunStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = gunStats.fireRateMult + 0.08f;
        return BuildWeaponUpgradeText(gunStats.displayName, gunStats.level, nextLevel, gunStats.damageMult, nextDamage, gunStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, nextPierce);
    }

    private string BuildBoomerangUpgradeText()
    {
        if (boomerangStats == null)
        {
            return string.Empty;
        }

        int nextLevel = boomerangStats.level + 1;
        float nextDamage = boomerangStats.damageMult + 0.20f;
        int currentProjectile = 1 + boomerangStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 4 == 0 ? 1 : 0);
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = boomerangStats.fireRateMult + 0.10f;
        return BuildWeaponUpgradeText(boomerangStats.displayName, boomerangStats.level, nextLevel, boomerangStats.damageMult, nextDamage, boomerangStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, nextPierce);
    }

    private string BuildNovaUpgradeText()
    {
        if (novaStats == null)
        {
            return string.Empty;
        }

        int nextLevel = novaStats.level + 1;
        float nextDamage = novaStats.damageMult + 0.20f;
        int currentCount = 8 + novaStats.bonusProjectiles;
        int nextCount = currentCount + (nextLevel % 3 == 0 ? 2 : 0);
        float nextRate = novaStats.fireRateMult + 0.12f;
        return BuildWeaponUpgradeText(novaStats.displayName, novaStats.level, nextLevel, novaStats.damageMult, nextDamage, novaStats.fireRateMult, nextRate, currentCount, nextCount, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildShotgunUpgradeText()
    {
        if (shotgunStats == null)
        {
            return string.Empty;
        }

        int nextLevel = shotgunStats.level + 1;
        float nextDamage = shotgunStats.damageMult + 0.18f;
        int currentProjectile = 5 + shotgunStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = shotgunStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(shotgunStats.displayName, shotgunStats.level, nextLevel, shotgunStats.damageMult, nextDamage, shotgunStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildLaserUpgradeText()
    {
        if (laserStats == null)
        {
            return string.Empty;
        }

        int nextLevel = laserStats.level + 1;
        float nextDamage = laserStats.damageMult + 0.20f;
        int currentProjectile = 1 + laserStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = laserStats.fireRateMult + 0.04f;
        return BuildWeaponUpgradeText(laserStats.displayName, laserStats.level, nextLevel, laserStats.damageMult, nextDamage, laserStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildChainUpgradeText()
    {
        if (chainStats == null)
        {
            return string.Empty;
        }

        int nextLevel = chainStats.level + 1;
        float nextDamage = chainStats.damageMult + 0.18f;
        int currentProjectile = 3 + chainStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = chainStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(chainStats.displayName, chainStats.level, nextLevel, chainStats.damageMult, nextDamage, chainStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildDroneUpgradeText()
    {
        if (droneStats == null)
        {
            return string.Empty;
        }

        int nextLevel = droneStats.level + 1;
        float nextDamage = droneStats.damageMult + 0.15f;
        int currentProjectile = 1 + droneStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = droneStats.fireRateMult + 0.04f;
        return BuildWeaponUpgradeText(droneStats.displayName, droneStats.level, nextLevel, droneStats.damageMult, nextDamage, droneStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildShurikenUpgradeText()
    {
        if (shurikenStats == null)
        {
            return string.Empty;
        }

        int nextLevel = shurikenStats.level + 1;
        float nextDamage = shurikenStats.damageMult + 0.18f;
        int currentProjectile = 1 + shurikenStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = shurikenStats.fireRateMult + 0.06f;
        return BuildWeaponUpgradeText(shurikenStats.displayName, shurikenStats.level, nextLevel, shurikenStats.damageMult, nextDamage, shurikenStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildFrostUpgradeText()
    {
        if (frostStats == null)
        {
            return string.Empty;
        }

        int nextLevel = frostStats.level + 1;
        float nextDamage = frostStats.damageMult + 0.16f;
        int currentProjectile = 1 + frostStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = frostStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(frostStats.displayName, frostStats.level, nextLevel, frostStats.damageMult, nextDamage, frostStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildLightningUpgradeText()
    {
        if (lightningStats == null)
        {
            return string.Empty;
        }

        int nextLevel = lightningStats.level + 1;
        float nextDamage = lightningStats.damageMult + 0.20f;
        int currentProjectile = 1 + lightningStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = lightningStats.fireRateMult + 0.06f;
        return BuildWeaponUpgradeText(lightningStats.displayName, lightningStats.level, nextLevel, lightningStats.damageMult, nextDamage, lightningStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, projectilePierceBonus);
    }

    private int GetBaseProjectileCount(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return 1;
        }

        if (stats == novaStats)
        {
            return 8;
        }
        if (stats == shotgunStats)
        {
            return 5;
        }
        if (stats == chainStats)
        {
            return 3;
        }
        if (stats == droneStats)
        {
            return 1;
        }
        if (stats == shurikenStats)
        {
            return 1;
        }
        if (stats == frostStats)
        {
            return 1;
        }
        if (stats == lightningStats)
        {
            return 1;
        }

        return 1;
    }

    private static void ResetWeaponToLocked(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.unlocked = false;
        stats.level = 0;
    }

    private string BuildWeaponUpgradeText(string name, int currentLevel, int nextLevel, float currentDamage, float nextDamage, float currentRate, float nextRate, int currentProjectile, int nextProjectile, int currentPierce, int nextPierce)
    {
        var lines = new List<string>
        {
            name,
            $"레벨 {currentLevel} -> {nextLevel}"
        };

        AddLineIfChanged(lines, "피해량", currentDamage, nextDamage);
        AddLineIfChanged(lines, "속도", currentRate, nextRate);
        AddLineIfChanged(lines, "투사체", currentProjectile, nextProjectile);
        AddLineIfChanged(lines, "관통", currentPierce, nextPierce);

        return string.Join("\n", lines);
    }

    private static void AddLineIfChanged(List<string> lines, string label, float current, float next)
    {
        if (Mathf.Abs(next - current) < 0.0001f)
        {
            return;
        }

        lines.Add($"{label} {current:0.##} -> {next:0.##}");
    }

    private static void AddLineIfChanged(List<string> lines, string label, int current, int next)
    {
        if (current == next)
        {
            return;
        }

        lines.Add($"{label} {current} -> {next}");
    }

    private string BuildPercentStatText(string label, float currentMult, float nextMult)
    {
        return $"{label} {currentMult * 100f:0.#}% -> {nextMult * 100f:0.#}%";
    }

    private string BuildValueStatText(string label, float currentValue, float nextValue)
    {
        return $"{label} {currentValue:0.#} -> {nextValue:0.#}";
    }

    private string BuildHealthReinforceText()
    {
        float currentMax = PlayerHealth != null ? PlayerHealth.MaxHealth : 0f;
        float nextMax = currentMax + 25f;
        float nextRegen = regenPerSecond + 0.5f;
        var lines = new List<string>
        {
            BuildValueStatText("최대 체력", currentMax, nextMax),
            BuildValueStatText("체력 재생", regenPerSecond, nextRegen),
            "획득 시 체력 100% 회복"
        };
        return string.Join("\n", lines);
    }

    private string BuildMagnetUpgradeText()
    {
        float nextRange = magnetRangeMult + magnetRangeStep;
        float nextSpeed = magnetSpeedMult + magnetSpeedStep;
        var lines = new List<string>
        {
            BuildValueStatText("자석 범위", magnetRangeMult, nextRange),
            BuildValueStatText("자석 속도", magnetSpeedMult, nextSpeed)
        };
        return string.Join("\n", lines);
    }

    private string BuildProjectileCountText()
    {
        int nextCount = projectileCount + ((projectileCountLevel + 1) % 2 == 0 ? 1 : 0);
        var lines = new List<string>
        {
            BuildValueStatText("투사체 수", projectileCount, nextCount),
            "2회 업그레이드마다 +1"
        };
        return string.Join("\n", lines);
    }

    private void OnGUI()
    {
        if (IsGameOver)
        {
            DrawGameOverPanel();
            return;
        }

        if (_waitingStartWeaponChoice)
        {
            DrawStartWeaponChoice();
            return;
        }

        if (!_gameStarted)
        {
            return;
        }

        if (_choosingUpgrade)
        {
            DrawUpgradeChoices();
        }

        DrawAutoPlayToggle();
    }

    private void DrawGameOverPanel()
    {
        const float width = 360f;
        const float height = 180f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(x, y, width, height), "게임 오버");
        GUI.Label(new Rect(x + 20f, y + 40f, width - 40f, 24f), $"생존 시간 {ElapsedTime:0.0}s");

        if (GUI.Button(new Rect(x + 80f, y + 100f, width - 160f, 40f), "처음 화면으로"))
        {
            ResetToStart();
        }
    }

    private void ResetToStart()
    {
        Time.timeScale = 1f;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void DrawUpgradeChoices()
    {
        const int columns = 5;
        const float boxHeight = 200f;
        const float gap = 10f;
        const float topPadding = 36f;
        const float sidePadding = 12f;

        int optionCount = Mathf.Min(4, _options.Count);

        float maxWidth = Screen.width - 40f;
        float boxWidth = Mathf.Floor((maxWidth - sidePadding * 2f - (columns - 1) * gap) / columns);
        float w = columns * boxWidth + (columns - 1) * gap + sidePadding * 2f;
        float h = topPadding + boxHeight + sidePadding;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        GUI.Box(new Rect(x, y, w, h), "레벨업 선택");

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        style.fontSize = 13;

        for (int i = 0; i < optionCount; i++)
        {
            float bx = x + sidePadding + i * (boxWidth + gap);
            float by = y + topPadding;

            var opt = _options[i];
            if (GUI.Button(new Rect(bx, by, boxWidth, boxHeight), $"{i + 1}. {opt.Title}\n{opt.Desc}", style))
            {
                ApplyUpgrade(i);
            }
        }

        float rx = x + sidePadding + 4 * (boxWidth + gap);
        float ry = y + topPadding;
        var rerollStyle = new GUIStyle(GUI.skin.button);
        rerollStyle.alignment = TextAnchor.MiddleCenter;
        rerollStyle.wordWrap = true;
        rerollStyle.fontSize = 13;

        GUI.enabled = _rerollAvailable;
        if (GUI.Button(new Rect(rx, ry, boxWidth, boxHeight), _rerollAvailable ? "리롤\n(1회)" : "리롤 완료", rerollStyle))
        {
            _rerollAvailable = false;
            BuildUpgradeOptions(false);
            _autoUpgradeStartTime = _autoPlayEnabled ? Time.unscaledTime : -1f;
        }
        GUI.enabled = true;
    }

    private void DrawAutoPlayToggle()
    {
        if (_player == null)
        {
            return;
        }

        const float width = 140f;
        const float height = 40f;
        float x = 12f;
        float y = Screen.height - height - 12f;

        int level = PlayerExperience != null ? PlayerExperience.Level : 1;
        string label = _autoPlayEnabled ? $"자동 Lv{level}\n켜짐" : $"자동 Lv{level}\n꺼짐";
        if (GUI.Button(new Rect(x, y, width, height), label))
        {
            _autoPlayEnabled = !_autoPlayEnabled;
            _player.SetAutoPlay(_autoPlayEnabled);
        }
    }

    private class UpgradeOption
    {
        public string Title;
        public System.Func<string> DescProvider;
        public System.Action Apply;

        public string Desc => DescProvider != null ? DescProvider() : string.Empty;

        public UpgradeOption(string title, System.Func<string> descProvider, System.Action apply)
        {
            Title = title;
            DescProvider = descProvider;
            Apply = apply;
        }
    }

    private void DrawStartWeaponChoice()
    {
        const float boxWidth = 560f;
        const float boxHeight = 220f;
        float x = (Screen.width - boxWidth) * 0.5f;
        float y = (Screen.height - boxHeight) * 0.5f;

        GUI.Box(new Rect(x, y, boxWidth, boxHeight), "시작 캐릭터 선택");
        GUI.Label(new Rect(x + 20f, y + 36f, boxWidth - 40f, 20f), "캐릭터 스탯은 현재 동일합니다.");

        float buttonWidth = 160f;
        float buttonHeight = 120f;
        float gap = 20f;
        float bx = x + (boxWidth - (buttonWidth * 3f + gap * 2f)) * 0.5f;
        float by = y + 70f;

        var rectMage = new Rect(bx, by, buttonWidth, buttonHeight);
        var rectWarrior = new Rect(bx + buttonWidth + gap, by, buttonWidth, buttonHeight);
        var rectDemon = new Rect(bx + (buttonWidth + gap) * 2f, by, buttonWidth, buttonHeight);

        EnsureStartCharacterPreviews();
        UpdateStartCharacterPreviews(rectMage, rectWarrior, rectDemon);

        if (GUI.Button(rectMage, "마법사\n기본 무기: 총"))
        {
            SelectStartWeapon(StartWeapon.Gun);
        }
        if (GUI.Button(rectWarrior, "전사\n기본 무기: 부메랑"))
        {
            SelectStartWeapon(StartWeapon.Boomerang);
        }
        if (GUI.Button(rectDemon, "데몬로드\n기본 무기: 노바"))
        {
            SelectStartWeapon(StartWeapon.Nova);
        }
    }

    private void SelectStartWeapon(StartWeapon weapon)
    {
        startWeapon = weapon;
        if (gunStats != null)
        {
            gunStats.unlocked = weapon == StartWeapon.Gun;
            gunStats.level = weapon == StartWeapon.Gun ? 1 : 0;
        }

        if (boomerangStats != null)
        {
            boomerangStats.unlocked = weapon == StartWeapon.Boomerang;
            boomerangStats.level = weapon == StartWeapon.Boomerang ? 1 : 0;
        }

        if (novaStats != null)
        {
            novaStats.unlocked = weapon == StartWeapon.Nova;
            novaStats.level = weapon == StartWeapon.Nova ? 1 : 0;
        }

        ResetWeaponToLocked(shotgunStats);
        ResetWeaponToLocked(laserStats);
        ResetWeaponToLocked(chainStats);
        ResetWeaponToLocked(droneStats);
        ResetWeaponToLocked(shurikenStats);
        ResetWeaponToLocked(frostStats);
        ResetWeaponToLocked(lightningStats);

        if (weapon == StartWeapon.Gun && gunStats != null)
        {
            TrackUpgrade($"무기: {gunStats.displayName}");
        }
        else if (weapon == StartWeapon.Boomerang && boomerangStats != null)
        {
            TrackUpgrade($"무기: {boomerangStats.displayName}");
        }
        else if (weapon == StartWeapon.Nova && novaStats != null)
        {
            TrackUpgrade($"무기: {novaStats.displayName}");
        }

        _waitingStartWeaponChoice = false;
        ClearStartCharacterPreviews();
        StartLocalGame();
    }

    private void EnsureStartCharacterPreviews()
    {
        if (_startPreviews != null && _startPreviews.Length == 3)
        {
            return;
        }

        _startPreviews = new GameObject[3];
        _startPreviews[0] = CreateStartPreview("StartPreview_Mage", "Animations/Player_Wizard");
        _startPreviews[1] = CreateStartPreview("StartPreview_Warrior", "Animations/Player_Knight");
        _startPreviews[2] = CreateStartPreview("StartPreview_DemonLord", "Animations/Player_DemonLord");
    }

    private GameObject CreateStartPreview(string name, string controllerPath)
    {
        var go = new GameObject(name);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = startPreviewSortingOrder;
        renderer.color = Color.white;

        var animator = go.AddComponent<Animator>();
        var controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }

        return go;
    }

    private void UpdateStartCharacterPreviews(Rect rectMage, Rect rectWarrior, Rect rectDemon)
    {
        var cam = _cachedCamera != null ? _cachedCamera : Camera.main;
        if (cam == null)
        {
            return;
        }
        _cachedCamera = cam;

        UpdatePreviewTransform(_startPreviews[0], cam, rectMage);
        UpdatePreviewTransform(_startPreviews[1], cam, rectWarrior);
        UpdatePreviewTransform(_startPreviews[2], cam, rectDemon);
    }

    private void UpdatePreviewTransform(GameObject preview, Camera cam, Rect rect)
    {
        if (preview == null)
        {
            return;
        }

        float depth = Mathf.Abs(cam.transform.position.z);
        float screenX = rect.x + rect.width * 0.5f;
        float screenY = Screen.height - (rect.y + rect.height * 0.5f);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenX, screenY, depth));
        world.z = 0f;
        preview.transform.position = world;

        float topY = Screen.height - rect.y;
        float bottomY = Screen.height - (rect.y + rect.height);
        float worldTop = cam.ScreenToWorldPoint(new Vector3(0f, topY, depth)).y;
        float worldBottom = cam.ScreenToWorldPoint(new Vector3(0f, bottomY, depth)).y;
        float worldHeight = Mathf.Abs(worldTop - worldBottom);
        float scale = Mathf.Max(0.1f, worldHeight * startPreviewScale);
        preview.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void ClearStartCharacterPreviews()
    {
        if (_startPreviews == null)
        {
            return;
        }

        for (int i = 0; i < _startPreviews.Length; i++)
        {
            if (_startPreviews[i] != null)
            {
                Destroy(_startPreviews[i]);
            }
        }

        _startPreviews = null;
    }

    private static Sprite CreateCircleSprite(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var colors = new Color32[size * size];
        float r = (size - 1) * 0.5f;
        float cx = r;
        float cy = r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                bool inside = (dx * dx + dy * dy) <= r * r;
                colors[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

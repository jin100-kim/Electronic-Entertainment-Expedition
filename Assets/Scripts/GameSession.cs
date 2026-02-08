using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [SerializeField]
    private GameConfig gameConfig;
    [SerializeField]
    private StageConfig stageConfig;
    [SerializeField]
    private DifficultyConfig difficultyConfig;

    [Header("Start Mode")]
    private bool autoStartLocal = false;

    private bool showNetworkUI = true;

    [Header("Spawn")]
    private float spawnInterval = 2f;

    private int maxEnemies = 20;

    private float spawnRadius = 8f;

    [Header("Difficulty")]
    private float minSpawnInterval = 0.4f;

    private float spawnIntervalDecayPerSec = 0.01f;

    private int maxEnemiesPerMinute = 10;

    private float monsterLevelInterval = 60f;

    private float enemyHealthPerLevel = 0.15f;

    private float enemyDamagePerLevel = 0.10f;

    private float enemySpeedPerLevel = 0.05f;

    private float enemyXpPerLevel = 0f;

    private float enemyHealthMultiplier = 1f;

    private float enemyDamageMultiplier = 1f;

    private float enemySpeedMultiplier = 1f;

    private float enemyXpMultiplier = 1f;

    [Header("Player")]
    private Vector3 localSpawnPosition = Vector3.zero;

    [Header("Map Bounds")]
    private Vector2 mapHalfSize = new Vector2(24f, 24f);

    [Header("Stage")]
    private float stageTimeLimitSeconds = 0f;

    private int stageKillTarget = 0;

    [Header("Upgrades")]
    private int maxUpgradeLevel = 10;

    private int maxWeaponSlots = 5;

    private int maxStatSlots = 5;

    private float damageMult = 1f;

    private float fireRateMult = 1f;

    private float rangeMult = 1f;

    private float sizeMult = 1f;

    private float lifetimeMult = 1f;

    private int projectileCount = 1;

    private int projectilePierceBonus = 0;

    private float weaponDamageMult = 1f;

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

    private float moveSpeedMult = 1f;

    private float xpGainMult = 1f;

    private float magnetRangeMult = 1f;

    private float magnetSpeedMult = 1f;

    private float magnetRangeStep = 0.5f;

    private float magnetSpeedStep = 0.5f;

    private float regenPerSecond = 0f;

    [Header("Drops")]
    private float coinDropChance = 0.06f;

    private int coinAmount = 1;

    [Header("Upgrade Levels")]
    private int damageLevel = 0;

    private int fireRateLevel = 0;

    private int moveSpeedLevel = 0;

    private int healthReinforceLevel = 0;

    private int rangeLevel = 0;

    private int xpGainLevel = 0;

    private int sizeLevel = 0;

    private int magnetLevel = 0;

    private int pierceLevel = 0;

    private int projectileCountLevel = 0;

    [Header("Start Weapon")]
    private bool requireStartWeaponChoice = true;

    private StartWeaponType startWeapon = StartWeaponType.Gun;

    [Header("Start Character Preview")]
    private float startPreviewScale = 2f;

    private float startPreviewDimAlpha = 0.5f;

    private float startPreviewHoverAlpha = 1f;

    private int startPreviewSortingOrder = 5000;

    private float startPreviewYOffset = -0.5f;

    [Header("UI")]
    private bool useUGUI = true;

    private Vector2 uiReferenceResolution = new Vector2(1280f, 720f);

    private Font uiFont;

    private Color startButtonNormalColor = new Color(0f, 0f, 0f, 0.25f);

    private Color startButtonHoverColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);

    private Color upgradeButtonNormalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    private Color upgradeButtonHoverColor = new Color(0.35f, 0.35f, 0.35f, 0.95f);

    private Color startButtonClickColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private Color upgradeButtonClickColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private float selectionClickDuration = 0.3f;

    private float selectionClickScale = 0.96f;

    private Color selectionOutlineColor = new Color(1f, 1f, 1f, 0.9f);

    private float selectionOutlineSize = 2f;

    private float selectionFlashStrength = 1f;

    [Header("Developer")]
    private bool allowAutoButtonSecret = true;

    private string autoButtonSecret = "auto";

    private float autoButtonSecretTimeout = 1.5f;

    public Vector2 MapHalfSize => mapHalfSize;
    public int MonsterLevel => Mathf.Max(1, 1 + Mathf.FloorToInt(ElapsedTime / Mathf.Max(1f, monsterLevelInterval)));
    public bool IsWaitingStartWeaponChoice => _waitingStartWeaponChoice;
    public bool IsGameplayActive => _gameStarted && !_waitingStartWeaponChoice;
    public bool IsChoosingUpgrade => _choosingUpgrade;

    private bool StraightUnlocked => gunStats != null && gunStats.unlocked && gunStats.level > 0;

    public bool IsGameOver { get; private set; }
    public bool IsStageComplete => _stageCompleted;
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
    private bool _spawnerDifficultyApplied;
    private AutoAttack _attack;
    private PlayerController _player;
    private bool _gameStarted;
    private bool _stageCompleted;

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
    private int _startPreviewHoverIndex = -1;
    private Canvas _uiCanvas;
    private RectTransform _uiRoot;
    private RectTransform _upgradePanel;
    private Text _upgradeTitleText;
    private Button[] _upgradeButtons;
    private Text[] _upgradeButtonTexts;
    private Button _rerollButton;
    private Text _rerollButtonText;
    private RectTransform _gameOverPanel;
    private Text _gameOverTitleText;
    private Text _gameOverTimeText;
    private Button _gameOverButton;
    private RectTransform _startPanel;
    private Text _startTitleText;
    private RectTransform _startMageRect;
    private RectTransform _startWarriorRect;
    private RectTransform _startDemonRect;
    private RectTransform _startMagePreviewRect;
    private RectTransform _startWarriorPreviewRect;
    private RectTransform _startDemonPreviewRect;
    private RectTransform _autoButtonRect;
    private Text _autoButtonText;
    private bool _uiReady;
    private bool _selectionLocked;
    private Coroutine _selectionFeedbackRoutine;
    private bool _showAutoButton;
    private string _autoSecretBuffer = string.Empty;
    private float _autoSecretLastTime = -1f;
    private bool _settingsApplied;

    private const string CoinPrefKey = "CoinCount";
    private int _coinCount;
    private int _killCount;

    private void Awake()
    {
        ApplySettings();
        ResetRuntimeState();
        Instance = this;
        _coinCount = PlayerPrefs.GetInt(CoinPrefKey, 0);
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.game;
        var stage = ResolveStageConfig(config);
        var difficulty = ResolveDifficultyConfig(config);

        autoStartLocal = settings.autoStartLocal;
        showNetworkUI = settings.showNetworkUI;

        spawnInterval = settings.spawnInterval;
        maxEnemies = settings.maxEnemies;
        spawnRadius = settings.spawnRadius;

        minSpawnInterval = settings.minSpawnInterval;
        spawnIntervalDecayPerSec = settings.spawnIntervalDecayPerSec;
        maxEnemiesPerMinute = settings.maxEnemiesPerMinute;
        monsterLevelInterval = settings.monsterLevelInterval;
        enemyHealthPerLevel = settings.enemyHealthPerLevel;
        enemyDamagePerLevel = settings.enemyDamagePerLevel;
        enemySpeedPerLevel = settings.enemySpeedPerLevel;
        enemyXpPerLevel = settings.enemyXpPerLevel;

        localSpawnPosition = settings.localSpawnPosition;
        mapHalfSize = settings.mapHalfSize;

        maxUpgradeLevel = settings.maxUpgradeLevel;
        maxWeaponSlots = settings.maxWeaponSlots;
        maxStatSlots = settings.maxStatSlots;

        damageMult = settings.damageMult;
        fireRateMult = settings.fireRateMult;
        rangeMult = settings.rangeMult;
        sizeMult = settings.sizeMult;
        lifetimeMult = settings.lifetimeMult;
        projectileCount = settings.projectileCount;
        projectilePierceBonus = settings.projectilePierceBonus;
        weaponDamageMult = settings.weaponDamageMult;

        gunStats = CloneWeaponStats(settings.gunStats);
        boomerangStats = CloneWeaponStats(settings.boomerangStats);
        novaStats = CloneWeaponStats(settings.novaStats);
        shotgunStats = CloneWeaponStats(settings.shotgunStats);
        laserStats = CloneWeaponStats(settings.laserStats);
        chainStats = CloneWeaponStats(settings.chainStats);
        droneStats = CloneWeaponStats(settings.droneStats);
        shurikenStats = CloneWeaponStats(settings.shurikenStats);
        frostStats = CloneWeaponStats(settings.frostStats);
        lightningStats = CloneWeaponStats(settings.lightningStats);

        moveSpeedMult = settings.moveSpeedMult;
        xpGainMult = settings.xpGainMult;
        magnetRangeMult = settings.magnetRangeMult;
        magnetSpeedMult = settings.magnetSpeedMult;
        magnetRangeStep = settings.magnetRangeStep;
        magnetSpeedStep = settings.magnetSpeedStep;
        regenPerSecond = settings.regenPerSecond;

        coinDropChance = settings.coinDropChance;
        coinAmount = settings.coinAmount;

        requireStartWeaponChoice = settings.requireStartWeaponChoice;
        startWeapon = settings.startWeapon;

        startPreviewScale = settings.startPreviewScale;
        startPreviewDimAlpha = settings.startPreviewDimAlpha;
        startPreviewHoverAlpha = settings.startPreviewHoverAlpha;
        startPreviewSortingOrder = settings.startPreviewSortingOrder;
        startPreviewYOffset = settings.startPreviewYOffset;

        useUGUI = settings.useUGUI;
        uiReferenceResolution = settings.uiReferenceResolution;
        uiFont = settings.uiFont;
        startButtonNormalColor = settings.startButtonNormalColor;
        startButtonHoverColor = settings.startButtonHoverColor;
        upgradeButtonNormalColor = settings.upgradeButtonNormalColor;
        upgradeButtonHoverColor = settings.upgradeButtonHoverColor;
        startButtonClickColor = settings.startButtonClickColor;
        upgradeButtonClickColor = settings.upgradeButtonClickColor;
        selectionClickDuration = settings.selectionClickDuration;
        selectionClickScale = settings.selectionClickScale;
        selectionOutlineColor = settings.selectionOutlineColor;
        selectionOutlineSize = settings.selectionOutlineSize;
        selectionFlashStrength = settings.selectionFlashStrength;

        allowAutoButtonSecret = settings.allowAutoButtonSecret;
        autoButtonSecret = settings.autoButtonSecret;
        autoButtonSecretTimeout = settings.autoButtonSecretTimeout;

        ApplyStageOverrides(stage);
        ApplyDifficultyOverrides(difficulty);

        _settingsApplied = true;
    }

    private void ResetRuntimeState()
    {
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
        _cachedSpawnerBase = false;
        _spawnerDifficultyApplied = false;
        _stageCompleted = false;
    }

    private static WeaponStatsData CloneWeaponStats(WeaponStatsData source)
    {
        if (source == null)
        {
            return new WeaponStatsData();
        }

        return new WeaponStatsData
        {
            displayName = source.displayName,
            level = source.level,
            unlocked = source.unlocked,
            damageMult = source.damageMult,
            fireRateMult = source.fireRateMult,
            rangeMult = source.rangeMult,
            bonusProjectiles = source.bonusProjectiles
        };
    }

    private StageConfig ResolveStageConfig(GameConfig config)
    {
        if (stageConfig != null)
        {
            return stageConfig;
        }

        if (config != null && config.defaultStage != null)
        {
            return config.defaultStage;
        }

        return null;
    }

    private DifficultyConfig ResolveDifficultyConfig(GameConfig config)
    {
        if (difficultyConfig != null)
        {
            return difficultyConfig;
        }

        if (config != null && config.defaultDifficulty != null)
        {
            return config.defaultDifficulty;
        }

        return null;
    }

    private void ApplyStageOverrides(StageConfig stage)
    {
        if (stage == null)
        {
            return;
        }

        stageTimeLimitSeconds = Mathf.Max(0f, stage.timeLimitSeconds);
        stageKillTarget = Mathf.Max(0, stage.killTarget);
        mapHalfSize = stage.mapHalfSize;
        localSpawnPosition = stage.localSpawnPosition;

        spawnInterval = stage.spawnInterval;
        maxEnemies = stage.maxEnemies;
        spawnRadius = stage.spawnRadius;
        minSpawnInterval = stage.minSpawnInterval;
        spawnIntervalDecayPerSec = stage.spawnIntervalDecayPerSec;
        maxEnemiesPerMinute = stage.maxEnemiesPerMinute;
    }

    private void ApplyDifficultyOverrides(DifficultyConfig difficulty)
    {
        enemyHealthMultiplier = 1f;
        enemyDamageMultiplier = 1f;
        enemySpeedMultiplier = 1f;
        enemyXpMultiplier = 1f;

        if (difficulty == null)
        {
            return;
        }

        spawnInterval *= difficulty.spawnIntervalMultiplier;
        minSpawnInterval *= difficulty.minSpawnIntervalMultiplier;
        spawnIntervalDecayPerSec *= difficulty.spawnIntervalDecayMultiplier;
        maxEnemies = Mathf.RoundToInt(maxEnemies * difficulty.maxEnemiesMultiplier);
        maxEnemiesPerMinute = Mathf.RoundToInt(maxEnemiesPerMinute * difficulty.maxEnemiesPerMinuteMultiplier);

        enemyHealthMultiplier = difficulty.enemyHealthMultiplier;
        enemyDamageMultiplier = difficulty.enemyDamageMultiplier;
        enemySpeedMultiplier = difficulty.enemySpeedMultiplier;
        enemyXpMultiplier = difficulty.enemyXpMultiplier;

        enemyHealthPerLevel *= difficulty.enemyHealthPerLevelMultiplier;
        enemyDamagePerLevel *= difficulty.enemyDamagePerLevelMultiplier;
        enemySpeedPerLevel *= difficulty.enemySpeedPerLevelMultiplier;
        enemyXpPerLevel *= difficulty.enemyXpPerLevelMultiplier;

        coinDropChance *= difficulty.coinDropChanceMultiplier;
        xpGainMult *= difficulty.xpGainMultiplier;
    }

    private void ApplyDifficultyToSpawner()
    {
        if (_spawner == null || _spawnerDifficultyApplied)
        {
            return;
        }

        _spawner.EnemyMaxHealth *= enemyHealthMultiplier;
        _spawner.EnemyDamage *= enemyDamageMultiplier;
        _spawner.EnemyMoveSpeed *= enemySpeedMultiplier;
        _spawner.EnemyXpReward = Mathf.Max(1, Mathf.RoundToInt(_spawner.EnemyXpReward * enemyXpMultiplier));
        _spawnerDifficultyApplied = true;
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
            if (_selectionLocked)
            {
                return;
            }

            if (_autoUpgradeStartTime < 0f)
            {
                _autoUpgradeStartTime = Time.unscaledTime;
            }

            if (Time.unscaledTime - _autoUpgradeStartTime >= 1f)
            {
                int index = PickAutoUpgradeIndex();
                if (index >= 0)
                {
                    SelectUpgradeWithFeedback(index);
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
            CheckStageCompletion();
        }
        else
        {
            HandleUpgradeHotkeys();
        }

        HandleAutoButtonSecret();
    }

    private void LateUpdate()
    {
        if (!useUGUI)
        {
            return;
        }

        if (!_uiReady)
        {
            BuildUGUI();
        }

        UpdateUGUI();
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
            var players = PlayerController.Active;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
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

    private void ApplyPlayerVisuals(StartWeaponType weapon)
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
            case StartWeaponType.Boomerang:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.Warrior);
                break;
            case StartWeaponType.Nova:
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

        ApplyDifficultyToSpawner();
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
            _options.Add(new UpgradeOption("HP 회복 (20%)", () => "최대 체력의 20%를 회복합니다.", () =>
            {
                if (PlayerHealth != null)
                {
                    PlayerHealth.Heal(PlayerHealth.MaxHealth * 0.2f);
                }
            }));
            _options.Add(new UpgradeOption("코인 +10", () => "즉시 코인 10개를 획득합니다.", () => AddCoins(10)));
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
        if (!_stageCompleted && stageKillTarget > 0 && _killCount >= stageKillTarget)
        {
            CompleteStage();
        }
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
        CoinPickup.Spawn(position, coinAmount);
    }

    private int PickAutoUpgradeIndex()
    {
        if (_options == null || _options.Count == 0)
        {
            return -1;
        }

        float totalWeight = 0f;
        var weights = new float[_options.Count];
        for (int i = 0; i < _options.Count; i++)
        {
            int score = ScoreUpgradeOption(_options[i]);
            float weight = 1f + Mathf.Max(0, score);
            weights[i] = weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0.0001f)
        {
            return Random.Range(0, _options.Count);
        }

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (pick <= cumulative)
            {
                return i;
            }
        }

        return _options.Count - 1;
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

    private void CheckStageCompletion()
    {
        if (_stageCompleted || IsGameOver)
        {
            return;
        }

        if (stageTimeLimitSeconds > 0f && ElapsedTime >= stageTimeLimitSeconds)
        {
            CompleteStage();
            return;
        }

        if (stageKillTarget > 0 && _killCount >= stageKillTarget)
        {
            CompleteStage();
        }
    }

    private void CompleteStage()
    {
        if (_stageCompleted)
        {
            return;
        }

        _stageCompleted = true;
        IsGameOver = true;
        _choosingUpgrade = false;
        Time.timeScale = 1f;

        if (_spawner != null)
        {
            _spawner.enabled = false;
        }
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

    private void BuildUGUI()
    {
        if (_uiReady)
        {
            return;
        }

        var fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();

        var canvasGo = new GameObject("GameSessionUI");
        canvasGo.transform.SetParent(transform, false);
        _uiCanvas = canvasGo.AddComponent<Canvas>();
        _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _uiCanvas.sortingOrder = 1200;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = uiReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        _uiRoot = canvasGo.GetComponent<RectTransform>();

        BuildGameOverUI(fontToUse);
        BuildStartChoiceUI(fontToUse);
        BuildUpgradeUI(fontToUse);
        BuildAutoButtonUI(fontToUse);

        _uiReady = true;
    }

    private void UpdateUGUI()
    {
        if (_uiRoot == null)
        {
            return;
        }

        bool showStart = _waitingStartWeaponChoice && !IsGameOver;
        bool showGameOver = IsGameOver;
        bool showUpgrade = _choosingUpgrade && !showStart && !IsGameOver;
        bool showAuto = _showAutoButton && _player != null && _gameStarted && !_waitingStartWeaponChoice && !IsGameOver;

        if (_startPanel != null)
        {
            _startPanel.gameObject.SetActive(showStart);
        }
        if (_startTitleText != null)
        {
            _startTitleText.gameObject.SetActive(showStart);
        }
        if (_gameOverPanel != null)
        {
            _gameOverPanel.gameObject.SetActive(showGameOver);
        }
        if (_upgradePanel != null)
        {
            _upgradePanel.gameObject.SetActive(showUpgrade);
        }
        if (_autoButtonRect != null)
        {
            _autoButtonRect.gameObject.SetActive(showAuto);
        }

        if (showGameOver && _gameOverTitleText != null)
        {
            _gameOverTitleText.text = _stageCompleted ? "스테이지 완료" : "게임 오버";
        }

        if (showGameOver && _gameOverTimeText != null)
        {
            string label = _stageCompleted ? "클리어 시간" : "생존 시간";
            _gameOverTimeText.text = $"{label} {ElapsedTime:0.0}s";
        }

        if (showUpgrade)
        {
            UpdateUpgradeUI();
        }

        if (_autoButtonText != null)
        {
            _autoButtonText.text = _autoPlayEnabled ? "자동\n켜짐" : "자동\n꺼짐";
        }

        if (showStart)
        {
            EnsureStartCharacterPreviews();
            UpdateStartPreviewsFromUI();
            UpdateStartPreviewColors();
        }
        else
        {
            _startPreviewHoverIndex = -1;
            ClearStartCharacterPreviews();
        }
    }

    private void UpdateUpgradeUI()
    {
        if (_upgradeButtons == null || _upgradeButtonTexts == null)
        {
            return;
        }

        int optionCount = Mathf.Min(4, _options.Count);
        for (int i = 0; i < _upgradeButtons.Length; i++)
        {
            bool active = i < optionCount;
            _upgradeButtons[i].gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            var opt = _options[i];
            _upgradeButtonTexts[i].text = $"{i + 1}. {opt.Title}\n{opt.Desc}";
        }

        if (_rerollButton != null)
        {
            _rerollButton.interactable = _rerollAvailable;
        }
        if (_rerollButtonText != null)
        {
            _rerollButtonText.text = _rerollAvailable ? "리롤\n(1회)" : "리롤 완료";
        }
    }

    private void SelectStartWeaponWithFeedback(StartWeaponType weapon, Button button)
    {
        if (_selectionLocked)
        {
            return;
        }

        BeginSelectionFeedback(button, startButtonClickColor, () => SelectStartWeapon(weapon));
    }

    private void SelectUpgradeWithFeedback(int index)
    {
        if (_selectionLocked)
        {
            return;
        }

        if (_options == null || index < 0 || index >= _options.Count)
        {
            return;
        }

        Button button = null;
        if (_upgradeButtons != null && index >= 0 && index < _upgradeButtons.Length)
        {
            button = _upgradeButtons[index];
        }

        BeginSelectionFeedback(button, upgradeButtonClickColor, () => ApplyUpgrade(index));
    }

    private void BeginSelectionFeedback(Button button, Color clickColor, System.Action onComplete)
    {
        if (_selectionLocked)
        {
            return;
        }

        _selectionLocked = true;

        if (_selectionFeedbackRoutine != null)
        {
            StopCoroutine(_selectionFeedbackRoutine);
        }

        _selectionFeedbackRoutine = StartCoroutine(PlaySelectionFeedback(button, clickColor, onComplete));
    }

    private IEnumerator PlaySelectionFeedback(Button button, Color clickColor, System.Action onComplete)
    {
        var image = button != null ? button.GetComponent<Image>() : null;
        var originalColor = image != null ? image.color : Color.white;
        var originalScale = button != null ? button.transform.localScale : Vector3.one;
        var originalTransition = button != null ? button.transition : Selectable.Transition.ColorTint;
        var outline = button != null ? button.GetComponent<Outline>() : null;
        bool createdOutline = false;
        bool originalOutlineEnabled = false;
        Vector2 originalOutlineDistance = Vector2.zero;
        Color originalOutlineColor = Color.clear;

        if (button != null)
        {
            float scale = Mathf.Clamp(selectionClickScale, 0.5f, 1.2f);
            button.transform.localScale = originalScale * scale;
            button.transition = Selectable.Transition.None;

            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
                createdOutline = true;
            }

            originalOutlineEnabled = outline.enabled;
            originalOutlineDistance = outline.effectDistance;
            originalOutlineColor = outline.effectColor;
            outline.effectDistance = new Vector2(selectionOutlineSize, selectionOutlineSize);
            outline.effectColor = selectionOutlineColor;
            outline.enabled = true;
        }

        if (image != null)
        {
            image.color = clickColor;
        }

        float wait = Mathf.Max(0.05f, selectionClickDuration);
        float half = wait * 0.5f;
        if (half > 0f)
        {
            yield return new WaitForSecondsRealtime(half);
        }

        if (image != null)
        {
            Color flashColor = Color.Lerp(clickColor, Color.white, Mathf.Clamp01(selectionFlashStrength));
            flashColor.a = clickColor.a;
            image.color = flashColor;
        }

        float remaining = wait - half;
        if (remaining > 0f)
        {
            yield return new WaitForSecondsRealtime(remaining);
        }

        if (button != null)
        {
            button.transform.localScale = originalScale;
            button.transition = originalTransition;
            if (outline != null)
            {
                outline.effectDistance = originalOutlineDistance;
                outline.effectColor = originalOutlineColor;
                outline.enabled = originalOutlineEnabled;
                if (createdOutline)
                {
                    Destroy(outline);
                }
            }
        }

        if (image != null)
        {
            image.color = originalColor;
        }

        _selectionLocked = false;
        onComplete?.Invoke();
        _selectionFeedbackRoutine = null;
    }

    private void HandleUpgradeHotkeys()
    {
        if (!_choosingUpgrade)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (IsNumberKeyPressed(keyboard, 1))
        {
            SelectUpgradeWithFeedback(0);
            return;
        }
        if (IsNumberKeyPressed(keyboard, 2))
        {
            SelectUpgradeWithFeedback(1);
            return;
        }
        if (IsNumberKeyPressed(keyboard, 3))
        {
            SelectUpgradeWithFeedback(2);
            return;
        }
        if (IsNumberKeyPressed(keyboard, 4))
        {
            SelectUpgradeWithFeedback(3);
            return;
        }
        if (IsNumberKeyPressed(keyboard, 5))
        {
            TryReroll();
        }
#else
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectUpgradeWithFeedback(0);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectUpgradeWithFeedback(1);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectUpgradeWithFeedback(2);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            SelectUpgradeWithFeedback(3);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            TryReroll();
        }
#endif
    }

    private void HandleAutoButtonSecret()
    {
        if (!allowAutoButtonSecret || string.IsNullOrEmpty(autoButtonSecret))
        {
            return;
        }

        if (_autoSecretLastTime > 0f && Time.unscaledTime - _autoSecretLastTime > autoButtonSecretTimeout)
        {
            _autoSecretBuffer = string.Empty;
            _autoSecretLastTime = -1f;
        }

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool appended = false;
        foreach (char c in autoButtonSecret)
        {
            if (WasSecretCharPressed(keyboard, c))
            {
                AppendAutoSecretChar(c);
                appended = true;
                break;
            }
        }

        if (!appended)
        {
            return;
        }
#else
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        for (int i = 0; i < input.Length; i++)
        {
            AppendAutoSecretChar(char.ToLowerInvariant(input[i]));
        }
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static bool WasSecretCharPressed(Keyboard keyboard, char c)
    {
        switch (char.ToLowerInvariant(c))
        {
            case 'a':
                return keyboard.aKey.wasPressedThisFrame;
            case 'u':
                return keyboard.uKey.wasPressedThisFrame;
            case 't':
                return keyboard.tKey.wasPressedThisFrame;
            case 'o':
                return keyboard.oKey.wasPressedThisFrame;
            default:
                return false;
        }
    }
#endif

    private void AppendAutoSecretChar(char c)
    {
        _autoSecretLastTime = Time.unscaledTime;
        _autoSecretBuffer += char.ToLowerInvariant(c);
        if (_autoSecretBuffer.Length > autoButtonSecret.Length)
        {
            _autoSecretBuffer = _autoSecretBuffer.Substring(_autoSecretBuffer.Length - autoButtonSecret.Length);
        }

        if (_autoSecretBuffer == autoButtonSecret.ToLowerInvariant())
        {
            _showAutoButton = !_showAutoButton;
            _autoSecretBuffer = string.Empty;
            _autoSecretLastTime = -1f;
        }
    }

#if ENABLE_INPUT_SYSTEM
    private static bool IsNumberKeyPressed(Keyboard keyboard, int number)
    {
        switch (number)
        {
            case 1:
                return keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame;
            case 2:
                return keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame;
            case 3:
                return keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame;
            case 4:
                return keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame;
            case 5:
                return keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame;
            default:
                return false;
        }
    }
#endif

    private void TryReroll()
    {
        if (!_rerollAvailable)
        {
            return;
        }

        _rerollAvailable = false;
        BuildUpgradeOptions(false);
        _autoUpgradeStartTime = _autoPlayEnabled ? Time.unscaledTime : -1f;
    }

    private void BuildGameOverUI(Font fontToUse)
    {
        _gameOverPanel = CreatePanel(_uiRoot, "GameOverPanel", new Vector2(360f, 180f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0f, 0f, 0f, 0.6f));

        var title = CreateText(_gameOverPanel, "Title", fontToUse, 20, TextAnchor.UpperCenter, Color.white);
        _gameOverTitleText = title;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(200f, 24f);
        title.text = "게임 오버";

        _gameOverTimeText = CreateText(_gameOverPanel, "Time", fontToUse, 16, TextAnchor.UpperCenter, Color.white);
        var timeRect = _gameOverTimeText.rectTransform;
        timeRect.anchorMin = new Vector2(0.5f, 1f);
        timeRect.anchorMax = new Vector2(0.5f, 1f);
        timeRect.pivot = new Vector2(0.5f, 1f);
        timeRect.anchoredPosition = new Vector2(0f, -50f);
        timeRect.sizeDelta = new Vector2(240f, 22f);

        _gameOverButton = CreateButton(_gameOverPanel, "RestartButton", new Vector2(200f, 40f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Color(0.2f, 0.2f, 0.2f, 0.9f));
        var btnLabel = CreateText(_gameOverButton.transform, "Label", fontToUse, 14, TextAnchor.MiddleCenter, Color.white);
        StretchToFill(btnLabel.rectTransform, new Vector2(4f, 4f));
        btnLabel.text = "처음 화면으로";
        _gameOverButton.onClick.AddListener(ResetToStart);
    }

    private void BuildStartChoiceUI(Font fontToUse)
    {
        float panelWidth = 560f;
        float panelHeight = 240f;
        _startPanel = CreatePanel(_uiRoot, "StartWeaponPanel", new Vector2(panelWidth, panelHeight), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0f, 0f, 0f, 0.6f));

        _startTitleText = CreateText(_uiRoot, "StartWeaponTitle", fontToUse, 18, TextAnchor.MiddleCenter, Color.white);
        var titleRect = _startTitleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0f);
        titleRect.anchoredPosition = new Vector2(0f, panelHeight * 0.5f + 12f);
        titleRect.sizeDelta = new Vector2(320f, 24f);
        _startTitleText.text = "시작 캐릭터 선택";

        var subtitle = CreateText(_startPanel, "Subtitle", fontToUse, 12, TextAnchor.UpperCenter, new Color(1f, 1f, 1f, 0.9f));
        var subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -10f);
        subtitleRect.sizeDelta = new Vector2(320f, 20f);
        subtitle.text = "캐릭터 스탯은 현재 동일합니다.";

        float buttonWidth = 160f;
        float buttonHeight = 120f;
        float gap = 20f;
        float totalWidth = buttonWidth * 3f + gap * 2f;
        float leftX = -totalWidth * 0.5f + buttonWidth * 0.5f;
        float midX = 0f;
        float rightX = totalWidth * 0.5f - buttonWidth * 0.5f;
        float buttonY = -80f;
        float labelHeight = 36f;
        float labelPadding = 6f;
        float previewPadding = 6f;
        float previewHeight = Mathf.Max(30f, buttonHeight - labelHeight - previewPadding - labelPadding);

        _startMageRect = CreateButton(_startPanel, "MageButton", new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(leftX, buttonY), startButtonNormalColor).GetComponent<RectTransform>();
        var mageLabel = CreateText(_startMageRect, "MageLabel", fontToUse, 12, TextAnchor.LowerCenter, Color.white);
        var mageLabelRect = mageLabel.rectTransform;
        mageLabelRect.anchorMin = new Vector2(0.5f, 0f);
        mageLabelRect.anchorMax = new Vector2(0.5f, 0f);
        mageLabelRect.pivot = new Vector2(0.5f, 0f);
        mageLabelRect.anchoredPosition = new Vector2(0f, labelPadding);
        mageLabelRect.sizeDelta = new Vector2(buttonWidth - 8f, labelHeight);
        mageLabel.text = "마법사\n기본 무기: 총";
        _startMagePreviewRect = CreateRect(_startMageRect, "MagePreview", new Vector2(buttonWidth - 8f, previewHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -previewPadding));
        var mageButton = _startMageRect.GetComponent<Button>();
        mageButton.onClick.AddListener(() => SelectStartWeapon(StartWeaponType.Gun));
        AddStartHoverTrigger(mageButton, 0);
        ApplyButtonColors(mageButton, startButtonNormalColor, startButtonHoverColor);

        _startWarriorRect = CreateButton(_startPanel, "WarriorButton", new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(midX, buttonY), startButtonNormalColor).GetComponent<RectTransform>();
        var warriorLabel = CreateText(_startWarriorRect, "WarriorLabel", fontToUse, 12, TextAnchor.LowerCenter, Color.white);
        var warriorLabelRect = warriorLabel.rectTransform;
        warriorLabelRect.anchorMin = new Vector2(0.5f, 0f);
        warriorLabelRect.anchorMax = new Vector2(0.5f, 0f);
        warriorLabelRect.pivot = new Vector2(0.5f, 0f);
        warriorLabelRect.anchoredPosition = new Vector2(0f, labelPadding);
        warriorLabelRect.sizeDelta = new Vector2(buttonWidth - 8f, labelHeight);
        warriorLabel.text = "전사\n기본 무기: 부메랑";
        _startWarriorPreviewRect = CreateRect(_startWarriorRect, "WarriorPreview", new Vector2(buttonWidth - 8f, previewHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -previewPadding));
        var warriorButton = _startWarriorRect.GetComponent<Button>();
        warriorButton.onClick.AddListener(() => SelectStartWeapon(StartWeaponType.Boomerang));
        AddStartHoverTrigger(warriorButton, 1);
        ApplyButtonColors(warriorButton, startButtonNormalColor, startButtonHoverColor);

        _startDemonRect = CreateButton(_startPanel, "DemonButton", new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(rightX, buttonY), startButtonNormalColor).GetComponent<RectTransform>();
        var demonLabel = CreateText(_startDemonRect, "DemonLabel", fontToUse, 12, TextAnchor.LowerCenter, Color.white);
        var demonLabelRect = demonLabel.rectTransform;
        demonLabelRect.anchorMin = new Vector2(0.5f, 0f);
        demonLabelRect.anchorMax = new Vector2(0.5f, 0f);
        demonLabelRect.pivot = new Vector2(0.5f, 0f);
        demonLabelRect.anchoredPosition = new Vector2(0f, labelPadding);
        demonLabelRect.sizeDelta = new Vector2(buttonWidth - 8f, labelHeight);
        demonLabel.text = "데몬로드\n기본 무기: 노바";
        _startDemonPreviewRect = CreateRect(_startDemonRect, "DemonPreview", new Vector2(buttonWidth - 8f, previewHeight), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -previewPadding));
        var demonButton = _startDemonRect.GetComponent<Button>();
        demonButton.onClick.AddListener(() => SelectStartWeapon(StartWeaponType.Nova));
        AddStartHoverTrigger(demonButton, 2);
        ApplyButtonColors(demonButton, startButtonNormalColor, startButtonHoverColor);

    }

    private void BuildUpgradeUI(Font fontToUse)
    {
        const int columns = 5;
        const float boxHeight = 200f;
        const float gap = 10f;
        const float topPadding = 36f;
        const float sidePadding = 12f;

        float maxWidth = uiReferenceResolution.x - 40f;
        float boxWidth = Mathf.Floor((maxWidth - sidePadding * 2f - (columns - 1) * gap) / columns);
        float panelWidth = columns * boxWidth + (columns - 1) * gap + sidePadding * 2f;
        float panelHeight = topPadding + boxHeight + sidePadding;

        _upgradePanel = CreatePanel(_uiRoot, "UpgradePanel", new Vector2(panelWidth, panelHeight), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0f, 0f, 0f, 0.6f));

        _upgradeTitleText = CreateText(_upgradePanel, "Title", fontToUse, 18, TextAnchor.UpperCenter, Color.white);
        var titleRect = _upgradeTitleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(240f, 24f);
        _upgradeTitleText.text = "레벨업 선택";

        _upgradeButtons = new Button[4];
        _upgradeButtonTexts = new Text[4];

        for (int i = 0; i < _upgradeButtons.Length; i++)
        {
            float bx = sidePadding + i * (boxWidth + gap);
            float by = -topPadding;
            var button = CreateButton(_upgradePanel, $"UpgradeButton_{i}", new Vector2(boxWidth, boxHeight), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(bx, by), upgradeButtonNormalColor);
            var label = CreateText(button.transform, "Label", fontToUse, 13, TextAnchor.UpperLeft, Color.white);
            StretchToFill(label.rectTransform, new Vector2(6f, 6f));
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            int index = i;
            button.onClick.AddListener(() => SelectUpgradeWithFeedback(index));
            ApplyButtonColors(button, upgradeButtonNormalColor, upgradeButtonHoverColor);
            _upgradeButtons[i] = button;
            _upgradeButtonTexts[i] = label;
        }

        float rx = sidePadding + 4 * (boxWidth + gap);
        float ry = -topPadding;
        _rerollButton = CreateButton(_upgradePanel, "RerollButton", new Vector2(boxWidth, boxHeight), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(rx, ry), upgradeButtonNormalColor);
        _rerollButtonText = CreateText(_rerollButton.transform, "Label", fontToUse, 13, TextAnchor.MiddleCenter, Color.white);
        StretchToFill(_rerollButtonText.rectTransform, new Vector2(6f, 6f));
        _rerollButton.onClick.AddListener(TryReroll);
        ApplyButtonColors(_rerollButton, upgradeButtonNormalColor, upgradeButtonHoverColor);
    }

    private void BuildAutoButtonUI(Font fontToUse)
    {
        _autoButtonRect = CreateButton(_uiRoot, "AutoPlayButton", new Vector2(140f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 12f), new Color(0.2f, 0.2f, 0.2f, 0.9f)).GetComponent<RectTransform>();
        _autoButtonText = CreateText(_autoButtonRect, "Label", fontToUse, 12, TextAnchor.MiddleCenter, Color.white);
        StretchToFill(_autoButtonText.rectTransform, new Vector2(4f, 4f));
        _autoButtonRect.GetComponent<Button>().onClick.AddListener(() =>
        {
            _autoPlayEnabled = !_autoPlayEnabled;
            _player?.SetAutoPlay(_autoPlayEnabled);
        });
    }

    private void UpdateStartPreviewsFromUI()
    {
        if (_startPreviews == null || _startPreviews.Length != 3)
        {
            return;
        }

        Rect rectMage;
        Rect rectWarrior;
        Rect rectDemon;
        var mageRectSource = _startMagePreviewRect != null ? _startMagePreviewRect : _startMageRect;
        var warriorRectSource = _startWarriorPreviewRect != null ? _startWarriorPreviewRect : _startWarriorRect;
        var demonRectSource = _startDemonPreviewRect != null ? _startDemonPreviewRect : _startDemonRect;
        if (!TryGetGuiRect(mageRectSource, out rectMage) ||
            !TryGetGuiRect(warriorRectSource, out rectWarrior) ||
            !TryGetGuiRect(demonRectSource, out rectDemon))
        {
            return;
        }

        UpdateStartCharacterPreviews(rectMage, rectWarrior, rectDemon);
    }

    private bool TryGetGuiRect(RectTransform rectTransform, out Rect rect)
    {
        rect = default;
        if (rectTransform == null)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector3 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector3 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
        float width = tr.x - bl.x;
        float height = tr.y - bl.y;
        float guiY = Screen.height - (bl.y + height);
        rect = new Rect(bl.x, guiY, width, height);
        return true;
    }

    private void AddStartHoverTrigger(Button button, int index)
    {
        if (button == null)
        {
            return;
        }

        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => SetStartPreviewHover(index));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, _ => ClearStartPreviewHover(index));
    }

    private static void AddEventTrigger(EventTrigger trigger, EventTriggerType type, System.Action<BaseEventData> callback)
    {
        if (trigger == null)
        {
            return;
        }

        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(data => callback?.Invoke(data));
        trigger.triggers.Add(entry);
    }

    private static void ApplyButtonColors(Button button, Color normal, Color hover)
    {
        if (button == null)
        {
            return;
        }

        var colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = hover;
        colors.selectedColor = hover;
        colors.pressedColor = new Color(hover.r * 0.9f, hover.g * 0.9f, hover.b * 0.9f, hover.a);
        colors.disabledColor = new Color(normal.r, normal.g, normal.b, normal.a * 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.05f;
        button.colors = colors;

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = normal;
        }
    }

    private void SetStartPreviewHover(int index)
    {
        if (_startPreviewHoverIndex == index)
        {
            return;
        }

        _startPreviewHoverIndex = index;
        UpdateStartPreviewColors();
    }

    private void ClearStartPreviewHover(int index)
    {
        if (_startPreviewHoverIndex != index)
        {
            return;
        }

        _startPreviewHoverIndex = -1;
        UpdateStartPreviewColors();
    }

    private void UpdateStartPreviewColors()
    {
        if (_startPreviews == null || _startPreviews.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _startPreviews.Length; i++)
        {
            var preview = _startPreviews[i];
            if (preview == null)
            {
                continue;
            }

            float alpha = i == _startPreviewHoverIndex ? startPreviewHoverAlpha : startPreviewDimAlpha;
            SetPreviewAlpha(preview, alpha);
        }
    }

    private static void SetPreviewAlpha(GameObject preview, float alpha)
    {
        var renderer = preview.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Color color = renderer.color;
        color.a = Mathf.Clamp01(alpha);
        renderer.color = color;
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = bgColor;
        return rect;
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = bgColor;
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static Text CreateText(Transform parent, string name, Font font, int fontSize, TextAnchor alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void StretchToFill(RectTransform rectTransform, Vector2 padding)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(padding.x, padding.y);
        rectTransform.offsetMax = new Vector2(-padding.x, -padding.y);
    }

    private static void EnsureEventSystem()
    {
        var existing = FindObjectOfType<EventSystem>();
        if (existing == null)
        {
            var go = new GameObject("EventSystem");
            existing = go.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (existing.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            existing.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        var legacy = existing.GetComponent<StandaloneInputModule>();
        if (legacy != null)
        {
            Destroy(legacy);
        }
#else
        if (existing.GetComponent<StandaloneInputModule>() == null)
        {
            existing.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }


    private void OnGUI()
    {
        if (useUGUI)
        {
            return;
        }

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

        if (_showAutoButton)
        {
            DrawAutoPlayToggle();
        }
    }

    private void DrawGameOverPanel()
    {
        const float width = 360f;
        const float height = 180f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        string title = _stageCompleted ? "스테이지 완료" : "게임 오버";
        string timeLabel = _stageCompleted ? "클리어 시간" : "생존 시간";
        GUI.Box(new Rect(x, y, width, height), title);
        GUI.Label(new Rect(x + 20f, y + 40f, width - 40f, 24f), $"{timeLabel} {ElapsedTime:0.0}s");

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
            TryReroll();
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

        string label = _autoPlayEnabled ? "자동\n켜짐" : "자동\n꺼짐";
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
            SelectStartWeapon(StartWeaponType.Gun);
        }
        if (GUI.Button(rectWarrior, "전사\n기본 무기: 부메랑"))
        {
            SelectStartWeapon(StartWeaponType.Boomerang);
        }
        if (GUI.Button(rectDemon, "데몬로드\n기본 무기: 노바"))
        {
            SelectStartWeapon(StartWeaponType.Nova);
        }
    }

    private void SelectStartWeapon(StartWeaponType weapon)
    {
        startWeapon = weapon;
        if (gunStats != null)
        {
            gunStats.unlocked = weapon == StartWeaponType.Gun;
            gunStats.level = weapon == StartWeaponType.Gun ? 1 : 0;
        }

        if (boomerangStats != null)
        {
            boomerangStats.unlocked = weapon == StartWeaponType.Boomerang;
            boomerangStats.level = weapon == StartWeaponType.Boomerang ? 1 : 0;
        }

        if (novaStats != null)
        {
            novaStats.unlocked = weapon == StartWeaponType.Nova;
            novaStats.level = weapon == StartWeaponType.Nova ? 1 : 0;
        }

        ResetWeaponToLocked(shotgunStats);
        ResetWeaponToLocked(laserStats);
        ResetWeaponToLocked(chainStats);
        ResetWeaponToLocked(droneStats);
        ResetWeaponToLocked(shurikenStats);
        ResetWeaponToLocked(frostStats);
        ResetWeaponToLocked(lightningStats);

        if (weapon == StartWeaponType.Gun && gunStats != null)
        {
            TrackUpgrade($"무기: {gunStats.displayName}");
        }
        else if (weapon == StartWeaponType.Boomerang && boomerangStats != null)
        {
            TrackUpgrade($"무기: {boomerangStats.displayName}");
        }
        else if (weapon == StartWeaponType.Nova && novaStats != null)
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
        renderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(startPreviewDimAlpha));

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

        float topY = Screen.height - rect.y;
        float bottomY = Screen.height - (rect.y + rect.height);
        float worldTop = cam.ScreenToWorldPoint(new Vector3(0f, topY, depth)).y;
        float worldBottom = cam.ScreenToWorldPoint(new Vector3(0f, bottomY, depth)).y;
        float worldHeight = Mathf.Abs(worldTop - worldBottom);
        float scale = Mathf.Max(0.1f, worldHeight * startPreviewScale);
        preview.transform.localScale = new Vector3(scale, scale, 1f);
        world.y += worldHeight * startPreviewYOffset;
        preview.transform.position = world;
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

}


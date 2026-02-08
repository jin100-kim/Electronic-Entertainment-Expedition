using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Collections;
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

    [Header("Multiplayer Scaling")]
    private bool enableMultiplayerScaling = true;
    private float multiplayerMaxEnemiesPerPlayer = 0.6f;
    private float multiplayerSpawnIntervalReductionPerPlayer = 0.12f;
    private float multiplayerEnemyHealthPerPlayer = 0.7f;
    private float multiplayerEnemyDamagePerPlayer = 0.4f;
    private float multiplayerEnemyXpPerPlayer = 0.35f;
    private float multiplayerEliteHealthPerPlayer = 0.8f;
    private float multiplayerBossHealthPerPlayer = 1.2f;

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
    private float _baseEliteHealthMult;
    private float _baseBossHealthMult;
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
    private readonly System.Collections.Generic.List<string> _networkUpgradeTitles = new System.Collections.Generic.List<string>();
    private readonly System.Collections.Generic.List<string> _networkUpgradeDescs = new System.Collections.Generic.List<string>();
    private readonly System.Collections.Generic.Dictionary<ulong, int> _upgradeSelections = new System.Collections.Generic.Dictionary<ulong, int>();
    private readonly System.Collections.Generic.List<ulong> _upgradePendingClients = new System.Collections.Generic.List<ulong>();
    private readonly System.Collections.Generic.Dictionary<ulong, PlayerUpgradeState> _playerUpgradeStates = new System.Collections.Generic.Dictionary<ulong, PlayerUpgradeState>();
    private readonly System.Collections.Generic.Dictionary<ulong, List<UpgradeOption>> _upgradeOptionsByClient = new System.Collections.Generic.Dictionary<ulong, List<UpgradeOption>>();
    private readonly System.Collections.Generic.Dictionary<ulong, bool> _upgradeRerollAvailable = new System.Collections.Generic.Dictionary<ulong, bool>();
    private int _upgradeRoundId;
    private int _networkUpgradeRoundId = -1;
    private bool _localUpgradeSubmitted;
    private GameObject[] _startPreviews;
    private bool _startWeaponApplied;
    private bool _hasPendingStartWeapon;
    private StartWeaponType _pendingStartWeapon;
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
    private Text _startSubtitleText;
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
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
        }
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

        enableMultiplayerScaling = settings.enableMultiplayerScaling;
        multiplayerMaxEnemiesPerPlayer = settings.multiplayerMaxEnemiesPerPlayer;
        multiplayerSpawnIntervalReductionPerPlayer = settings.multiplayerSpawnIntervalReductionPerPlayer;
        multiplayerEnemyHealthPerPlayer = settings.multiplayerEnemyHealthPerPlayer;
        multiplayerEnemyDamagePerPlayer = settings.multiplayerEnemyDamagePerPlayer;
        multiplayerEnemyXpPerPlayer = settings.multiplayerEnemyXpPerPlayer;
        multiplayerEliteHealthPerPlayer = settings.multiplayerEliteHealthPerPlayer;
        multiplayerBossHealthPerPlayer = settings.multiplayerBossHealthPerPlayer;

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
        _startWeaponApplied = false;
        _hasPendingStartWeapon = false;
        _upgradeSelections.Clear();
        _upgradePendingClients.Clear();
        _upgradeOptionsByClient.Clear();
        _upgradeRerollAvailable.Clear();
        _playerUpgradeStates.Clear();
        _networkUpgradeTitles.Clear();
        _networkUpgradeDescs.Clear();
        _upgradeRoundId = 0;
        _networkUpgradeRoundId = -1;
        _localUpgradeSubmitted = false;
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

    private PlayerUpgradeState CreateBaseUpgradeState()
    {
        var state = new PlayerUpgradeState
        {
            damageMult = damageMult,
            fireRateMult = fireRateMult,
            rangeMult = rangeMult,
            sizeMult = sizeMult,
            lifetimeMult = lifetimeMult,
            projectileCount = projectileCount,
            projectilePierceBonus = projectilePierceBonus,
            weaponDamageMult = weaponDamageMult,
            moveSpeedMult = moveSpeedMult,
            xpGainMult = xpGainMult,
            magnetRangeMult = magnetRangeMult,
            magnetSpeedMult = magnetSpeedMult,
            magnetRangeStep = magnetRangeStep,
            magnetSpeedStep = magnetSpeedStep,
            regenPerSecond = regenPerSecond,
            gunStats = CloneWeaponStats(gunStats),
            boomerangStats = CloneWeaponStats(boomerangStats),
            novaStats = CloneWeaponStats(novaStats),
            shotgunStats = CloneWeaponStats(shotgunStats),
            laserStats = CloneWeaponStats(laserStats),
            chainStats = CloneWeaponStats(chainStats),
            droneStats = CloneWeaponStats(droneStats),
            shurikenStats = CloneWeaponStats(shurikenStats),
            frostStats = CloneWeaponStats(frostStats),
            lightningStats = CloneWeaponStats(lightningStats),
            startWeapon = startWeapon
        };

        if (requireStartWeaponChoice)
        {
            ResetWeaponToLocked(state.gunStats);
            ResetWeaponToLocked(state.boomerangStats);
            ResetWeaponToLocked(state.novaStats);
            ResetWeaponToLocked(state.shotgunStats);
            ResetWeaponToLocked(state.laserStats);
            ResetWeaponToLocked(state.chainStats);
            ResetWeaponToLocked(state.droneStats);
            ResetWeaponToLocked(state.shurikenStats);
            ResetWeaponToLocked(state.frostStats);
            ResetWeaponToLocked(state.lightningStats);
        }
        else
        {
            ApplyStartWeaponSelection(state, startWeapon, false);
        }

        return state;
    }

    private PlayerUpgradeState GetOrCreateState(PlayerController player)
    {
        if (player == null)
        {
            return null;
        }

        return GetOrCreateState(player.OwnerClientId);
    }

    private PlayerUpgradeState GetOrCreateState(ulong clientId)
    {
        if (_playerUpgradeStates.TryGetValue(clientId, out var state))
        {
            return state;
        }

        state = CreateBaseUpgradeState();
        _playerUpgradeStates[clientId] = state;
        return state;
    }

    private PlayerUpgradeState GetLocalState()
    {
        if (NetworkSession.IsActive)
        {
            var owner = FindOwnerPlayer();
            if (owner != null)
            {
                return GetOrCreateState(owner.OwnerClientId);
            }
            return null;
        }

        return GetOrCreateState(0);
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

    private static bool IsNetworkSession()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    private static bool HasUpgradeAuthority()
    {
        return !NetworkSession.IsActive || NetworkSession.IsServer;
    }

    private static PlayerController FindOwnerPlayer()
    {
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null && player.IsOwner)
            {
                return player;
            }
        }

        return null;
    }

    private static PlayerController FindServerPlayer()
    {
        ulong serverId = NetworkManager.ServerClientId;
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null && player.OwnerClientId == serverId)
            {
                return player;
            }
        }

        return null;
    }

    private int GetActivePlayerCount()
    {
        if (!NetworkSession.IsActive)
        {
            return 1;
        }

        int count = 0;
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                count++;
            }
        }

        if (count <= 0 && NetworkManager.Singleton != null)
        {
            count = NetworkManager.Singleton.ConnectedClients.Count;
        }

        return Mathf.Max(1, count);
    }

    private bool HasLocalStartWeaponSelection()
    {
        var owner = FindOwnerPlayer();
        return owner != null && owner.HasStartWeaponSelection;
    }

    private bool AreAllPlayersReady()
    {
        var players = PlayerController.Active;
        if (players.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                return false;
            }
            if (!player.HasStartWeaponSelection)
            {
                return false;
            }
        }

        return true;
    }

    private void GetPlayerReadyCounts(out int readyCount, out int totalCount)
    {
        readyCount = 0;
        totalCount = 0;

        var players = PlayerController.Active;
        totalCount = players.Count;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null && player.HasStartWeaponSelection)
            {
                readyCount += 1;
            }
        }
    }

    private string GetStartWaitingText()
    {
        if (!IsNetworkSession())
        {
            return "캐릭터 스탯은 현재 동일합니다.";
        }

        GetPlayerReadyCounts(out int ready, out int total);
        if (total <= 0)
        {
            return "대기중 (0/0)";
        }

        return $"대기중 ({ready}/{total})";
    }

    private void SubmitStartWeaponSelection(StartWeaponType weapon)
    {
        var owner = FindOwnerPlayer();
        if (owner == null)
        {
            _hasPendingStartWeapon = true;
            _pendingStartWeapon = weapon;
            return;
        }

        owner.SetStartWeaponSelection(weapon);
    }

    private void UpdatePendingStartWeaponSelection()
    {
        if (!_hasPendingStartWeapon)
        {
            return;
        }

        var owner = FindOwnerPlayer();
        if (owner == null)
        {
            return;
        }

        owner.SetStartWeaponSelection(_pendingStartWeapon);
        _hasPendingStartWeapon = false;
    }

    private void TryStartNetworkGameFromSelection()
    {
        if (_gameStarted || !IsNetworkSession())
        {
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (requireStartWeaponChoice && !AreAllPlayersReady())
            {
                return;
            }

            BeginNetworkGame();
            var serverPlayer = FindServerPlayer();
            if (serverPlayer != null)
            {
                serverPlayer.SetGameStartedSignal(true);
            }
            return;
        }

        var hostPlayer = FindServerPlayer();
        if (hostPlayer == null || !hostPlayer.GameStartedSignal)
        {
            return;
        }

        BeginNetworkGame();
    }

    private void BeginNetworkGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            EnsureNetworkPlayers();
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var players = PlayerController.Active;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || !player.HasStartWeaponSelection)
                {
                    continue;
                }

                var state = GetOrCreateState(player);
                ApplyStartWeaponSelection(state, player.StartWeaponSelection, !state.startWeaponApplied);
            }

            SyncAllUpgradeIconStatesToClients();
        }
        else
        {
            var owner = FindOwnerPlayer();
            if (owner != null && owner.HasStartWeaponSelection)
            {
                var state = GetOrCreateState(owner);
                ApplyStartWeaponSelection(state, owner.StartWeaponSelection, !state.startWeaponApplied);
            }
        }

        _waitingStartWeaponChoice = false;
        _gameStarted = true;
        StartCoroutine(WaitForOwnerPlayer());
    }

    private void EnsureNetworkPlayers()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        RuntimeNetworkPrefabs.EnsureRegistered();

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = kvp.Key;
            if (HasPlayerOwnedBy(clientId))
            {
                continue;
            }

            CreateNetworkPlayer(clientId, localSpawnPosition);
        }
    }

    private static bool HasPlayerOwnedBy(ulong clientId)
    {
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null && player.OwnerClientId == clientId)
            {
                return true;
            }
        }

        return false;
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
        if (IsGameOver)
        {
            return;
        }

        if (!_gameStarted)
        {
            UpdatePendingStartWeaponSelection();
            TryStartNetworkGameFromSelection();
            return;
        }

        if (_choosingUpgrade && _autoPlayEnabled && !_localUpgradeSubmitted)
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

        if (requireStartWeaponChoice && !_waitingStartWeaponChoice)
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
        _player.SetAutoPlay(_autoPlayEnabled);
        var state = GetOrCreateState(player);
        ApplyPlayerVisuals(player, state != null ? state.startWeapon : startWeapon);

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

        ApplyPlayerStatMultipliers(player, state);
        PlayerExperience.OnLevelUp += OnLevelUp;

        _attack = player.GetComponent<AutoAttack>();
        if (_attack == null)
        {
            _attack = player.gameObject.AddComponent<AutoAttack>();
        }

        ApplyAttackStats();
    }

    private void ApplyPlayerVisuals(PlayerController player, StartWeaponType weapon)
    {
        if (player == null)
        {
            return;
        }

        var visuals = player.GetComponent<PlayerVisuals>();
        if (visuals == null)
        {
            visuals = player.gameObject.AddComponent<PlayerVisuals>();
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
        GameObject go;
        if (NetworkSession.IsActive)
        {
            var playerPrefab = NetworkManager.Singleton != null ? NetworkManager.Singleton.NetworkConfig.PlayerPrefab : null;
            go = playerPrefab != null ? Instantiate(playerPrefab) : new GameObject("Player");
        }
        else
        {
            go = new GameObject("LocalPlayer");
        }

        go.name = "Player";
        go.transform.position = position;

        if (go.GetComponent<NetworkObject>() == null)
        {
            go.AddComponent<NetworkObject>();
        }

        if (NetworkSession.IsActive && go.GetComponent<Unity.Netcode.Components.NetworkTransform>() == null)
        {
            go.AddComponent<Unity.Netcode.Components.NetworkTransform>();
        }

        var controller = go.GetComponent<PlayerController>();
        if (controller == null)
        {
            controller = go.AddComponent<PlayerController>();
        }

        return controller;
    }

    private PlayerController CreateNetworkPlayer(ulong clientId, Vector3 position)
    {
        var controller = CreateLocalPlayer(position);
        var netObj = controller.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.SpawnAsPlayerObject(clientId, true);
        }
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
        if (NetworkSession.IsActive && NetworkSession.IsServer)
        {
            BeginUpgradeSelectionServer();
            ShowUpgradeChoices(false);
            NotifyUpgradeStartToClients();
            return;
        }

        ShowUpgradeChoices(true);
    }

    public void ShowUpgradeChoicesFromNetwork(string[] titles, string[] descs, int roundId, bool rerollAvailable)
    {
        if (HasUpgradeAuthority())
        {
            return;
        }

        bool newRound = roundId != _networkUpgradeRoundId;
        _networkUpgradeRoundId = roundId;
        ApplyNetworkUpgradeOptions(titles, descs);
        _rerollAvailable = rerollAvailable;
        if (newRound)
        {
            _localUpgradeSubmitted = false;
        }

        if (!_choosingUpgrade)
        {
            ShowUpgradeChoices(false);
        }
    }

    public void HideUpgradeChoicesFromNetwork(int roundId)
    {
        if (HasUpgradeAuthority())
        {
            return;
        }

        if (roundId != _networkUpgradeRoundId)
        {
            return;
        }

        EndUpgradeChoices();
        _networkUpgradeRoundId = -1;
        _networkUpgradeTitles.Clear();
        _networkUpgradeDescs.Clear();
    }

    private void ShowUpgradeChoices(bool buildOptions)
    {
        if (_choosingUpgrade)
        {
            return;
        }

        if (buildOptions)
        {
            var state = GetLocalState();
            var owner = _player != null ? _player : FindOwnerPlayer();
            if (state != null)
            {
                BuildUpgradeOptions(owner, state, _options);
            }
            _rerollAvailable = true;
        }

        _choosingUpgrade = true;
        Time.timeScale = 0f;
        _autoUpgradeStartTime = buildOptions && _autoPlayEnabled ? Time.unscaledTime : -1f;
        _localUpgradeSubmitted = false;
    }

    private void EndUpgradeChoices()
    {
        _choosingUpgrade = false;
        Time.timeScale = 1f;
        _autoUpgradeStartTime = -1f;
        _selectionLocked = false;
        _localUpgradeSubmitted = false;
    }

    private void NotifyUpgradeStartToClients()
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            SendUpgradeOptionsToClient(player.OwnerClientId);
        }
    }

    private void NotifyUpgradeEndToClients()
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            ulong clientId = player.OwnerClientId;
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };
            player.HideUpgradeUIClientRpc(_upgradeRoundId, rpcParams);
        }
    }

    private void SendUpgradeOptionsToClient(ulong clientId)
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        var player = FindPlayerByClientId(clientId);
        if (player == null)
        {
            return;
        }

        if (!_upgradeOptionsByClient.TryGetValue(clientId, out var options))
        {
            return;
        }

        BuildUpgradeOptionDisplayData(options, out string[] titles, out string[] descs);
        var titleData = ToFixedString128Array(titles);
        var descData = ToFixedString512Array(descs);
        bool rerollAvailable = _upgradeRerollAvailable.TryGetValue(clientId, out var available) && available;
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        player.ShowUpgradeUIClientRpc(titleData, descData, _upgradeRoundId, rerollAvailable, rpcParams);
    }

    private static PlayerController FindPlayerByClientId(ulong clientId)
    {
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null && player.OwnerClientId == clientId)
            {
                return player;
            }
        }

        return null;
    }

    private void BeginUpgradeSelectionServer()
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        _upgradeRoundId += 1;
        _upgradeSelections.Clear();
        _upgradePendingClients.Clear();
        _upgradeOptionsByClient.Clear();
        _upgradeRerollAvailable.Clear();

        ulong localClientId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : NetworkManager.ServerClientId;
        bool localOptionsSet = false;

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            ulong clientId = player.OwnerClientId;
            var state = GetOrCreateState(player);

            List<UpgradeOption> options;
            if (clientId == localClientId)
            {
                options = _options;
                localOptionsSet = true;
            }
            else
            {
                options = new List<UpgradeOption>(4);
            }

            BuildUpgradeOptions(player, state, options);
            _upgradeOptionsByClient[clientId] = options;
            _upgradeRerollAvailable[clientId] = true;

            if (!_upgradePendingClients.Contains(clientId))
            {
                _upgradePendingClients.Add(clientId);
            }
        }

        if (localOptionsSet)
        {
            _rerollAvailable = true;
        }
        else
        {
            _options.Clear();
            _rerollAvailable = false;
        }
    }

    public void ReceiveUpgradeSelectionServer(ulong clientId, int index, int roundId)
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        if (!_choosingUpgrade || roundId != _upgradeRoundId)
        {
            return;
        }

        if (!_upgradeOptionsByClient.TryGetValue(clientId, out var options))
        {
            return;
        }

        if (index < 0 || index >= options.Count)
        {
            return;
        }

        int pendingIndex = _upgradePendingClients.IndexOf(clientId);
        if (pendingIndex < 0)
        {
            return;
        }

        _upgradeSelections[clientId] = index;
        _upgradeRerollAvailable[clientId] = false;
        _upgradePendingClients.RemoveAt(pendingIndex);

        if (_upgradePendingClients.Count == 0)
        {
            ApplyUpgradeSelections();
        }
    }

    public void RequestUpgradeRerollServer(ulong clientId, int roundId)
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        if (!_choosingUpgrade || roundId != _upgradeRoundId)
        {
            return;
        }

        if (_upgradeSelections.ContainsKey(clientId))
        {
            return;
        }

        if (!_upgradeRerollAvailable.TryGetValue(clientId, out var available) || !available)
        {
            return;
        }

        var player = FindPlayerByClientId(clientId);
        if (player == null)
        {
            return;
        }

        var state = GetOrCreateState(player);
        if (!_upgradeOptionsByClient.TryGetValue(clientId, out var options))
        {
            options = new List<UpgradeOption>(4);
            _upgradeOptionsByClient[clientId] = options;
        }

        BuildUpgradeOptions(player, state, options);
        _upgradeRerollAvailable[clientId] = false;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == clientId)
        {
            _rerollAvailable = false;
            _autoUpgradeStartTime = _autoPlayEnabled ? Time.unscaledTime : -1f;
        }

        SendUpgradeOptionsToClient(clientId);
    }

    private void ApplyUpgradeSelections()
    {
        if (_upgradeOptionsByClient.Count == 0)
        {
            EndUpgradeChoices();
            NotifyUpgradeEndToClients();
            return;
        }

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            ulong clientId = player.OwnerClientId;
            if (!_upgradeSelections.TryGetValue(clientId, out int index))
            {
                continue;
            }

            if (!_upgradeOptionsByClient.TryGetValue(clientId, out var options))
            {
                continue;
            }

            if (index < 0 || index >= options.Count)
            {
                continue;
            }

            var opt = options[index];
            opt.Apply?.Invoke();
            TrackUpgrade(opt.Title);
        }

        ApplyUpgradeStats();
        SyncAllUpgradeIconStatesToClients();
        EndUpgradeChoices();
        NotifyUpgradeEndToClients();
        _upgradeSelections.Clear();
        _upgradePendingClients.Clear();
    }

    private void ApplyUpgradeEffect(int index)
    {
        if (index < 0 || index >= _options.Count)
        {
            return;
        }

        var opt = _options[index];
        opt.Apply?.Invoke();
        TrackUpgrade(opt.Title);
    }

    private void ApplyUpgradeStats()
    {
        if (NetworkSession.IsActive && NetworkSession.IsServer)
        {
            var players = PlayerController.Active;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                {
                    continue;
                }

                var state = GetOrCreateState(player);
                ApplyPlayerStatMultipliers(player, state);
            }
        }
        else
        {
            var state = GetLocalState();
            var owner = _player != null ? _player : FindOwnerPlayer();
            ApplyPlayerStatMultipliers(owner, state);
        }

        ApplyAttackStats();
    }

    private static void ApplyPlayerStatMultipliers(PlayerController player, PlayerUpgradeState state)
    {
        if (player == null || state == null)
        {
            return;
        }

        player.SetMoveSpeedMultiplier(state.moveSpeedMult);
        var xp = player.GetComponent<Experience>();
        if (xp != null)
        {
            xp.SetXpMultiplier(state.xpGainMult);
            xp.SetMagnetMultiplier(state.magnetRangeMult, state.magnetSpeedMult);
        }

        var health = player.GetComponent<Health>();
        health?.SetRegenPerSecond(state.regenPerSecond);
    }

    private void BuildUpgradeOptionDisplayData(List<UpgradeOption> options, out string[] titles, out string[] descs)
    {
        int count = options != null ? options.Count : 0;
        titles = new string[count];
        descs = new string[count];
        for (int i = 0; i < count; i++)
        {
            var opt = options[i];
            titles[i] = opt != null ? opt.Title : string.Empty;
            descs[i] = opt != null ? opt.Desc : string.Empty;
        }
    }

    private static FixedString128Bytes[] ToFixedString128Array(string[] values)
    {
        if (values == null)
        {
            return null;
        }

        var result = new FixedString128Bytes[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i] ?? string.Empty;
        }
        return result;
    }

    private static FixedString512Bytes[] ToFixedString512Array(string[] values)
    {
        if (values == null)
        {
            return null;
        }

        var result = new FixedString512Bytes[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i] ?? string.Empty;
        }
        return result;
    }

    private void ApplyNetworkUpgradeOptions(string[] titles, string[] descs)
    {
        _networkUpgradeTitles.Clear();
        _networkUpgradeDescs.Clear();

        if (titles == null)
        {
            return;
        }

        _networkUpgradeTitles.AddRange(titles);

        if (descs != null)
        {
            _networkUpgradeDescs.AddRange(descs);
        }
    }

    private void BuildUpgradeOptions(PlayerController player, PlayerUpgradeState state, List<UpgradeOption> options)
    {
        if (state == null || options == null)
        {
            return;
        }

        options.Clear();

        if (state.damageLevel < maxUpgradeLevel && CanOfferNewStat(state, state.damageLevel))
        {
            options.Add(new UpgradeOption("공격력 +10%", () => BuildPercentStatText("공격력", state.damageMult, state.damageMult + 0.10f), () => { state.damageMult += 0.10f; state.damageLevel += 1; }));
        }
        if (state.fireRateLevel < maxUpgradeLevel && CanOfferNewStat(state, state.fireRateLevel))
        {
            options.Add(new UpgradeOption("공격속도 +10%", () => BuildPercentStatText("공격속도", state.fireRateMult, state.fireRateMult + 0.10f), () => { state.fireRateMult += 0.10f; state.fireRateLevel += 1; }));
        }
        if (state.moveSpeedLevel < maxUpgradeLevel && CanOfferNewStat(state, state.moveSpeedLevel))
        {
            options.Add(new UpgradeOption("이동속도 +10%", () => BuildPercentStatText("이동속도", state.moveSpeedMult, state.moveSpeedMult + 0.10f), () => { state.moveSpeedMult += 0.10f; state.moveSpeedLevel += 1; }));
        }
        if (state.healthReinforceLevel < maxUpgradeLevel && CanOfferNewStat(state, state.healthReinforceLevel))
        {
            options.Add(new UpgradeOption("체력 강화", () => BuildHealthReinforceText(player, state), () =>
            {
                var health = player != null ? player.GetComponent<Health>() : null;
                if (health != null)
                {
                    health.AddMaxHealth(25f, true);
                    health.Heal(health.MaxHealth);
                }
                state.regenPerSecond += 0.5f;
                state.healthReinforceLevel += 1;
            }));
        }
        if (state.rangeLevel < maxUpgradeLevel && CanOfferNewStat(state, state.rangeLevel))
        {
            options.Add(new UpgradeOption("사거리 +15%", () => BuildPercentStatText("사거리", state.rangeMult, state.rangeMult + 0.15f), () => { state.rangeMult += 0.15f; state.rangeLevel += 1; }));
        }
        if (state.xpGainLevel < maxUpgradeLevel && CanOfferNewStat(state, state.xpGainLevel))
        {
            options.Add(new UpgradeOption("경험치 +10%", () => BuildPercentStatText("경험치 획득", state.xpGainMult, state.xpGainMult + 0.10f), () => { state.xpGainMult += 0.10f; state.xpGainLevel += 1; }));
        }
        if (state.magnetLevel < maxUpgradeLevel && CanOfferNewStat(state, state.magnetLevel))
        {
            options.Add(new UpgradeOption("경험치 자석", () => BuildMagnetUpgradeText(state), () =>
            {
                state.magnetRangeMult += state.magnetRangeStep;
                state.magnetSpeedMult += state.magnetSpeedStep;
                state.magnetLevel += 1;
            }));
        }
        if (state.sizeLevel < maxUpgradeLevel && CanOfferNewStat(state, state.sizeLevel))
        {
            options.Add(new UpgradeOption("투사체 크기 +25%", () => BuildPercentStatText("투사체 크기", state.sizeMult, state.sizeMult + 0.25f), () => { state.sizeMult += 0.25f; state.sizeLevel += 1; }));
        }
        if (state.projectileCountLevel < maxUpgradeLevel && CanOfferNewStat(state, state.projectileCountLevel))
        {
            options.Add(new UpgradeOption("투사체 수", () => BuildProjectileCountText(state), () =>
            {
                state.projectileCountLevel += 1;
                if (state.projectileCountLevel % 2 == 0)
                {
                    state.projectileCount += 1;
                }
            }));
        }
        if (state.pierceLevel < maxUpgradeLevel && CanOfferNewStat(state, state.pierceLevel))
        {
            options.Add(new UpgradeOption("관통 +1", () => BuildValueStatText("관통", state.projectilePierceBonus, state.projectilePierceBonus + 1), () => { state.projectilePierceBonus += 1; state.pierceLevel += 1; }));
        }

        AddWeaponChoice(options, state, state.gunStats, () => BuildStraightUpgradeText(state), () => UnlockStraight(state), () => LevelUpStraightWeapon(state));
        AddWeaponChoice(options, state, state.boomerangStats, () => BuildBoomerangUpgradeText(state), () => UnlockBoomerang(state), () => LevelUpBoomerangWeapon(state));
        AddWeaponChoice(options, state, state.novaStats, () => BuildNovaUpgradeText(state), () => UnlockNova(state), () => LevelUpNovaWeapon(state));
        AddWeaponChoice(options, state, state.shotgunStats, () => BuildShotgunUpgradeText(state), () => UnlockShotgun(state), () => LevelUpShotgunWeapon(state));
        AddWeaponChoice(options, state, state.laserStats, () => BuildLaserUpgradeText(state), () => UnlockLaser(state), () => LevelUpLaserWeapon(state));
        AddWeaponChoice(options, state, state.chainStats, () => BuildChainUpgradeText(state), () => UnlockChain(state), () => LevelUpChainWeapon(state));
        AddWeaponChoice(options, state, state.droneStats, () => BuildDroneUpgradeText(state), () => UnlockDrone(state), () => LevelUpDroneWeapon(state));
        AddWeaponChoice(options, state, state.shurikenStats, () => BuildShurikenUpgradeText(state), () => UnlockShuriken(state), () => LevelUpShurikenWeapon(state));
        AddWeaponChoice(options, state, state.frostStats, () => BuildFrostUpgradeText(state), () => UnlockFrost(state), () => LevelUpFrostWeapon(state));
        AddWeaponChoice(options, state, state.lightningStats, () => BuildLightningUpgradeText(state), () => UnlockLightning(state), () => LevelUpLightningWeapon(state));

        if (options.Count == 0)
        {
            options.Add(new UpgradeOption("HP 회복 (20%)", () => "최대 체력의 20%를 회복합니다.", () =>
            {
                var health = player != null ? player.GetComponent<Health>() : null;
                if (health != null)
                {
                    health.Heal(health.MaxHealth * 0.2f);
                }
            }));
            options.Add(new UpgradeOption("코인 +10", () => "즉시 코인 10개를 획득합니다.", () => AddCoins(10)));
        }

        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = options[i];
            options[i] = options[j];
            options[j] = temp;
        }

        if (options.Count > 4)
        {
            options.RemoveRange(4, options.Count - 4);
        }
    }

    private void ApplyUpgrade(int index)
    {
        if (NetworkSession.IsActive)
        {
            return;
        }

        if (!_choosingUpgrade || index < 0 || index >= _options.Count)
        {
            return;
        }

        ApplyUpgradeEffect(index);
        ApplyUpgradeStats();
        EndUpgradeChoices();
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

    public struct UpgradeIconState : INetworkSerializable
    {
        public int damageLevel;
        public int fireRateLevel;
        public int moveSpeedLevel;
        public int healthReinforceLevel;
        public int rangeLevel;
        public int xpGainLevel;
        public int sizeLevel;
        public int magnetLevel;
        public int pierceLevel;
        public int projectileCountLevel;

        public int gunLevel;
        public int boomerangLevel;
        public int novaLevel;
        public int shotgunLevel;
        public int laserLevel;
        public int chainLevel;
        public int droneLevel;
        public int shurikenLevel;
        public int frostLevel;
        public int lightningLevel;

        public bool gunUnlocked;
        public bool boomerangUnlocked;
        public bool novaUnlocked;
        public bool shotgunUnlocked;
        public bool laserUnlocked;
        public bool chainUnlocked;
        public bool droneUnlocked;
        public bool shurikenUnlocked;
        public bool frostUnlocked;
        public bool lightningUnlocked;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref damageLevel);
            serializer.SerializeValue(ref fireRateLevel);
            serializer.SerializeValue(ref moveSpeedLevel);
            serializer.SerializeValue(ref healthReinforceLevel);
            serializer.SerializeValue(ref rangeLevel);
            serializer.SerializeValue(ref xpGainLevel);
            serializer.SerializeValue(ref sizeLevel);
            serializer.SerializeValue(ref magnetLevel);
            serializer.SerializeValue(ref pierceLevel);
            serializer.SerializeValue(ref projectileCountLevel);

            serializer.SerializeValue(ref gunLevel);
            serializer.SerializeValue(ref boomerangLevel);
            serializer.SerializeValue(ref novaLevel);
            serializer.SerializeValue(ref shotgunLevel);
            serializer.SerializeValue(ref laserLevel);
            serializer.SerializeValue(ref chainLevel);
            serializer.SerializeValue(ref droneLevel);
            serializer.SerializeValue(ref shurikenLevel);
            serializer.SerializeValue(ref frostLevel);
            serializer.SerializeValue(ref lightningLevel);

            serializer.SerializeValue(ref gunUnlocked);
            serializer.SerializeValue(ref boomerangUnlocked);
            serializer.SerializeValue(ref novaUnlocked);
            serializer.SerializeValue(ref shotgunUnlocked);
            serializer.SerializeValue(ref laserUnlocked);
            serializer.SerializeValue(ref chainUnlocked);
            serializer.SerializeValue(ref droneUnlocked);
            serializer.SerializeValue(ref shurikenUnlocked);
            serializer.SerializeValue(ref frostUnlocked);
            serializer.SerializeValue(ref lightningUnlocked);
        }
    }

    public void GetUpgradeIconData(List<UpgradeIconData> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();
        var state = GetLocalState();
        if (state == null)
        {
            return;
        }

        AddWeaponIcon(results, state.gunStats);
        AddWeaponIcon(results, state.boomerangStats);
        AddWeaponIcon(results, state.novaStats);
        AddWeaponIcon(results, state.shotgunStats);
        AddWeaponIcon(results, state.laserStats);
        AddWeaponIcon(results, state.chainStats);
        AddWeaponIcon(results, state.droneStats);
        AddWeaponIcon(results, state.shurikenStats);
        AddWeaponIcon(results, state.frostStats);
        AddWeaponIcon(results, state.lightningStats);

        AddStatIcon(results, "공격력", state.damageLevel);
        AddStatIcon(results, "공격속도", state.fireRateLevel);
        AddStatIcon(results, "이동속도", state.moveSpeedLevel);
        AddStatIcon(results, "체력강화", state.healthReinforceLevel);
        AddStatIcon(results, "사거리", state.rangeLevel);
        AddStatIcon(results, "경험치", state.xpGainLevel);
        AddStatIcon(results, "자석", state.magnetLevel);
        AddStatIcon(results, "투사체크기", state.sizeLevel);
        AddStatIcon(results, "투사체수", state.projectileCountLevel);
        AddStatIcon(results, "관통", state.pierceLevel);
    }

    public void ApplyUpgradeIconState(UpgradeIconState data)
    {
        var state = GetLocalState();
        if (state == null)
        {
            return;
        }

        state.damageLevel = data.damageLevel;
        state.fireRateLevel = data.fireRateLevel;
        state.moveSpeedLevel = data.moveSpeedLevel;
        state.healthReinforceLevel = data.healthReinforceLevel;
        state.rangeLevel = data.rangeLevel;
        state.xpGainLevel = data.xpGainLevel;
        state.sizeLevel = data.sizeLevel;
        state.magnetLevel = data.magnetLevel;
        state.pierceLevel = data.pierceLevel;
        state.projectileCountLevel = data.projectileCountLevel;

        ApplyWeaponIconState(state.gunStats, data.gunLevel, data.gunUnlocked);
        ApplyWeaponIconState(state.boomerangStats, data.boomerangLevel, data.boomerangUnlocked);
        ApplyWeaponIconState(state.novaStats, data.novaLevel, data.novaUnlocked);
        ApplyWeaponIconState(state.shotgunStats, data.shotgunLevel, data.shotgunUnlocked);
        ApplyWeaponIconState(state.laserStats, data.laserLevel, data.laserUnlocked);
        ApplyWeaponIconState(state.chainStats, data.chainLevel, data.chainUnlocked);
        ApplyWeaponIconState(state.droneStats, data.droneLevel, data.droneUnlocked);
        ApplyWeaponIconState(state.shurikenStats, data.shurikenLevel, data.shurikenUnlocked);
        ApplyWeaponIconState(state.frostStats, data.frostLevel, data.frostUnlocked);
        ApplyWeaponIconState(state.lightningStats, data.lightningLevel, data.lightningUnlocked);
    }

    private static void ApplyWeaponIconState(WeaponStatsData stats, int level, bool unlocked)
    {
        if (stats == null)
        {
            return;
        }

        stats.level = level;
        stats.unlocked = unlocked || level > 0;
    }

    private static UpgradeIconState BuildUpgradeIconState(PlayerUpgradeState state)
    {
        return new UpgradeIconState
        {
            damageLevel = state.damageLevel,
            fireRateLevel = state.fireRateLevel,
            moveSpeedLevel = state.moveSpeedLevel,
            healthReinforceLevel = state.healthReinforceLevel,
            rangeLevel = state.rangeLevel,
            xpGainLevel = state.xpGainLevel,
            sizeLevel = state.sizeLevel,
            magnetLevel = state.magnetLevel,
            pierceLevel = state.pierceLevel,
            projectileCountLevel = state.projectileCountLevel,
            gunLevel = state.gunStats != null ? state.gunStats.level : 0,
            boomerangLevel = state.boomerangStats != null ? state.boomerangStats.level : 0,
            novaLevel = state.novaStats != null ? state.novaStats.level : 0,
            shotgunLevel = state.shotgunStats != null ? state.shotgunStats.level : 0,
            laserLevel = state.laserStats != null ? state.laserStats.level : 0,
            chainLevel = state.chainStats != null ? state.chainStats.level : 0,
            droneLevel = state.droneStats != null ? state.droneStats.level : 0,
            shurikenLevel = state.shurikenStats != null ? state.shurikenStats.level : 0,
            frostLevel = state.frostStats != null ? state.frostStats.level : 0,
            lightningLevel = state.lightningStats != null ? state.lightningStats.level : 0,
            gunUnlocked = state.gunStats != null && state.gunStats.unlocked,
            boomerangUnlocked = state.boomerangStats != null && state.boomerangStats.unlocked,
            novaUnlocked = state.novaStats != null && state.novaStats.unlocked,
            shotgunUnlocked = state.shotgunStats != null && state.shotgunStats.unlocked,
            laserUnlocked = state.laserStats != null && state.laserStats.unlocked,
            chainUnlocked = state.chainStats != null && state.chainStats.unlocked,
            droneUnlocked = state.droneStats != null && state.droneStats.unlocked,
            shurikenUnlocked = state.shurikenStats != null && state.shurikenStats.unlocked,
            frostUnlocked = state.frostStats != null && state.frostStats.unlocked,
            lightningUnlocked = state.lightningStats != null && state.lightningStats.unlocked
        };
    }

    private void SyncUpgradeIconStateToClient(ulong clientId, PlayerUpgradeState state)
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer || state == null)
        {
            return;
        }

        var player = FindPlayerByClientId(clientId);
        if (player == null)
        {
            return;
        }

        var data = BuildUpgradeIconState(state);
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        player.SyncUpgradeIconStateClientRpc(data, rpcParams);
    }

    private void SyncAllUpgradeIconStatesToClients()
    {
        if (!NetworkSession.IsActive || !NetworkSession.IsServer)
        {
            return;
        }

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            var state = GetOrCreateState(player);
            SyncUpgradeIconStateToClient(player.OwnerClientId, state);
        }
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

        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        _coinCount += amount;
        PlayerPrefs.SetInt(CoinPrefKey, _coinCount);
        PlayerPrefs.Save();
    }

    public void AddSharedXp(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        var list = Experience.Active;
        Experience source = PlayerExperience;
        if (source == null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var xp = list[i];
                if (xp != null)
                {
                    source = xp;
                    break;
                }
            }
        }

        if (source == null)
        {
            return;
        }

        source.AddXp(amount);
        int level = source.Level;
        float current = source.CurrentXp;
        float next = source.XpToNext;

        for (int i = 0; i < list.Count; i++)
        {
            var xp = list[i];
            if (xp == null || xp == source)
            {
                continue;
            }

            xp.SetSharedState(level, current, next);
        }

        if (NetworkSession.IsActive)
        {
            SyncSharedXpToClients(level, current, next);
        }
    }

    private void SyncSharedXpToClients(int level, float currentXp, float xpToNext)
    {
        if (!NetworkSession.IsServer)
        {
            return;
        }

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player != null)
            {
                player.SyncSharedXpClientRpc(level, currentXp, xpToNext);
            }
        }
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

    private int GetUpgradeOptionCount()
    {
        if (HasUpgradeAuthority())
        {
            return _options != null ? _options.Count : 0;
        }

        return _networkUpgradeTitles.Count;
    }

    private bool TryGetUpgradeOptionText(int index, out string title, out string desc)
    {
        title = string.Empty;
        desc = string.Empty;

        if (HasUpgradeAuthority())
        {
            if (_options == null || index < 0 || index >= _options.Count)
            {
                return false;
            }

            var opt = _options[index];
            if (opt == null)
            {
                return false;
            }

            title = opt.Title;
            desc = opt.Desc;
            return true;
        }

        if (index < 0 || index >= _networkUpgradeTitles.Count)
        {
            return false;
        }

        title = _networkUpgradeTitles[index];
        if (index < _networkUpgradeDescs.Count)
        {
            desc = _networkUpgradeDescs[index];
        }

        return true;
    }

    private string GetUpgradeOptionTitle(int index)
    {
        if (HasUpgradeAuthority())
        {
            if (_options == null || index < 0 || index >= _options.Count)
            {
                return string.Empty;
            }

            return _options[index] != null ? _options[index].Title : string.Empty;
        }

        if (index < 0 || index >= _networkUpgradeTitles.Count)
        {
            return string.Empty;
        }

        return _networkUpgradeTitles[index];
    }

    private int PickAutoUpgradeIndex()
    {
        int optionCount = GetUpgradeOptionCount();
        if (optionCount <= 0)
        {
            return -1;
        }

        float totalWeight = 0f;
        var weights = new float[optionCount];
        for (int i = 0; i < optionCount; i++)
        {
            int score = ScoreUpgradeTitle(GetUpgradeOptionTitle(i));
            float weight = 1f + Mathf.Max(0, score);
            weights[i] = weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0.0001f)
        {
            return Random.Range(0, optionCount);
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

        return optionCount - 1;
    }

    private int ScoreUpgradeOption(UpgradeOption option)
    {
        if (option == null)
        {
            return 0;
        }

        return ScoreUpgradeTitle(option.Title);
    }

    private int ScoreUpgradeTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return 0;
        }

        if (title.Contains("무기:"))
        {
            return 1;
        }

        return 0;
    }

    private void AddWeaponChoice(List<UpgradeOption> options, PlayerUpgradeState state, WeaponStatsData stats, System.Func<string> upgradeText, System.Action unlockAction, System.Action levelUpAction)
    {
        if (options == null || state == null || stats == null)
        {
            return;
        }
        if (stats.level >= maxUpgradeLevel)
        {
            return;
        }
        if (!stats.unlocked && !CanOfferNewWeapon(state, stats))
        {
            return;
        }

        options.Add(new UpgradeOption(
            $"무기: {stats.displayName}",
            () => stats.unlocked && stats.level > 0 ? upgradeText() : BuildWeaponAcquireText(state, stats),
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

    private bool CanOfferNewWeapon(PlayerUpgradeState state, WeaponStatsData stats)
    {
        if (state == null || stats == null)
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

        return GetUnlockedWeaponCount(state) < maxWeaponSlots;
    }

    private bool CanOfferNewStat(PlayerUpgradeState state, int currentLevel)
    {
        if (currentLevel > 0)
        {
            return true;
        }
        if (maxStatSlots <= 0)
        {
            return true;
        }

        return GetUnlockedStatCount(state) < maxStatSlots;
    }

    private int GetUnlockedWeaponCount(PlayerUpgradeState state)
    {
        if (state == null)
        {
            return 0;
        }

        int count = 0;
        if (state.gunStats != null && state.gunStats.unlocked && state.gunStats.level > 0) count++;
        if (state.boomerangStats != null && state.boomerangStats.unlocked && state.boomerangStats.level > 0) count++;
        if (state.novaStats != null && state.novaStats.unlocked && state.novaStats.level > 0) count++;
        if (state.shotgunStats != null && state.shotgunStats.unlocked && state.shotgunStats.level > 0) count++;
        if (state.laserStats != null && state.laserStats.unlocked && state.laserStats.level > 0) count++;
        if (state.chainStats != null && state.chainStats.unlocked && state.chainStats.level > 0) count++;
        if (state.droneStats != null && state.droneStats.unlocked && state.droneStats.level > 0) count++;
        if (state.shurikenStats != null && state.shurikenStats.unlocked && state.shurikenStats.level > 0) count++;
        if (state.frostStats != null && state.frostStats.unlocked && state.frostStats.level > 0) count++;
        if (state.lightningStats != null && state.lightningStats.unlocked && state.lightningStats.level > 0) count++;
        return count;
    }

    private int GetUnlockedStatCount(PlayerUpgradeState state)
    {
        if (state == null)
        {
            return 0;
        }

        int count = 0;
        if (state.damageLevel > 0) count++;
        if (state.fireRateLevel > 0) count++;
        if (state.moveSpeedLevel > 0) count++;
        if (state.healthReinforceLevel > 0) count++;
        if (state.rangeLevel > 0) count++;
        if (state.xpGainLevel > 0) count++;
        if (state.sizeLevel > 0) count++;
        if (state.magnetLevel > 0) count++;
        if (state.projectileCountLevel > 0) count++;
        if (state.pierceLevel > 0) count++;
        return count;
    }

    private void ApplyAttackStats()
    {
        if (NetworkSession.IsActive && NetworkSession.IsServer)
        {
            var players = PlayerController.Active;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                {
                    continue;
                }

                var state = GetOrCreateState(player);
                ApplyAttackStats(player, state);
            }
            return;
        }

        var localState = GetLocalState();
        if (localState == null)
        {
            return;
        }

        var owner = _player != null ? _player : FindOwnerPlayer();
        ApplyAttackStats(owner, localState);
    }

    private void ApplyAttackStats(PlayerController player, PlayerUpgradeState state)
    {
        if (player == null || state == null)
        {
            return;
        }

        var attack = player.GetComponent<AutoAttack>();
        if (attack == null)
        {
            attack = player.gameObject.AddComponent<AutoAttack>();
        }

        attack.ApplyStats(state.damageMult, state.fireRateMult, state.rangeMult, state.sizeMult, state.lifetimeMult, state.projectileCount, state.projectilePierceBonus, state.weaponDamageMult);
        attack.SetWeaponStats(AutoAttack.WeaponType.Straight, state.gunStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Boomerang, state.boomerangStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Nova, state.novaStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Shotgun, state.shotgunStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Laser, state.laserStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.ChainLightning, state.chainStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Drone, state.droneStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Shuriken, state.shurikenStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.FrostOrb, state.frostStats);
        attack.SetWeaponStats(AutoAttack.WeaponType.Lightning, state.lightningStats);
    }

    private void ApplyDifficultyScaling()
    {
        if (_spawner == null)
        {
            return;
        }

        CacheSpawnerBaseStats();

        int playerCount = GetActivePlayerCount();
        int extraPlayers = Mathf.Max(0, playerCount - 1);
        float playerCountFactor = enableMultiplayerScaling ? 1f + multiplayerMaxEnemiesPerPlayer * extraPlayers : 1f;
        float spawnIntervalFactor = enableMultiplayerScaling
            ? Mathf.Max(0.4f, 1f - multiplayerSpawnIntervalReductionPerPlayer * extraPlayers)
            : 1f;
        float healthFactor = enableMultiplayerScaling ? 1f + multiplayerEnemyHealthPerPlayer * extraPlayers : 1f;
        float damageFactor = enableMultiplayerScaling ? 1f + multiplayerEnemyDamagePerPlayer * extraPlayers : 1f;
        float xpFactor = enableMultiplayerScaling ? 1f + multiplayerEnemyXpPerPlayer * extraPlayers : 1f;
        float eliteHealthFactor = enableMultiplayerScaling ? 1f + multiplayerEliteHealthPerPlayer * extraPlayers : 1f;
        float bossHealthFactor = enableMultiplayerScaling ? 1f + multiplayerBossHealthPerPlayer * extraPlayers : 1f;

        float newInterval = Mathf.Max(minSpawnInterval, spawnInterval * spawnIntervalFactor - ElapsedTime * spawnIntervalDecayPerSec);
        _spawner.SpawnInterval = newInterval;

        int extra = Mathf.FloorToInt((ElapsedTime / 60f) * maxEnemiesPerMinute * playerCountFactor);
        _spawner.MaxEnemies = Mathf.RoundToInt(maxEnemies * playerCountFactor) + extra;

        int level = MonsterLevel;
        float levelFactor = Mathf.Max(0f, level - 1f);
        _spawner.EnemyMoveSpeed = _baseEnemyMoveSpeed * (1f + enemySpeedPerLevel * levelFactor);
        _spawner.EnemyDamage = _baseEnemyDamage * damageFactor * (1f + enemyDamagePerLevel * levelFactor);
        _spawner.EnemyMaxHealth = _baseEnemyMaxHealth * healthFactor * (1f + enemyHealthPerLevel * levelFactor);
        _spawner.EnemyXpReward = Mathf.Max(1, Mathf.RoundToInt(_baseEnemyXp * xpFactor * (1f + enemyXpPerLevel * levelFactor)));
        _spawner.EliteHealthMult = _baseEliteHealthMult * eliteHealthFactor;
        _spawner.BossHealthMult = _baseBossHealthMult * bossHealthFactor;
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
        _baseEliteHealthMult = _spawner.EliteHealthMult;
        _baseBossHealthMult = _spawner.BossHealthMult;
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

    private void LevelUpStraightWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.gunStats == null)
        {
            return;
        }
        if (state.gunStats.level >= maxUpgradeLevel)
        {
            return;
        }

        state.gunStats.level += 1;
        state.gunStats.damageMult += 0.20f;
        state.gunStats.fireRateMult += 0.08f;

        if (state.gunStats.level % 3 == 0)
        {
            state.gunStats.bonusProjectiles += 1;
        }

        if (state.gunStats.level % 4 == 0)
        {
            state.projectilePierceBonus += 1;
        }
    }

    private void LevelUpBoomerangWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.boomerangStats == null)
        {
            return;
        }
        if (state.boomerangStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.boomerangStats.unlocked)
        {
            UnlockBoomerang(state);
        }

        state.boomerangStats.level += 1;
        state.boomerangStats.damageMult += 0.20f;
        state.boomerangStats.fireRateMult += 0.10f;

        if (state.boomerangStats.level % 4 == 0)
        {
            state.boomerangStats.bonusProjectiles += 1;
            state.projectilePierceBonus += 1;
        }
    }

    private void LevelUpNovaWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.novaStats == null)
        {
            return;
        }
        if (state.novaStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.novaStats.unlocked)
        {
            UnlockNova(state);
        }

        state.novaStats.level += 1;
        state.novaStats.damageMult += 0.20f;
        state.novaStats.fireRateMult += 0.12f;

        if (state.novaStats.level % 3 == 0)
        {
            state.novaStats.bonusProjectiles += 2;
        }
    }

    private void LevelUpShotgunWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.shotgunStats == null)
        {
            return;
        }
        if (state.shotgunStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.shotgunStats.unlocked)
        {
            UnlockShotgun(state);
        }

        state.shotgunStats.level += 1;
        state.shotgunStats.damageMult += 0.18f;
        state.shotgunStats.fireRateMult += 0.05f;

        if (state.shotgunStats.level % 2 == 0)
        {
            state.shotgunStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpLaserWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.laserStats == null)
        {
            return;
        }
        if (state.laserStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.laserStats.unlocked)
        {
            UnlockLaser(state);
        }

        state.laserStats.level += 1;
        state.laserStats.damageMult += 0.20f;
        state.laserStats.fireRateMult += 0.04f;

        if (state.laserStats.level % 3 == 0)
        {
            state.laserStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpChainWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.chainStats == null)
        {
            return;
        }
        if (state.chainStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.chainStats.unlocked)
        {
            UnlockChain(state);
        }

        state.chainStats.level += 1;
        state.chainStats.damageMult += 0.18f;
        state.chainStats.fireRateMult += 0.05f;

        if (state.chainStats.level % 2 == 0)
        {
            state.chainStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpDroneWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.droneStats == null)
        {
            return;
        }
        if (state.droneStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.droneStats.unlocked)
        {
            UnlockDrone(state);
        }

        state.droneStats.level += 1;
        state.droneStats.damageMult += 0.15f;
        state.droneStats.fireRateMult += 0.04f;

        if (state.droneStats.level % 3 == 0)
        {
            state.droneStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpShurikenWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.shurikenStats == null)
        {
            return;
        }
        if (state.shurikenStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.shurikenStats.unlocked)
        {
            UnlockShuriken(state);
        }

        state.shurikenStats.level += 1;
        state.shurikenStats.damageMult += 0.18f;
        state.shurikenStats.fireRateMult += 0.06f;

        if (state.shurikenStats.level % 3 == 0)
        {
            state.shurikenStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpFrostWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.frostStats == null)
        {
            return;
        }
        if (state.frostStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.frostStats.unlocked)
        {
            UnlockFrost(state);
        }

        state.frostStats.level += 1;
        state.frostStats.damageMult += 0.16f;
        state.frostStats.fireRateMult += 0.05f;

        if (state.frostStats.level % 3 == 0)
        {
            state.frostStats.bonusProjectiles += 1;
        }
    }

    private void LevelUpLightningWeapon(PlayerUpgradeState state)
    {
        if (state == null || state.lightningStats == null)
        {
            return;
        }
        if (state.lightningStats.level >= maxUpgradeLevel)
        {
            return;
        }

        if (!state.lightningStats.unlocked)
        {
            UnlockLightning(state);
        }

        state.lightningStats.level += 1;
        state.lightningStats.damageMult += 0.20f;
        state.lightningStats.fireRateMult += 0.06f;

        if (state.lightningStats.level % 2 == 0)
        {
            state.lightningStats.bonusProjectiles += 1;
        }
    }

    private void UnlockBoomerang(PlayerUpgradeState state)
    {
        if (state == null || state.boomerangStats == null)
        {
            return;
        }

        state.boomerangStats.unlocked = true;
        if (state.boomerangStats.level < 1)
        {
            state.boomerangStats.level = 1;
        }
    }

    private void UnlockNova(PlayerUpgradeState state)
    {
        if (state == null || state.novaStats == null)
        {
            return;
        }

        state.novaStats.unlocked = true;
        if (state.novaStats.level < 1)
        {
            state.novaStats.level = 1;
        }
    }

    private void UnlockShotgun(PlayerUpgradeState state)
    {
        if (state == null || state.shotgunStats == null)
        {
            return;
        }

        state.shotgunStats.unlocked = true;
        if (state.shotgunStats.level < 1)
        {
            state.shotgunStats.level = 1;
        }
    }

    private void UnlockLaser(PlayerUpgradeState state)
    {
        if (state == null || state.laserStats == null)
        {
            return;
        }

        state.laserStats.unlocked = true;
        if (state.laserStats.level < 1)
        {
            state.laserStats.level = 1;
        }
    }

    private void UnlockChain(PlayerUpgradeState state)
    {
        if (state == null || state.chainStats == null)
        {
            return;
        }

        state.chainStats.unlocked = true;
        if (state.chainStats.level < 1)
        {
            state.chainStats.level = 1;
        }
    }

    private void UnlockDrone(PlayerUpgradeState state)
    {
        if (state == null || state.droneStats == null)
        {
            return;
        }

        state.droneStats.unlocked = true;
        if (state.droneStats.level < 1)
        {
            state.droneStats.level = 1;
        }
    }

    private void UnlockShuriken(PlayerUpgradeState state)
    {
        if (state == null || state.shurikenStats == null)
        {
            return;
        }

        state.shurikenStats.unlocked = true;
        if (state.shurikenStats.level < 1)
        {
            state.shurikenStats.level = 1;
        }
    }

    private void UnlockFrost(PlayerUpgradeState state)
    {
        if (state == null || state.frostStats == null)
        {
            return;
        }

        state.frostStats.unlocked = true;
        if (state.frostStats.level < 1)
        {
            state.frostStats.level = 1;
        }
    }

    private void UnlockLightning(PlayerUpgradeState state)
    {
        if (state == null || state.lightningStats == null)
        {
            return;
        }

        state.lightningStats.unlocked = true;
        if (state.lightningStats.level < 1)
        {
            state.lightningStats.level = 1;
        }
    }

    private void UnlockStraight(PlayerUpgradeState state)
    {
        if (state == null || state.gunStats == null)
        {
            return;
        }

        state.gunStats.unlocked = true;
        if (state.gunStats.level < 1)
        {
            state.gunStats.level = 1;
        }
    }

    private string BuildWeaponAcquireText(PlayerUpgradeState state, WeaponStatsData stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        int currentLevel = stats.level;
        int nextLevel = Mathf.Max(1, currentLevel + 1);
        float dmg = stats.damageMult;
        float rate = stats.fireRateMult;
        int baseProjectiles = GetBaseProjectileCount(state, stats);
        return $"{stats.displayName}\n레벨 {currentLevel} -> {nextLevel}\n피해량 {dmg:0.##} -> {dmg:0.##}\n속도 {rate:0.##} -> {rate:0.##}\n투사체 {baseProjectiles} -> {baseProjectiles}\n관통 0 -> 0";
    }

    private string BuildStraightUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.gunStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.gunStats.level + 1;
        float nextDamage = state.gunStats.damageMult + 0.20f;
        int currentProjectile = 1 + state.gunStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        int nextPierce = state.projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = state.gunStats.fireRateMult + 0.08f;
        return BuildWeaponUpgradeText(state.gunStats.displayName, state.gunStats.level, nextLevel, state.gunStats.damageMult, nextDamage, state.gunStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, nextPierce);
    }

    private string BuildBoomerangUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.boomerangStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.boomerangStats.level + 1;
        float nextDamage = state.boomerangStats.damageMult + 0.20f;
        int currentProjectile = 1 + state.boomerangStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 4 == 0 ? 1 : 0);
        int nextPierce = state.projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = state.boomerangStats.fireRateMult + 0.10f;
        return BuildWeaponUpgradeText(state.boomerangStats.displayName, state.boomerangStats.level, nextLevel, state.boomerangStats.damageMult, nextDamage, state.boomerangStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, nextPierce);
    }

    private string BuildNovaUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.novaStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.novaStats.level + 1;
        float nextDamage = state.novaStats.damageMult + 0.20f;
        int currentCount = 8 + state.novaStats.bonusProjectiles;
        int nextCount = currentCount + (nextLevel % 3 == 0 ? 2 : 0);
        float nextRate = state.novaStats.fireRateMult + 0.12f;
        return BuildWeaponUpgradeText(state.novaStats.displayName, state.novaStats.level, nextLevel, state.novaStats.damageMult, nextDamage, state.novaStats.fireRateMult, nextRate, currentCount, nextCount, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildShotgunUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.shotgunStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.shotgunStats.level + 1;
        float nextDamage = state.shotgunStats.damageMult + 0.18f;
        int currentProjectile = 5 + state.shotgunStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = state.shotgunStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(state.shotgunStats.displayName, state.shotgunStats.level, nextLevel, state.shotgunStats.damageMult, nextDamage, state.shotgunStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildLaserUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.laserStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.laserStats.level + 1;
        float nextDamage = state.laserStats.damageMult + 0.20f;
        int currentProjectile = 1 + state.laserStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = state.laserStats.fireRateMult + 0.04f;
        return BuildWeaponUpgradeText(state.laserStats.displayName, state.laserStats.level, nextLevel, state.laserStats.damageMult, nextDamage, state.laserStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildChainUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.chainStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.chainStats.level + 1;
        float nextDamage = state.chainStats.damageMult + 0.18f;
        int currentProjectile = 3 + state.chainStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = state.chainStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(state.chainStats.displayName, state.chainStats.level, nextLevel, state.chainStats.damageMult, nextDamage, state.chainStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildDroneUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.droneStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.droneStats.level + 1;
        float nextDamage = state.droneStats.damageMult + 0.15f;
        int currentProjectile = 1 + state.droneStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = state.droneStats.fireRateMult + 0.04f;
        return BuildWeaponUpgradeText(state.droneStats.displayName, state.droneStats.level, nextLevel, state.droneStats.damageMult, nextDamage, state.droneStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildShurikenUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.shurikenStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.shurikenStats.level + 1;
        float nextDamage = state.shurikenStats.damageMult + 0.18f;
        int currentProjectile = 1 + state.shurikenStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = state.shurikenStats.fireRateMult + 0.06f;
        return BuildWeaponUpgradeText(state.shurikenStats.displayName, state.shurikenStats.level, nextLevel, state.shurikenStats.damageMult, nextDamage, state.shurikenStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildFrostUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.frostStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.frostStats.level + 1;
        float nextDamage = state.frostStats.damageMult + 0.16f;
        int currentProjectile = 1 + state.frostStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        float nextRate = state.frostStats.fireRateMult + 0.05f;
        return BuildWeaponUpgradeText(state.frostStats.displayName, state.frostStats.level, nextLevel, state.frostStats.damageMult, nextDamage, state.frostStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private string BuildLightningUpgradeText(PlayerUpgradeState state)
    {
        if (state == null || state.lightningStats == null)
        {
            return string.Empty;
        }

        int nextLevel = state.lightningStats.level + 1;
        float nextDamage = state.lightningStats.damageMult + 0.20f;
        int currentProjectile = 1 + state.lightningStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 2 == 0 ? 1 : 0);
        float nextRate = state.lightningStats.fireRateMult + 0.06f;
        return BuildWeaponUpgradeText(state.lightningStats.displayName, state.lightningStats.level, nextLevel, state.lightningStats.damageMult, nextDamage, state.lightningStats.fireRateMult, nextRate, currentProjectile, nextProjectile, state.projectilePierceBonus, state.projectilePierceBonus);
    }

    private int GetBaseProjectileCount(PlayerUpgradeState state, WeaponStatsData stats)
    {
        if (stats == null)
        {
            return 1;
        }

        if (state != null && stats == state.novaStats)
        {
            return 8;
        }
        if (state != null && stats == state.shotgunStats)
        {
            return 5;
        }
        if (state != null && stats == state.chainStats)
        {
            return 3;
        }
        if (state != null && stats == state.droneStats)
        {
            return 1;
        }
        if (state != null && stats == state.shurikenStats)
        {
            return 1;
        }
        if (state != null && stats == state.frostStats)
        {
            return 1;
        }
        if (state != null && stats == state.lightningStats)
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

    private string BuildHealthReinforceText(PlayerController player, PlayerUpgradeState state)
    {
        float currentMax = 0f;
        var health = player != null ? player.GetComponent<Health>() : null;
        if (health != null)
        {
            currentMax = health.MaxHealth;
        }
        float nextMax = currentMax + 25f;
        float currentRegen = state != null ? state.regenPerSecond : 0f;
        float nextRegen = currentRegen + 0.5f;
        var lines = new List<string>
        {
            BuildValueStatText("최대 체력", currentMax, nextMax),
            BuildValueStatText("체력 재생", currentRegen, nextRegen),
            "획득 시 체력 100% 회복"
        };
        return string.Join("\n", lines);
    }

    private string BuildMagnetUpgradeText(PlayerUpgradeState state)
    {
        float currentRange = state != null ? state.magnetRangeMult : 0f;
        float currentSpeed = state != null ? state.magnetSpeedMult : 0f;
        float nextRange = currentRange + (state != null ? state.magnetRangeStep : 0f);
        float nextSpeed = currentSpeed + (state != null ? state.magnetSpeedStep : 0f);
        var lines = new List<string>
        {
            BuildValueStatText("자석 범위", currentRange, nextRange),
            BuildValueStatText("자석 속도", currentSpeed, nextSpeed)
        };
        return string.Join("\n", lines);
    }

    private string BuildProjectileCountText(PlayerUpgradeState state)
    {
        int currentCount = state != null ? state.projectileCount : 0;
        int currentLevel = state != null ? state.projectileCountLevel : 0;
        int nextCount = currentCount + ((currentLevel + 1) % 2 == 0 ? 1 : 0);
        var lines = new List<string>
        {
            BuildValueStatText("투사체 수", currentCount, nextCount),
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
        if (_startSubtitleText != null)
        {
            _startSubtitleText.gameObject.SetActive(showStart);
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

        if (showStart && _startSubtitleText != null)
        {
            _startSubtitleText.text = GetStartWaitingText();
        }

        if (showUpgrade && _upgradeTitleText != null)
        {
            if (HasUpgradeAuthority())
            {
                _upgradeTitleText.text = NetworkSession.IsActive && _localUpgradeSubmitted ? "선택 완료 (대기)" : "레벨업 선택";
            }
            else
            {
                _upgradeTitleText.text = _localUpgradeSubmitted ? "선택 완료 (대기)" : "레벨업 선택";
            }
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

        int optionCount = Mathf.Min(4, GetUpgradeOptionCount());
        for (int i = 0; i < _upgradeButtons.Length; i++)
        {
            bool active = i < optionCount;
            _upgradeButtons[i].gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            if (TryGetUpgradeOptionText(i, out string title, out string desc))
            {
                _upgradeButtonTexts[i].text = $"{i + 1}. {title}\n{desc}";
            }
            else
            {
                _upgradeButtonTexts[i].text = $"{i + 1}.";
            }

            bool canInteract = !NetworkSession.IsActive || !_localUpgradeSubmitted;
            _upgradeButtons[i].interactable = canInteract;
        }

        if (_rerollButton != null)
        {
            _rerollButton.gameObject.SetActive(true);
            bool rerollAllowed = _rerollAvailable && !_localUpgradeSubmitted;
            _rerollButton.interactable = rerollAllowed;
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
        if (_localUpgradeSubmitted && NetworkSession.IsActive)
        {
            return;
        }

        if (_selectionLocked)
        {
            return;
        }

        int optionCount = GetUpgradeOptionCount();
        if (index < 0 || index >= optionCount)
        {
            return;
        }

        Button button = null;
        if (_upgradeButtons != null && index >= 0 && index < _upgradeButtons.Length)
        {
            button = _upgradeButtons[index];
        }

        BeginSelectionFeedback(button, upgradeButtonClickColor, () => SubmitUpgradeSelection(index));
    }

    private void SubmitUpgradeSelection(int index)
    {
        if (!_choosingUpgrade)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            if (_localUpgradeSubmitted)
            {
                return;
            }

            if (NetworkSession.IsServer)
            {
                _localUpgradeSubmitted = true;
                ulong clientId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : NetworkManager.ServerClientId;
                ReceiveUpgradeSelectionServer(clientId, index, _upgradeRoundId);
            }
            else
            {
                if (_networkUpgradeRoundId < 0)
                {
                    return;
                }

                var owner = _player != null ? _player : FindOwnerPlayer();
                if (owner != null)
                {
                    _localUpgradeSubmitted = true;
                    owner.SubmitUpgradeSelectionServerRpc(index, _networkUpgradeRoundId);
                }
            }

            return;
        }

        ApplyUpgrade(index);
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
        if (!_choosingUpgrade)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            if (_localUpgradeSubmitted)
            {
                return;
            }

            if (NetworkSession.IsServer)
            {
                ulong clientId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : NetworkManager.ServerClientId;
                RequestUpgradeRerollServer(clientId, _upgradeRoundId);
                return;
            }

            if (_networkUpgradeRoundId < 0)
            {
                return;
            }

            var owner = _player != null ? _player : FindOwnerPlayer();
            if (owner != null)
            {
                owner.RequestUpgradeRerollServerRpc(_networkUpgradeRoundId);
            }

            return;
        }

        if (!_rerollAvailable)
        {
            return;
        }

        var state = GetLocalState();
        var localPlayer = _player != null ? _player : FindOwnerPlayer();
        if (state != null)
        {
            BuildUpgradeOptions(localPlayer, state, _options);
        }

        _rerollAvailable = false;
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
        _startSubtitleText = subtitle;
        var subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -10f);
        subtitleRect.sizeDelta = new Vector2(320f, 20f);
        subtitle.text = GetStartWaitingText();

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
        mageButton.onClick.AddListener(() => SelectStartWeaponWithFeedback(StartWeaponType.Gun, mageButton));
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
        warriorButton.onClick.AddListener(() => SelectStartWeaponWithFeedback(StartWeaponType.Boomerang, warriorButton));
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
        demonButton.onClick.AddListener(() => SelectStartWeaponWithFeedback(StartWeaponType.Nova, demonButton));
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

        int optionCount = Mathf.Min(4, GetUpgradeOptionCount());
        if (optionCount <= 0)
        {
            const float width = 360f;
            const float height = 140f;
            float wx = (Screen.width - width) * 0.5f;
            float wy = (Screen.height - height) * 0.5f;

            GUI.Box(new Rect(wx, wy, width, height), "레벨업 선택");
            GUI.Label(new Rect(wx + 20f, wy + 50f, width - 40f, 24f), "옵션 수신 중...");
            return;
        }

        float maxWidth = Screen.width - 40f;
        float boxWidth = Mathf.Floor((maxWidth - sidePadding * 2f - (columns - 1) * gap) / columns);
        float w = columns * boxWidth + (columns - 1) * gap + sidePadding * 2f;
        float h = topPadding + boxHeight + sidePadding;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        string title = HasUpgradeAuthority()
            ? (NetworkSession.IsActive && _localUpgradeSubmitted ? "선택 완료 (대기)" : "레벨업 선택")
            : (_localUpgradeSubmitted ? "선택 완료 (대기)" : "레벨업 선택");
        GUI.Box(new Rect(x, y, w, h), title);

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        style.fontSize = 13;

        bool canSelect = !NetworkSession.IsActive || !_localUpgradeSubmitted;
        GUI.enabled = canSelect;
        for (int i = 0; i < optionCount; i++)
        {
            float bx = x + sidePadding + i * (boxWidth + gap);
            float by = y + topPadding;

            if (TryGetUpgradeOptionText(i, out string optTitle, out string optDesc))
            {
                if (GUI.Button(new Rect(bx, by, boxWidth, boxHeight), $"{i + 1}. {optTitle}\n{optDesc}", style))
                {
                    SelectUpgradeWithFeedback(i);
                }
            }
        }
        GUI.enabled = true;

        float rx = x + sidePadding + 4 * (boxWidth + gap);
        float ry = y + topPadding;
        var rerollStyle = new GUIStyle(GUI.skin.button);
        rerollStyle.alignment = TextAnchor.MiddleCenter;
        rerollStyle.wordWrap = true;
        rerollStyle.fontSize = 13;

        bool rerollAllowed = _rerollAvailable && !_localUpgradeSubmitted;
        GUI.enabled = rerollAllowed;
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

    private class PlayerUpgradeState
    {
        public float damageMult;
        public float fireRateMult;
        public float rangeMult;
        public float sizeMult;
        public float lifetimeMult;
        public int projectileCount;
        public int projectilePierceBonus;
        public float weaponDamageMult;

        public float moveSpeedMult;
        public float xpGainMult;
        public float magnetRangeMult;
        public float magnetSpeedMult;
        public float magnetRangeStep;
        public float magnetSpeedStep;
        public float regenPerSecond;

        public int damageLevel;
        public int fireRateLevel;
        public int moveSpeedLevel;
        public int healthReinforceLevel;
        public int rangeLevel;
        public int xpGainLevel;
        public int sizeLevel;
        public int magnetLevel;
        public int pierceLevel;
        public int projectileCountLevel;

        public WeaponStatsData gunStats;
        public WeaponStatsData boomerangStats;
        public WeaponStatsData novaStats;
        public WeaponStatsData shotgunStats;
        public WeaponStatsData laserStats;
        public WeaponStatsData chainStats;
        public WeaponStatsData droneStats;
        public WeaponStatsData shurikenStats;
        public WeaponStatsData frostStats;
        public WeaponStatsData lightningStats;

        public StartWeaponType startWeapon;
        public bool startWeaponApplied;
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
        GUI.Label(new Rect(x + 20f, y + 36f, boxWidth - 40f, 20f), GetStartWaitingText());

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
        var state = GetLocalState();
        ApplyStartWeaponSelection(state, weapon, true);

        if (IsNetworkSession())
        {
            SubmitStartWeaponSelection(weapon);
            return;
        }

        _waitingStartWeaponChoice = false;
        ClearStartCharacterPreviews();
        StartLocalGame();
    }

    private void ApplyStartWeaponSelection(PlayerUpgradeState state, StartWeaponType weapon, bool trackUpgrade)
    {
        if (state == null)
        {
            return;
        }

        state.startWeapon = weapon;
        if (state.gunStats != null)
        {
            state.gunStats.unlocked = weapon == StartWeaponType.Gun;
            state.gunStats.level = weapon == StartWeaponType.Gun ? 1 : 0;
        }

        if (state.boomerangStats != null)
        {
            state.boomerangStats.unlocked = weapon == StartWeaponType.Boomerang;
            state.boomerangStats.level = weapon == StartWeaponType.Boomerang ? 1 : 0;
        }

        if (state.novaStats != null)
        {
            state.novaStats.unlocked = weapon == StartWeaponType.Nova;
            state.novaStats.level = weapon == StartWeaponType.Nova ? 1 : 0;
        }

        ResetWeaponToLocked(state.shotgunStats);
        ResetWeaponToLocked(state.laserStats);
        ResetWeaponToLocked(state.chainStats);
        ResetWeaponToLocked(state.droneStats);
        ResetWeaponToLocked(state.shurikenStats);
        ResetWeaponToLocked(state.frostStats);
        ResetWeaponToLocked(state.lightningStats);

        if (trackUpgrade && !state.startWeaponApplied)
        {
            if (weapon == StartWeaponType.Gun && state.gunStats != null)
            {
                TrackUpgrade($"무기: {state.gunStats.displayName}");
            }
            else if (weapon == StartWeaponType.Boomerang && state.boomerangStats != null)
            {
                TrackUpgrade($"무기: {state.boomerangStats.displayName}");
            }
            else if (weapon == StartWeaponType.Nova && state.novaStats != null)
            {
                TrackUpgrade($"무기: {state.novaStats.displayName}");
            }
        }

        state.startWeaponApplied = true;
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


using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    public GameSessionSettings game = new GameSessionSettings();
    public StageConfig defaultStage;
    public DifficultyConfig defaultDifficulty;
    public HudConfig hud = new HudConfig();
    public MinimapConfig minimap = new MinimapConfig();
    public EnemySpawnerConfig enemySpawner = new EnemySpawnerConfig();
    public AutoAttackConfig autoAttack = new AutoAttackConfig();
    public ElementSystemConfig elementSystem = new ElementSystemConfig();
    public PlayerConfig player = new PlayerConfig();
    public ExperienceConfig experience = new ExperienceConfig();
    public PickupConfig pickups = new PickupConfig();
    public MapBackgroundConfig mapBackground = new MapBackgroundConfig();
    public MapBorderConfig mapBorder = new MapBorderConfig();
    public NetworkUiConfig networkUi = new NetworkUiConfig();
    public WindowConfig window = new WindowConfig();

    public const string DefaultResourcePath = "GameConfig";
    private static GameConfig _cached;

    public static GameConfig LoadDefault()
    {
        if (_cached == null)
        {
            _cached = Resources.Load<GameConfig>(DefaultResourcePath);
        }
        return _cached;
    }

    public static GameConfig LoadOrCreate()
    {
        return LoadDefault() ?? ScriptableObject.CreateInstance<GameConfig>();
    }
}

[System.Serializable]
public class GameSessionSettings
{
    [Header("Start Mode")]
    public bool autoStartLocal = false;
    public bool showNetworkUI = true;

    [Header("Spawn")]
    public float spawnInterval = 2f;
    public int maxEnemies = 20;
    public float spawnRadius = 8f;

    [Header("Difficulty")]
    public float minSpawnInterval = 0.4f;
    public float spawnIntervalDecayPerSec = 0.01f;
    public int maxEnemiesPerMinute = 10;
    public float monsterLevelInterval = 60f;
    public float enemyHealthPerLevel = 0.15f;
    public float enemyDamagePerLevel = 0.10f;
    public float enemySpeedPerLevel = 0.05f;
    public float enemyXpPerLevel = 0f;

    [Header("Multiplayer Scaling")]
    public bool enableMultiplayerScaling = true;
    public float multiplayerMaxEnemiesPerPlayer = 0.6f;
    public float multiplayerSpawnIntervalReductionPerPlayer = 0.12f;
    public float multiplayerEnemyHealthPerPlayer = 0.7f;
    public float multiplayerEnemyDamagePerPlayer = 0.4f;
    public float multiplayerEnemyXpPerPlayer = 0.35f;
    public float multiplayerEliteHealthPerPlayer = 0.8f;
    public float multiplayerBossHealthPerPlayer = 1.2f;

    [Header("Enrage")]
    public bool enableEnrage = true;
    public float enrageStartTime = 600f;
    public float enrageHealthPerSecond = 0.004f;
    public float enrageDamagePerSecond = 0.003f;
    public float enrageSpeedPerSecond = 0.0015f;
    public float enrageSpawnIntervalReductionPerSecond = 0.005f;
    public float enrageMinSpawnInterval = 0.25f;
    public float enrageExtraEnemiesPerSecond = 0.05f;

    [Header("Player")]
    public Vector3 localSpawnPosition = Vector3.zero;

    [Header("Map Bounds")]
    public Vector2 mapHalfSize = new Vector2(24f, 24f);

    [Header("Upgrades")]
    public int maxUpgradeLevel = 10;
    public int maxWeaponSlots = 5;
    public int maxStatSlots = 5;

    [Header("Base Multipliers")]
    public float damageMult = 1f;
    public float fireRateMult = 1f;
    public float rangeMult = 1f;
    public float sizeMult = 1f;
    public float lifetimeMult = 1f;
    public int projectileCount = 1;
    public int projectilePierceBonus = 0;
    public float weaponDamageMult = 1f;

    [Header("Weapon Stats")]
    public WeaponStatsData gunStats = new WeaponStatsData
    {
        displayName = "총",
        level = 1,
        unlocked = true,
        damageMult = 1f,
        fireRateMult = 1.2f,
        rangeMult = 1f,
        bonusProjectiles = 0
    };

    public WeaponStatsData boomerangStats = new WeaponStatsData
    {
        displayName = "부메랑",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.8f,
        rangeMult = 0.7f,
        bonusProjectiles = 0
    };

    public WeaponStatsData novaStats = new WeaponStatsData
    {
        displayName = "노바",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.6f,
        rangeMult = 0.5f,
        bonusProjectiles = 0
    };

    public WeaponStatsData shotgunStats = new WeaponStatsData
    {
        displayName = "샷건",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.7f,
        rangeMult = 0.75f,
        bonusProjectiles = 0
    };

    public WeaponStatsData laserStats = new WeaponStatsData
    {
        displayName = "레이저",
        level = 0,
        unlocked = false,
        damageMult = 1.1f,
        fireRateMult = 0.8f,
        rangeMult = 1.4f,
        bonusProjectiles = 0
    };

    public WeaponStatsData chainStats = new WeaponStatsData
    {
        displayName = "체인 라이트닝",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.75f,
        rangeMult = 1.1f,
        bonusProjectiles = 0
    };

    public WeaponStatsData droneStats = new WeaponStatsData
    {
        displayName = "드론",
        level = 0,
        unlocked = false,
        damageMult = 0.8f,
        fireRateMult = 0.5f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    public WeaponStatsData shurikenStats = new WeaponStatsData
    {
        displayName = "수리검",
        level = 0,
        unlocked = false,
        damageMult = 0.9f,
        fireRateMult = 0.9f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    public WeaponStatsData frostStats = new WeaponStatsData
    {
        displayName = "빙결 구체",
        level = 0,
        unlocked = false,
        damageMult = 0.85f,
        fireRateMult = 0.8f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    public WeaponStatsData lightningStats = new WeaponStatsData
    {
        displayName = "번개",
        level = 0,
        unlocked = false,
        damageMult = 1.0f,
        fireRateMult = 0.7f,
        rangeMult = 1.0f,
        bonusProjectiles = 0
    };

    [Header("Stat Multipliers")]
    public float moveSpeedMult = 1f;
    public float xpGainMult = 1f;
    public float magnetRangeMult = 1f;
    public float magnetSpeedMult = 1f;
    public float magnetRangeStep = 1f;
    public float magnetSpeedStep = 1f;
    public float regenPerSecond = 0f;

    [Header("Drops")]
    public float coinDropChance = 0.06f;
    public int coinAmount = 1;

    [Header("Start Weapon")]
    public bool requireStartWeaponChoice = true;
    public StartWeaponType startWeapon = StartWeaponType.Gun;

    [Header("Start Character Preview")]
    public float startPreviewScale = 2f;
    public float startPreviewDimAlpha = 0.5f;
    public float startPreviewHoverAlpha = 1f;
    public int startPreviewSortingOrder = 5000;
    public float startPreviewYOffset = -0.5f;

    [Header("UI")]
    public bool useUGUI = true;
    public Vector2 uiReferenceResolution = new Vector2(1280f, 720f);
    public Font uiFont;
    public Color startButtonNormalColor = new Color(0f, 0f, 0f, 0.25f);
    public Color startButtonHoverColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
    public Color upgradeButtonNormalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color upgradeButtonHoverColor = new Color(0.35f, 0.35f, 0.35f, 0.95f);
    public Color startButtonClickColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color upgradeButtonClickColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public float selectionClickDuration = 0.3f;
    public float selectionClickScale = 0.96f;
    public Color selectionOutlineColor = new Color(1f, 1f, 1f, 0.9f);
    public float selectionOutlineSize = 2f;
    public float selectionFlashStrength = 1f;

    [Header("Developer")]
    public bool allowAutoButtonSecret = true;
    public string autoButtonSecret = "auto";
    public float autoButtonSecretTimeout = 1.5f;
}

[System.Serializable]
public class HudConfig
{
    public Vector2 referenceResolution = new Vector2(1280f, 720f);
    public Font uiFont;
    public float margin = 12f;
    public float xpBarHeight = 18f;
    public float iconSize = 56f;
    public float iconGap = 6f;
    public float iconStartOffsetX = 20f;
    public int labelFontSize = 24;
    public int smallFontSize = 20;
    public int iconFontSize = 11;
    public int iconLevelFontSize = 12;
}

[System.Serializable]
public class MinimapConfig
{
    public bool useUGUI = true;
    public Vector2 size = new Vector2(180f, 180f);
    public Vector2 margin = new Vector2(12f, 12f);
    public Vector2 referenceResolution = new Vector2(1280f, 720f);
    public Font uiFont;
    public bool showCameraRect = true;
    public Color cameraRectColor = new Color(1f, 1f, 1f, 0.6f);
    public float cameraRectThickness = 1f;
    public float playerDotSize = 6f;
    public float enemyDotSize = 2f;
    public float weaponDotSize = 2f;
    public float borderThickness = 2f;
    public Color borderColor = new Color(1f, 1f, 1f, 0.5f);
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);
    public int labelFontSize = 12;
    public string labelText = "미니맵";
}

[System.Serializable]
public class EnemySpawnerConfig
{
    public float spawnInterval = 2f;
    public int maxEnemies = 20;
    public float spawnRadius = 8f;
    public float enemyMoveSpeed = 1.6667f;
    public float enemyDamage = 10f;
    public float enemyDamageCooldown = 0.5f;
    public int enemyXpReward = 2;
    public float enemyMaxHealth = 40f;
    public float enemyVisualScale = 4f;

    public float eliteStartTime = 120f;
    public float eliteInterval = 20f;
    [Range(0f, 1f)] public float eliteChance = 0.6f;
    public int maxEliteAlive = 3;
    public float bossStartTime = 600f;
    public float bossInterval = 90f;
    public int maxBossAlive = 1;

    public float eliteHealthMult = 1.6f;
    public float eliteDamageMult = 1.4f;
    public float eliteSpeedMult = 1.15f;
    public float eliteXpMult = 3f;

    public float bossHealthMult = 4f;
    public float bossDamageMult = 2f;
    public float bossSpeedMult = 1.2f;
    public float bossXpMult = 8f;

    [Header("Visual Variety")]
    public bool enableVisualPhases = true;
    public float phase1EndTime = 180f;
    public float phase2EndTime = 360f;
    public float phase3EndTime = 600f;
    public Vector3 phase1Weights = new Vector3(1f, 0f, 0f);
    public Vector3 phase2Weights = new Vector3(0.7f, 0.3f, 0f);
    public Vector3 phase3Weights = new Vector3(0.4f, 0.4f, 0.2f);
    public Vector3 phase4Weights = new Vector3(0.25f, 0.35f, 0.4f);
}

[System.Serializable]
public class AutoAttackConfig
{
    public float baseFireInterval = 0.6f;
    public float baseProjectileSpeed = 10f;
    public float baseProjectileDamage = 10f;
    public float baseRange = 6f;
    public int baseProjectileSize = 50;
    public float baseProjectileLifetime = 2f;
    public int baseProjectilePierce = 0;
    public float straightParallelSpacing = 0.35f;
    public float novaOrbitAngularSpeed = 8f;

    public int shotgunBasePellets = 5;
    public float shotgunSpreadAngle = 32f;
    public float shotgunPelletDamageMult = 0.75f;
    public float shotgunSpeedMult = 0.95f;

    public float laserSpeedMult = 1.8f;
    public float laserThickness = 0.12f;
    public float laserLengthScale = 1.4f;
    public float laserParallelSpacing = 0.3f;
    public Color laserColor = new Color(1f, 0.3f, 0.3f, 1f);

    public int chainBaseJumps = 3;
    public float chainJumpRangeMult = 0.7f;
    public float chainLineWidth = 0.12f;
    public float chainEffectDuration = 0.12f;
    public Color chainColor = new Color(0.5f, 0.8f, 1f, 1f);

    public float droneOrbitRadius = 1.8f;
    public float droneAngularSpeed = 3.2f;
    public float droneLifetime = 6f;
    public float droneDamageMult = 0.6f;
    public Color droneColor = new Color(0.9f, 0.9f, 1f, 1f);

    public float shurikenSpeedMult = 1.4f;
    public float shurikenSpinSpeed = 1080f;
    public float shurikenDamageMult = 0.85f;
    public Color shurikenColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    public float frostSpeedMult = 0.8f;
    public float frostDamageMult = 0.8f;
    public float frostSlowMultiplier = 0.6f;
    public float frostSlowDuration = 1.5f;
    public Color frostColor = new Color(0.6f, 0.85f, 1f, 1f);

    public float lightningEffectDuration = 0.12f;
    public float lightningLineWidth = 0.14f;
    public float lightningLineLength = 1.6f;
    public Color lightningColor = new Color(1f, 0.95f, 0.5f, 1f);

    [Header("Sprites (Resources)")]
    public string straightSpritePath;
    public string boomerangSpritePath;
    public string novaSpritePath;
    public string shotgunSpritePath;
    public string droneSpritePath;
    public string shurikenSpritePath;
    public string frostSpritePath;
    public float projectileSpriteScale = 2.5f;
}

[System.Serializable]
public class PlayerConfig
{
    public float moveSpeed = 5f;
    public Color playerColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    public Color[] playerPalette = new[]
    {
        new Color(0.2f, 0.9f, 0.3f, 1f),
        new Color(1f, 0.85f, 0.2f, 1f),
        new Color(0.3f, 0.7f, 1f, 1f),
        new Color(1f, 0.4f, 0.6f, 1f)
    };
    public Vector3 shadowOffset = new Vector3(0f, -0.35f, 0f);
    public Vector3 shadowScale = new Vector3(0.6f, 0.25f, 1f);
    public float shadowAlpha = 0.6f;
    public bool allowOfflineControl = true;
    public bool autoPlayEnabled = false;
    public float visualScale = 2.5f;

    public bool autoSeekXp = true;
    public bool autoXpPriority = false;
    public float autoXpSeekRange = 5f;
    public float autoMinDistance = 2.5f;
    public float autoMaxDistance = 4.0f;
    public float autoOrbitStrength = 0.8f;
    public float autoKeepDistanceStrength = 0.6f;
    public float autoCenterPull = 0.9f;
    public float autoSmooth = 10f;
    public float autoMidCenterPull = 0.1f;
}

[System.Serializable]
public class ExperienceConfig
{
    public float initialXpToNext = 8f;
    public float xpGrowth = 4f;
    public float xpMultiplier = 1.5f;
    public float baseMagnetRange = 1.5f;
    public float baseMagnetSpeed = 1.5f;
}

[System.Serializable]
public class PickupConfig
{
    public float xpPickupScale = 0.4f;
    public float coinPickupScale = 0.4f;
    public float xpMagnetScanInterval = 0.2f;
    public float coinMagnetScanInterval = 0.2f;
    public int xpSpriteSize = 50;
    public int coinSpriteSize = 40;
    public string xpSpritePath;
    public string coinSpritePath;
    public Color xpColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color coinColor = new Color(1f, 0.85f, 0.2f, 1f);
    public float xpColliderRadius = 0.15f;
    public float coinColliderRadius = 0.12f;
    public int coinSortingOrder = 1;
}

[System.Serializable]
public class MapBackgroundConfig
{
    public string backgroundSpritePath;
    public float tileScale = 1f;
    public Color tint = Color.white;
    public int sortingOrder = -10;
    public bool useGrid = true;
    public int gridCellSize = 32;
    public int gridLineThickness = 1;
    public Color gridLineColor = new Color(0.3f, 0.35f, 0.45f, 0.35f);
    public Color gridBackgroundColor = new Color(0.16f, 0.18f, 0.22f, 1f);
    public bool useChecker = false;
    public int checkerCellSize = 32;
    public Color checkerColorA = new Color(0.6f, 0.85f, 0.55f, 1f);
    public Color checkerColorB = Color.white;
}

[System.Serializable]
public class MapBorderConfig
{
    public Color color = new Color(1f, 1f, 1f, 0.6f);
    public float width = 0.05f;
    public int sortingOrder = 100;
}

[System.Serializable]
public class NetworkUiConfig
{
    public string hostButtonText = "Start Host";
    public string clientButtonText = "Start Client";
    public string localButtonText = "Start Local";
    public Vector2 buttonSize = new Vector2(200f, 50f);
    public float buttonSpacing = 10f;
    public string address = "127.0.0.1";
    public ushort port = 7777;
    public bool useImGuiFallback = false;
    public Vector3 localSpawnPosition = Vector3.zero;
}

[System.Serializable]
public class WindowConfig
{
    public int width = 1280;
    public int height = 720;
    public FullScreenMode fullscreenMode = FullScreenMode.Windowed;
}

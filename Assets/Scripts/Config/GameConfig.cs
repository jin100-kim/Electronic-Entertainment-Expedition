using UnityEngine;
using UnityEngine.Serialization;

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
    public int maxWeaponSlots = 3;
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
    [FormerlySerializedAs("gunStats")]
    public WeaponStatsData singleShotStats = new WeaponStatsData
    {
        displayName = "SingleShot",
        level = 1,
        unlocked = true,
        damageMult = 1f,
        fireRateMult = 1.2f,
        rangeMult = 1.3333f,
        areaMult = 0f,
        bonusProjectiles = 0,
        hitStunDuration = 0.05f,
        knockbackDistance = 0.05f
    };

    [FormerlySerializedAs("boomerangStats")]
    public WeaponStatsData multiShotStats = new WeaponStatsData
    {
        displayName = "MultiShot",
        level = 0,
        unlocked = false,
        damageMult = 0.5f,
        fireRateMult = 0.6f,
        rangeMult = 0.8333f,
        areaMult = 0f,
        bonusProjectiles = 2,
        hitStunDuration = 0.05f,
        knockbackDistance = 0.05f
    };

    [FormerlySerializedAs("novaStats")]
    public WeaponStatsData piercingShotStats = new WeaponStatsData
    {
        displayName = "PiercingShot",
        level = 0,
        unlocked = false,
        damageMult = 2.5f,
        fireRateMult = 0.48f,
        rangeMult = 3.3333f,
        areaMult = 0f,
        bonusProjectiles = 0,
        hitStunDuration = 0.2f,
        knockbackDistance = 0.2f
    };

    [FormerlySerializedAs("shotgunStats")]
    public WeaponStatsData auraStats = new WeaponStatsData
    {
        displayName = "Aura",
        level = 0,
        unlocked = false,
        damageMult = 0.5f,
        fireRateMult = 1.8f,
        rangeMult = 0f,
        areaMult = 0.25f,
        bonusProjectiles = 0,
        hitStunDuration = 0.01f,
        knockbackDistance = 0f
    };

    [FormerlySerializedAs("laserStats")]
    public WeaponStatsData homingShotStats = new WeaponStatsData
    {
        displayName = "HomingShot",
        level = 0,
        unlocked = false,
        damageMult = 1.2f,
        fireRateMult = 0.72f,
        rangeMult = 1.6667f,
        areaMult = 0f,
        bonusProjectiles = 0,
        hitStunDuration = 0.08f,
        knockbackDistance = 0.08f
    };

    [FormerlySerializedAs("chainStats")]
    public WeaponStatsData grenadeStats = new WeaponStatsData
    {
        displayName = "Grenade",
        level = 0,
        unlocked = false,
        damageMult = 1.8f,
        fireRateMult = 0.66f,
        rangeMult = 2f,
        areaMult = 1f,
        bonusProjectiles = 0,
        hitStunDuration = 0.1f,
        knockbackDistance = 0.12f
    };

    [FormerlySerializedAs("lightningStats")]
    public WeaponStatsData meleeStats = new WeaponStatsData
    {
        displayName = "Melee",
        level = 0,
        unlocked = false,
        damageMult = 2f,
        fireRateMult = 1.5f,
        rangeMult = 0f,
        areaMult = 0.25f,
        bonusProjectiles = 0,
        hitStunDuration = 0.15f,
        knockbackDistance = 0.25f
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

    [Header("Start Character")]
    [FormerlySerializedAs("requireStartWeaponChoice")]
    public bool requireStartCharacterChoice = true;
    [FormerlySerializedAs("startWeapon")]
    public StartCharacterType startCharacter = StartCharacterType.SingleShot;

    [Header("Start Map")]
    public bool requireMapChoice = true;
    public bool allowMapChoiceInNetwork = false;
    public MapChoiceEntry[] mapChoices = new[]
    {
        new MapChoiceEntry { theme = MapTheme.Forest, displayName = "Forest", sceneName = "ForestOpenWorld" },
        new MapChoiceEntry { theme = MapTheme.Desert, displayName = "Desert", sceneName = "DesertOpenWorld" },
        new MapChoiceEntry { theme = MapTheme.Snow, displayName = "Snow", sceneName = "SnowOpenWorld" }
    };

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
    public bool allowTestSpawnSecret = true;
    public string testSpawnSecret = "test";
    public float testSpawnSecretTimeout = 1.5f;
    public bool allowLevelUpSecret = true;
    public string levelUpSecret = "lvl";
    public float levelUpSecretTimeout = 1.5f;
    public bool allowAdminWeaponUnlockSecret = true;
    public string adminWeaponUnlockSecret = "admin";
    public float adminWeaponUnlockSecretTimeout = 1.5f;
    public Vector2[] testSpawnOffsets = new[]
    {
        new Vector2(2f, 0f),
        new Vector2(-2f, 0f),
        new Vector2(0f, 2f)
    };
    public bool showColliderGizmos = true;
}

[System.Serializable]
public class MapChoiceEntry
{
    public MapTheme theme = MapTheme.Forest;
    public string displayName = "Forest";
    public string sceneName = "ForestOpenWorld";
    public DifficultyConfig difficulty;
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
    public bool enabled = true;
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
    public string labelText = "Minimap";
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
    public float enemyColliderRadius = 0.28f;
    public float enemySeparationRadius = 0.65f;
    public float enemySeparationStrength = 2.2f;
    public int enemySeparationMaxNeighbors = 8;
    public float enemyPlayerOverlapPushStrength = 6f;

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
    public Vector3 phase1Weights = new Vector3(0.7f, 0.3f, 0f);
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
    [FormerlySerializedAs("straightParallelSpacing")]
    public float singleShotParallelSpacing = 0.35f;
    [FormerlySerializedAs("novaOrbitAngularSpeed")]
    public float piercingShotOrbitAngularSpeed = 8f;

    [FormerlySerializedAs("shotgunBasePellets")]
    public int auraBasePellets = 5;
    [FormerlySerializedAs("shotgunSpreadAngle")]
    public float auraSpreadAngle = 32f;
    [FormerlySerializedAs("shotgunPelletDamageMult")]
    public float auraPelletDamageMult = 0.75f;
    [FormerlySerializedAs("shotgunSpeedMult")]
    public float auraSpeedMult = 0.95f;

    [FormerlySerializedAs("laserSpeedMult")]
    public float homingShotSpeedMult = 1.8f;
    [FormerlySerializedAs("laserThickness")]
    public float homingShotThickness = 0.12f;
    [FormerlySerializedAs("laserLengthScale")]
    public float homingShotLengthScale = 1.4f;
    [FormerlySerializedAs("laserParallelSpacing")]
    public float homingShotParallelSpacing = 0.3f;
    [FormerlySerializedAs("laserColor")]
    public Color homingShotColor = new Color(1f, 0.3f, 0.3f, 1f);

    [FormerlySerializedAs("chainBaseJumps")]
    public int grenadeBaseJumps = 3;
    [FormerlySerializedAs("chainJumpRangeMult")]
    public float grenadeJumpRangeMult = 0.7f;
    [FormerlySerializedAs("chainLineWidth")]
    public float grenadeLineWidth = 0.12f;
    [FormerlySerializedAs("chainEffectDuration")]
    public float grenadeEffectDuration = 0.12f;
    [FormerlySerializedAs("chainColor")]
    public Color grenadeColor = new Color(0.5f, 0.8f, 1f, 1f);


    [FormerlySerializedAs("lightningEffectDuration")]
    public float meleeEffectDuration = 0.12f;
    [FormerlySerializedAs("lightningLineWidth")]
    public float meleeLineWidth = 0.14f;
    [FormerlySerializedAs("lightningLineLength")]
    public float meleeLineLength = 1.6f;
    [FormerlySerializedAs("lightningColor")]
    public Color meleeColor = new Color(1f, 0.95f, 0.5f, 1f);
    public float meleeConeAngle = 120f;
    public float meleeSwordVisualScale = 0.9f;
    public float meleeSwordSpriteAngleOffset = -45f;

    [Header("Sprites (Resources)")]
    [FormerlySerializedAs("straightSpritePath")]
    public string singleShotSpritePath = "Art/Items/projectile_single_shot";
    [FormerlySerializedAs("boomerangSpritePath")]
    public string multiShotSpritePath = "Art/Items/projectile_multi_shot";
    [FormerlySerializedAs("novaSpritePath")]
    public string piercingShotSpritePath = "Art/Items/projectile_piercing_shot";
    [FormerlySerializedAs("shotgunSpritePath")]
    public string auraSpritePath = "Art/Projectiles/projectile_aura";
    [FormerlySerializedAs("shurikenSpritePath")]
    public string homingShotSpritePath = "Art/Items/projectile_homing_shot";
    [FormerlySerializedAs("frostSpritePath")]
    public string grenadeSpritePath = "Art/Items/projectile_grenade";
    public float projectileSpriteScale = 2.5f;
    public float piercingShotVisualScaleMult = 2f;
}

[System.Serializable]
public class PlayerConfig
{
    public float moveSpeed = 5f;
    public float colliderRadius = 0.28f;
    public float damageInvulnerabilityDuration = 0.35f;
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
    public float autoMinDistance = 3f;
    public float autoMaxDistance = 3.8f;
    public float autoOrbitStrength = 1f;
    public float autoKeepDistanceStrength = 0.85f;
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
    public float xpPickupScale = 2.1f;
    public float coinPickupScale = 1.9f;
    public float xpMagnetScanInterval = 0.2f;
    public float coinMagnetScanInterval = 0.2f;
    public int xpSpriteSize = 50;
    public int coinSpriteSize = 40;
    public string xpSpritePath = "Art/Items/pickup_xp";
    public string coinSpritePath = "Art/Items/pickup_coin";
    public Color xpColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color coinColor = new Color(1f, 0.85f, 0.2f, 1f);
    public float xpColliderRadius = 0.14f;
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


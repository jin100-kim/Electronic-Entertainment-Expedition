using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private float spawnInterval = 2f;

    [SerializeField]
    private int maxEnemies = 20;

    [SerializeField]
    private float spawnRadius = 8f;

    [SerializeField]
    private float enemyMoveSpeed = 1.6667f;

    [SerializeField]
    private float enemyDamage = 10f;

    [SerializeField]
    private float enemyDamageCooldown = 0.5f;

    [SerializeField]
    private int enemyXpReward = 2;

    [SerializeField]
    private float enemyMaxHealth = 40f;

    [SerializeField]
    private float enemyVisualScale = 4f;

    [Header("Elite/Boss Spawn")]
    [SerializeField]
    private float eliteStartTime = 120f;

    [SerializeField]
    private float eliteInterval = 20f;

    [SerializeField]
    [Range(0f, 1f)]
    private float eliteChance = 0.6f;

    [SerializeField]
    private int maxEliteAlive = 3;

    [SerializeField]
    private float bossStartTime = 600f;

    [SerializeField]
    private float bossInterval = 90f;

    [SerializeField]
    private int maxBossAlive = 1;

    [Header("Elite/Boss Multipliers")]
    [SerializeField]
    private float eliteHealthMult = 1.6f;

    [SerializeField]
    private float eliteDamageMult = 1.4f;

    [SerializeField]
    private float eliteSpeedMult = 1.15f;

    [SerializeField]
    private float eliteXpMult = 3f;

    [SerializeField]
    private float bossHealthMult = 4f;

    [SerializeField]
    private float bossDamageMult = 2f;

    [SerializeField]
    private float bossSpeedMult = 1.2f;

    [SerializeField]
    private float bossXpMult = 8f;

    [Header("Visual Variety")]
    [SerializeField]
    private bool enableVisualPhases = true;

    [SerializeField]
    private float phase1EndTime = 180f;

    [SerializeField]
    private float phase2EndTime = 360f;

    [SerializeField]
    private float phase3EndTime = 600f;

    [SerializeField]
    private Vector3 phase1Weights = new Vector3(1f, 0f, 0f);

    [SerializeField]
    private Vector3 phase2Weights = new Vector3(0.7f, 0.3f, 0f);

    [SerializeField]
    private Vector3 phase3Weights = new Vector3(0.4f, 0.4f, 0.2f);

    [SerializeField]
    private Vector3 phase4Weights = new Vector3(0.25f, 0.35f, 0.4f);

    private float _nextSpawnTime;
    private float _nextEliteTime = -1f;
    private float _nextBossTime = -1f;
    private readonly List<GameObject> _enemies = new List<GameObject>();
    private bool _settingsApplied;

    public Transform Target { get; set; }

    public float SpawnInterval
    {
        get => spawnInterval;
        set => spawnInterval = value;
    }

    public int MaxEnemies
    {
        get => maxEnemies;
        set => maxEnemies = value;
    }

    public float SpawnRadius
    {
        get => spawnRadius;
        set => spawnRadius = value;
    }

    public float EnemyMoveSpeed
    {
        get => enemyMoveSpeed;
        set => enemyMoveSpeed = Mathf.Max(0.1f, value);
    }

    public float EnemyDamage
    {
        get => enemyDamage;
        set => enemyDamage = Mathf.Max(0f, value);
    }

    public float EnemyDamageCooldown
    {
        get => enemyDamageCooldown;
        set => enemyDamageCooldown = Mathf.Max(0.05f, value);
    }

    public int EnemyXpReward
    {
        get => enemyXpReward;
        set => enemyXpReward = Mathf.Max(1, value);
    }

    public float EnemyMaxHealth
    {
        get => enemyMaxHealth;
        set => enemyMaxHealth = Mathf.Max(1f, value);
    }

    public float EliteHealthMult
    {
        get => eliteHealthMult;
        set => eliteHealthMult = Mathf.Max(0.1f, value);
    }

    public float BossHealthMult
    {
        get => bossHealthMult;
        set => bossHealthMult = Mathf.Max(0.1f, value);
    }

    private void Awake()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.enemySpawner;

        spawnInterval = settings.spawnInterval;
        maxEnemies = settings.maxEnemies;
        spawnRadius = settings.spawnRadius;
        enemyMoveSpeed = settings.enemyMoveSpeed;
        enemyDamage = settings.enemyDamage;
        enemyDamageCooldown = settings.enemyDamageCooldown;
        enemyXpReward = settings.enemyXpReward;
        enemyMaxHealth = settings.enemyMaxHealth;
        enemyVisualScale = settings.enemyVisualScale;

        eliteStartTime = settings.eliteStartTime;
        eliteInterval = settings.eliteInterval;
        eliteChance = settings.eliteChance;
        maxEliteAlive = settings.maxEliteAlive;
        bossStartTime = settings.bossStartTime;
        bossInterval = settings.bossInterval;
        maxBossAlive = settings.maxBossAlive;

        eliteHealthMult = settings.eliteHealthMult;
        eliteDamageMult = settings.eliteDamageMult;
        eliteSpeedMult = settings.eliteSpeedMult;
        eliteXpMult = settings.eliteXpMult;

        bossHealthMult = settings.bossHealthMult;
        bossDamageMult = settings.bossDamageMult;
        bossSpeedMult = settings.bossSpeedMult;
        bossXpMult = settings.bossXpMult;

        enableVisualPhases = settings.enableVisualPhases;
        phase1EndTime = settings.phase1EndTime;
        phase2EndTime = settings.phase2EndTime;
        phase3EndTime = settings.phase3EndTime;
        phase1Weights = settings.phase1Weights;
        phase2Weights = settings.phase2Weights;
        phase3Weights = settings.phase3Weights;
        phase4Weights = settings.phase4Weights;

        _settingsApplied = true;
    }

    private void Update()
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        if (GameSession.Instance != null && !GameSession.Instance.IsGameplayActive)
        {
            return;
        }

        CleanupDeadEnemies();

        if (Time.time >= _nextSpawnTime && _enemies.Count < maxEnemies)
        {
            var spawnTarget = ResolveSpawnTarget();
            if (spawnTarget != null)
            {
                SpawnEnemyWithTier(spawnTarget);
                _nextSpawnTime = Time.time + spawnInterval;
            }
        }
    }

    private Transform ResolveSpawnTarget()
    {
        if (NetworkSession.IsActive)
        {
            var players = PlayerController.Active;
            if (players != null && players.Count > 0)
            {
                int start = Random.Range(0, players.Count);
                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[(start + i) % players.Count];
                    if (player == null)
                    {
                        continue;
                    }

                    var health = player.GetComponent<Health>();
                    if (health != null && health.IsDead)
                    {
                        continue;
                    }

                    return player.transform;
                }
            }
        }

        return Target;
    }

    private void SpawnEnemyWithTier(Transform spawnTarget)
    {
        float elapsed = GetElapsedTime();
        if (_nextEliteTime < 0f)
        {
            _nextEliteTime = eliteStartTime;
        }
        if (_nextBossTime < 0f)
        {
            _nextBossTime = bossStartTime;
        }

        if (elapsed >= _nextBossTime && CountAlive(EnemyTier.Tier.Boss) < maxBossAlive)
        {
            SpawnEnemy(EnemyTier.Tier.Boss, spawnTarget);
            _nextBossTime = elapsed + bossInterval;
            return;
        }

        if (elapsed >= _nextEliteTime && CountAlive(EnemyTier.Tier.Elite) < maxEliteAlive)
        {
            if (Random.value <= eliteChance)
            {
                SpawnEnemy(EnemyTier.Tier.Elite, spawnTarget);
            }
            _nextEliteTime = elapsed + eliteInterval;
            return;
        }

        SpawnEnemy(EnemyTier.Tier.Normal, spawnTarget);
    }

    private void SpawnEnemy(EnemyTier.Tier tier, Transform spawnTarget)
    {
        SpawnEnemyInternal(tier, spawnTarget, null, false, null);
    }

    public bool SpawnManual(EnemyVisuals.EnemyVisualType visualType, Transform spawnTarget, Vector3? positionOverride = null)
    {
        return SpawnEnemyInternal(EnemyTier.Tier.Normal, spawnTarget, visualType, true, positionOverride);
    }

    private bool SpawnEnemyInternal(EnemyTier.Tier tier, Transform spawnTarget, EnemyVisuals.EnemyVisualType? visualOverride, bool forceSpawn, Vector3? positionOverride)
    {
        if (spawnTarget == null)
        {
            return false;
        }

        if (!forceSpawn && _enemies.Count >= maxEnemies)
        {
            return false;
        }

        Vector3 position;
        if (positionOverride.HasValue)
        {
            position = positionOverride.Value;
        }
        else
        {
            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
            position = spawnTarget.position + new Vector3(offset.x, offset.y, 0f);
        }
        if (GameSession.Instance != null)
        {
            position = GameSession.Instance.ClampToBounds(position);
        }

        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return false;
        }

        GameObject enemy = NetworkSession.IsActive ? RuntimeNetworkPrefabs.InstantiateEnemy() : new GameObject("Enemy");
        if (enemy == null)
        {
            return false;
        }

        enemy.name = "Enemy";
        enemy.transform.position = position;

        var renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = enemy.AddComponent<SpriteRenderer>();
        }
        renderer.enabled = false;

        enemy.transform.localScale = Vector3.one;

        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = enemy.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = enemy.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = enemy.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        var controller = enemy.GetComponent<EnemyController>();
        if (controller == null)
        {
            controller = enemy.AddComponent<EnemyController>();
        }
        controller.Target = spawnTarget;
        controller.MoveSpeed = enemyMoveSpeed * GetSpeedMult(tier);
        controller.ContactDamage = enemyDamage * GetDamageMult(tier);
        controller.DamageCooldown = enemyDamageCooldown;
        controller.XpReward = Mathf.Max(1, Mathf.RoundToInt(enemyXpReward * GetXpMult(tier)));
        controller.MaxHealth = enemyMaxHealth * GetHealthMult(tier);

        var visuals = enemy.GetComponent<EnemyVisuals>();
        if (visuals == null)
        {
            visuals = enemy.AddComponent<EnemyVisuals>();
        }
        var visualType = visualOverride.HasValue ? visualOverride.Value : GetVisualType(tier);
        visuals.SetType(visualType);
        visuals.SetVisualScale(enemyVisualScale);

        var tierInfo = enemy.GetComponent<EnemyTier>();
        if (tierInfo == null)
        {
            tierInfo = enemy.AddComponent<EnemyTier>();
        }
        tierInfo.SetTier(tier);

        var netState = enemy.GetComponent<EnemyNetState>();
        if (netState != null)
        {
            netState.SetTier(tier);
            netState.SetVisualScale(enemyVisualScale);
        }

        if (NetworkSession.IsActive)
        {
            var netObj = enemy.GetComponent<Unity.Netcode.NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }
        }

        _enemies.Add(enemy);
        return true;
    }

    private void CleanupDeadEnemies()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            if (_enemies[i] == null)
            {
                _enemies.RemoveAt(i);
            }
        }
    }

    private float GetElapsedTime()
    {
        var session = GameSession.Instance;
        return session != null ? session.ElapsedTime : Time.time;
    }

    private int CountAlive(EnemyTier.Tier tier)
    {
        int count = 0;
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            if (enemy == null)
            {
                continue;
            }

            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null && controller.IsDead)
            {
                continue;
            }

            var tierInfo = enemy.GetComponent<EnemyTier>();
            if (tierInfo != null && tierInfo.CurrentTier == tier)
            {
                count++;
            }
        }

        return count;
    }

    private float GetHealthMult(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return bossHealthMult;
            case EnemyTier.Tier.Elite:
                return eliteHealthMult;
            default:
                return 1f;
        }
    }

    private float GetDamageMult(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return bossDamageMult;
            case EnemyTier.Tier.Elite:
                return eliteDamageMult;
            default:
                return 1f;
        }
    }

    private float GetSpeedMult(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return bossSpeedMult;
            case EnemyTier.Tier.Elite:
                return eliteSpeedMult;
            default:
                return 1f;
        }
    }

    private float GetXpMult(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return bossXpMult;
            case EnemyTier.Tier.Elite:
                return eliteXpMult;
            default:
                return 1f;
        }
    }

    private EnemyVisuals.EnemyVisualType GetVisualType(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return EnemyVisuals.EnemyVisualType.Skeleton;
            case EnemyTier.Tier.Elite:
                return EnemyVisuals.EnemyVisualType.Mushroom;
            default:
                return GetNormalVisualType();
        }
    }

    private EnemyVisuals.EnemyVisualType GetNormalVisualType()
    {
        if (!enableVisualPhases)
        {
            return EnemyVisuals.EnemyVisualType.Slime;
        }

        float elapsed = GetElapsedTime();
        Vector3 weights = phase1Weights;
        if (elapsed >= phase3EndTime)
        {
            weights = phase4Weights;
        }
        else if (elapsed >= phase2EndTime)
        {
            weights = phase3Weights;
        }
        else if (elapsed >= phase1EndTime)
        {
            weights = phase2Weights;
        }

        float total = Mathf.Max(0f, weights.x) + Mathf.Max(0f, weights.y) + Mathf.Max(0f, weights.z);
        if (total <= 0.001f)
        {
            return EnemyVisuals.EnemyVisualType.Slime;
        }

        float roll = Random.value * total;
        float slime = Mathf.Max(0f, weights.x);
        float mushroom = Mathf.Max(0f, weights.y);

        if (roll < slime)
        {
            return EnemyVisuals.EnemyVisualType.Slime;
        }

        if (roll < slime + mushroom)
        {
            return EnemyVisuals.EnemyVisualType.Mushroom;
        }

        return EnemyVisuals.EnemyVisualType.Skeleton;
    }

    // Visuals are now handled by EnemyVisuals + Animator.
}

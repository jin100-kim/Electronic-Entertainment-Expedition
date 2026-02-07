using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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

    private float _nextSpawnTime;
    private float _nextEliteTime = -1f;
    private float _nextBossTime = -1f;
    private readonly List<GameObject> _enemies = new List<GameObject>();

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

    private void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        if (Target == null)
        {
            return;
        }

        CleanupDeadEnemies();

        if (Time.time >= _nextSpawnTime && _enemies.Count < maxEnemies)
        {
            SpawnEnemyWithTier();
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnEnemyWithTier()
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
            SpawnEnemy(EnemyTier.Tier.Boss);
            _nextBossTime = elapsed + bossInterval;
            return;
        }

        if (elapsed >= _nextEliteTime && CountAlive(EnemyTier.Tier.Elite) < maxEliteAlive)
        {
            if (Random.value <= eliteChance)
            {
                SpawnEnemy(EnemyTier.Tier.Elite);
            }
            _nextEliteTime = elapsed + eliteInterval;
            return;
        }

        SpawnEnemy(EnemyTier.Tier.Normal);
    }

    private void SpawnEnemy(EnemyTier.Tier tier)
    {
        Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 position = Target.position + new Vector3(offset.x, offset.y, 0f);
        if (GameSession.Instance != null)
        {
            position = GameSession.Instance.ClampToBounds(position);
        }

        var enemy = new GameObject("Enemy");
        enemy.transform.position = position;

        var renderer = enemy.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        enemy.transform.localScale = Vector3.one;

        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var controller = enemy.AddComponent<EnemyController>();
        controller.Target = Target;
        controller.MoveSpeed = enemyMoveSpeed * GetSpeedMult(tier);
        controller.ContactDamage = enemyDamage * GetDamageMult(tier);
        controller.DamageCooldown = enemyDamageCooldown;
        controller.XpReward = Mathf.Max(1, Mathf.RoundToInt(enemyXpReward * GetXpMult(tier)));
        controller.MaxHealth = enemyMaxHealth * GetHealthMult(tier);

        var visuals = enemy.AddComponent<EnemyVisuals>();
        visuals.SetType(GetVisualType(tier));
        visuals.SetVisualScale(enemyVisualScale);

        var tierInfo = enemy.AddComponent<EnemyTier>();
        tierInfo.SetTier(tier);

        _enemies.Add(enemy);
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
                return EnemyVisuals.EnemyVisualType.Slime;
        }
    }

    // Visuals are now handled by EnemyVisuals + Animator.
}

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

    private float _nextSpawnTime;
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
            SpawnEnemy();
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnEnemy()
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
        controller.MoveSpeed = enemyMoveSpeed;
        controller.ContactDamage = enemyDamage;
        controller.DamageCooldown = enemyDamageCooldown;
        controller.XpReward = enemyXpReward;
        controller.MaxHealth = enemyMaxHealth;

        var visuals = enemy.AddComponent<EnemyVisuals>();
        visuals.SetType(EnemyVisuals.EnemyVisualType.Slime);
        visuals.SetVisualScale(enemyVisualScale);

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

    // Visuals are now handled by EnemyVisuals + Animator.
}

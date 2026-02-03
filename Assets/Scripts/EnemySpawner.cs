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
        renderer.sprite = CreateCircleSprite(50);
        renderer.color = new Color(0.9f, 0.2f, 0.2f, 1f);

        enemy.transform.localScale = Vector3.one * 0.7f;

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

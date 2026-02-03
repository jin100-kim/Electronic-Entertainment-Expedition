using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    public enum WeaponType
    {
        Straight,
        Boomerang,
        Nova
    }

    [SerializeField]
    private float baseFireInterval = 0.6f;

    [SerializeField]
    private float baseProjectileSpeed = 10f;

    [SerializeField]
    private float baseProjectileDamage = 10f;

    [SerializeField]
    private float baseRange = 6f;

    [SerializeField]
    private int baseProjectileSize = 50;

    [SerializeField]
    private float baseProjectileLifetime = 2f;

    [SerializeField]
    private int baseProjectilePierce = 0;

    private float _fireInterval;
    private float _baseInterval;
    private float _projectileSpeed;
    private float _projectileDamage;
    private float _range;
    private int _projectileSize;
    private float _projectileLifetime;
    private int _projectileCount = 1;
    private int _projectilePierce;
    private int _projectilePierceBonus;
    private float _weaponDamageMult = 1f;

    private struct WeaponConfig
    {
        public bool Enabled;
        public float DamageMult;
        public float FireRateMult;
        public float RangeMult;
        public int BonusCount;
    }

    private WeaponConfig _straight;
    private WeaponConfig _boomerang;
    private WeaponConfig _nova;

    private float _nextFireStraight;
    private float _nextFireBoomerang;
    private float _nextFireNova;

    private void Awake()
    {
        ApplyStats(1f, 1f, 1f, 1f, 1f, 1, 0, 1f);
        _straight = CreateDefaultConfig(true);
        _boomerang = CreateDefaultConfig(false);
        _nova = CreateDefaultConfig(false);
    }

    private void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        bool needsTarget = _straight.Enabled || _boomerang.Enabled;
        Transform target = null;
        Vector2 dir = Vector2.zero;

        if (needsTarget)
        {
            target = FindClosestEnemy();
            if (target != null)
            {
                dir = (target.position - transform.position);
            }
        }

        if (_straight.Enabled && Time.time >= _nextFireStraight && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireStraight(dir.normalized);
            _nextFireStraight = Time.time + GetIntervalForWeapon(_straight.FireRateMult);
        }

        if (_boomerang.Enabled && Time.time >= _nextFireBoomerang && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireBoomerang(dir.normalized);
            _nextFireBoomerang = Time.time + GetIntervalForWeapon(_boomerang.FireRateMult);
        }

        if (_nova.Enabled && Time.time >= _nextFireNova && target != null)
        {
            FireNova();
            _nextFireNova = Time.time + GetIntervalForWeapon(_nova.FireRateMult);
        }
    }

    public void ApplyStats(float damageMult, float fireRateMult, float rangeMult, float sizeMult, float lifetimeMult, int projectileCount, int pierceBonus, float weaponDamageMult)
    {
        _weaponDamageMult = Mathf.Max(0.1f, weaponDamageMult);
        _projectileDamage = baseProjectileDamage * Mathf.Max(0.1f, damageMult) * _weaponDamageMult;
        _baseInterval = Mathf.Max(0.05f, baseFireInterval / Mathf.Max(0.1f, fireRateMult));
        _fireInterval = _baseInterval;
        _range = baseRange * Mathf.Max(0.1f, rangeMult);
        _projectileSpeed = baseProjectileSpeed;
        _projectileSize = Mathf.Max(2, Mathf.RoundToInt(baseProjectileSize * Mathf.Max(0.2f, sizeMult)));
        _projectileLifetime = baseProjectileLifetime * Mathf.Max(0.1f, lifetimeMult);
        _projectileCount = Mathf.Max(1, projectileCount);
        _projectilePierceBonus = Mathf.Max(0, pierceBonus);
        _projectilePierce = Mathf.Max(0, baseProjectilePierce + _projectilePierceBonus);
    }

    public void SetWeaponStats(WeaponType type, WeaponStatsData stats)
    {
        var cfg = CreateConfigFromStats(stats);
        switch (type)
        {
            case WeaponType.Boomerang:
                _boomerang = cfg;
                break;
            case WeaponType.Nova:
                _nova = cfg;
                break;
            default:
                _straight = cfg;
                break;
        }
    }


    private Transform FindClosestEnemy()
    {
        float best = _range * _range;
        Transform bestTarget = null;
        var enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            float dist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (dist <= best)
            {
                best = dist;
                bestTarget = enemy.transform;
            }
        }

        return bestTarget;
    }

    private void FireStraight(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _straight.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        if (_projectileCount <= 1)
        {
            SpawnProjectile(direction, _projectileDamage * _straight.DamageMult, 0f, lifetime);
            _range = savedRange;
            return;
        }

        float angleStep = 16f;
        float start = -angleStep * (_projectileCount - 1) * 0.5f;
        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = start + angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnProjectile(dir, _projectileDamage * _straight.DamageMult, 0f, lifetime);
        }

        _range = savedRange;
    }

    private void FireBoomerang(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _boomerang.RangeMult;
        float lifetime = CalculateBoomerangLifetimeForRange(_range);

        if (_projectileCount <= 1)
        {
            SpawnBoomerang(direction, _projectileDamage * _boomerang.DamageMult, lifetime);
            _range = savedRange;
            return;
        }

        float angleStep = 20f;
        float start = -angleStep * (_projectileCount - 1) * 0.5f;
        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = start + angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnBoomerang(dir, _projectileDamage * _boomerang.DamageMult, lifetime);
        }

        _range = savedRange;
    }

    private void FireNova()
    {
        float savedRange = _range;
        _range *= _nova.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        int count = 8 + _nova.BonusCount;
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
            SpawnNovaProjectile(dir, lifetime);
        }

        _range = savedRange;
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed = 0f, float lifetimeOverride = -1f)
    {
        var go = new GameObject("Projectile");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(_projectileSize);
        renderer.color = new Color(0.9f, 0.9f, 0.2f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.AddComponent<Projectile>();
        float life = lifetimeOverride > 0f ? lifetimeOverride : _projectileLifetime;
        proj.Initialize(direction, _projectileSpeed, damageOverride, life, _projectilePierce, spinSpeed);
    }

    private void SpawnNovaProjectile(Vector2 direction, float lifetime)
    {
        var go = new GameObject("NovaProjectile");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(_projectileSize);
        renderer.color = new Color(0.6f, 0.8f, 1f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.AddComponent<Projectile>();
        proj.InitializeOrbit(transform.position, direction, _projectileSpeed, 4f, _projectileDamage * _nova.DamageMult, lifetime, _projectilePierce, 720f);
    }

    private void SpawnBoomerang(Vector2 direction, float damageOverride, float lifetime)
    {
        var go = new GameObject("Boomerang");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.45f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(_projectileSize);
        renderer.color = new Color(0.2f, 0.9f, 0.9f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var boom = go.AddComponent<BoomerangProjectile>();
        boom.Initialize(transform, direction, _projectileSpeed, damageOverride, lifetime, 9999);
    }

    private float CalculateLifetimeForRange(float range)
    {
        return Mathf.Max(0.1f, range / Mathf.Max(0.1f, _projectileSpeed));
    }

    private float CalculateBoomerangLifetimeForRange(float range)
    {
        return Mathf.Max(0.2f, (range * 2f) / Mathf.Max(0.1f, _projectileSpeed));
    }

    private float GetIntervalForWeapon(float weaponFireRateMult)
    {
        return Mathf.Max(0.05f, _baseInterval / Mathf.Max(0.1f, weaponFireRateMult));
    }

    private static WeaponConfig CreateDefaultConfig(bool enabled)
    {
        return new WeaponConfig
        {
            Enabled = enabled,
            DamageMult = 1f,
            FireRateMult = 1f,
            RangeMult = 1f,
            BonusCount = 0
        };
    }

    private static WeaponConfig CreateConfigFromStats(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return CreateDefaultConfig(false);
        }

        return new WeaponConfig
        {
            Enabled = stats.unlocked && stats.level > 0,
            DamageMult = Mathf.Max(0.1f, stats.damageMult),
            FireRateMult = Mathf.Max(0.1f, stats.fireRateMult),
            RangeMult = Mathf.Max(0.1f, stats.rangeMult),
            BonusCount = Mathf.Max(0, stats.bonusProjectiles)
        };
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

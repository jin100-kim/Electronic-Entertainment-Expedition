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
    private float baseRange = 2f;

    [SerializeField]
    private int baseProjectileSize = 50;

    [SerializeField]
    private float baseProjectileLifetime = 2f;

    [SerializeField]
    private int baseProjectilePierce = 0;

    private float _fireInterval;
    private float _projectileSpeed;
    private float _projectileDamage;
    private float _range;
    private int _projectileSize;
    private float _projectileLifetime;
    private int _projectileCount = 1;
    private int _projectilePierce;
    private int _projectilePierceBonus;
    private float _weaponDamageMult = 1f;
    private float _straightDamageMult = 1f;
    private float _boomerangDamageMult = 1f;
    private float _novaDamageMult = 1f;
    private int _novaBonusCount;

    private float _nextFireStraight;
    private float _nextFireBoomerang;
    private float _nextFireNova;

    private bool _straightEnabled = true;
    private bool _boomerangEnabled;
    private bool _novaEnabled;

    private void Awake()
    {
        ApplyStats(1f, 1f, 1f, 1f, 1f, 1, 0, 1f);
    }

    private void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        bool needsTarget = _straightEnabled || _boomerangEnabled;
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

        if (_straightEnabled && Time.time >= _nextFireStraight && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireStraight(dir.normalized);
            _nextFireStraight = Time.time + _fireInterval;
        }

        if (_boomerangEnabled && Time.time >= _nextFireBoomerang && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireBoomerang(dir.normalized);
            _nextFireBoomerang = Time.time + _fireInterval;
        }

        if (_novaEnabled && Time.time >= _nextFireNova && target != null)
        {
            FireNova();
            _nextFireNova = Time.time + _fireInterval;
        }
    }

    public void ApplyStats(float damageMult, float fireRateMult, float rangeMult, float sizeMult, float lifetimeMult, int projectileCount, int pierceBonus, float weaponDamageMult)
    {
        _weaponDamageMult = Mathf.Max(0.1f, weaponDamageMult);
        _projectileDamage = baseProjectileDamage * Mathf.Max(0.1f, damageMult) * _weaponDamageMult;
        _fireInterval = Mathf.Max(0.05f, baseFireInterval / Mathf.Max(0.1f, fireRateMult));
        _range = baseRange * Mathf.Max(0.1f, rangeMult);
        _projectileSpeed = baseProjectileSpeed;
        _projectileSize = Mathf.Max(2, Mathf.RoundToInt(baseProjectileSize * Mathf.Max(0.2f, sizeMult)));
        _projectileLifetime = baseProjectileLifetime * Mathf.Max(0.1f, lifetimeMult);
        _projectileCount = Mathf.Max(1, projectileCount);
        _projectilePierceBonus = Mathf.Max(0, pierceBonus);
        _projectilePierce = Mathf.Max(0, baseProjectilePierce + _projectilePierceBonus);
    }

    public void SetWeaponEnabled(WeaponType type, bool enabled)
    {
        switch (type)
        {
            case WeaponType.Boomerang:
                _boomerangEnabled = enabled;
                break;
            case WeaponType.Nova:
                _novaEnabled = enabled;
                break;
            default:
                _straightEnabled = enabled;
                break;
        }
    }

    public void SetWeaponDamageMultipliers(float straight, float boomerang, float nova)
    {
        _straightDamageMult = Mathf.Max(0.1f, straight);
        _boomerangDamageMult = Mathf.Max(0.1f, boomerang);
        _novaDamageMult = Mathf.Max(0.1f, nova);
    }

    public void SetNovaBonusCount(int value)
    {
        _novaBonusCount = Mathf.Max(0, value);
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
        if (_projectileCount <= 1)
        {
            SpawnProjectile(direction, _projectileDamage * _straightDamageMult);
            return;
        }

        float angleStep = 16f;
        float start = -angleStep * (_projectileCount - 1) * 0.5f;
        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = start + angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnProjectile(dir, _projectileDamage * _straightDamageMult);
        }
    }

    private void FireBoomerang(Vector2 direction)
    {
        if (_projectileCount <= 1)
        {
            SpawnBoomerang(direction, _projectileDamage * _boomerangDamageMult);
            return;
        }

        float angleStep = 20f;
        float start = -angleStep * (_projectileCount - 1) * 0.5f;
        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = start + angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnBoomerang(dir, _projectileDamage * _boomerangDamageMult);
        }
    }

    private void FireNova()
    {
        int count = 8 + _novaBonusCount;
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
            SpawnProjectile(dir, _projectileDamage * _novaDamageMult);
        }
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride)
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
        proj.Initialize(direction, _projectileSpeed, damageOverride, _projectileLifetime, _projectilePierce);
    }

    private void SpawnBoomerang(Vector2 direction, float damageOverride)
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
        boom.Initialize(transform, direction, _projectileSpeed, damageOverride, _projectileLifetime, _projectilePierce);
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

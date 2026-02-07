using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    public enum WeaponType
    {
        Straight,
        Boomerang,
        Nova,
        Shotgun,
        Laser,
        ChainLightning,
        Lightning,
        Drone,
        Shuriken,
        FrostOrb
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

    [SerializeField]
    private float straightParallelSpacing = 0.35f;

    [SerializeField]
    private float novaOrbitAngularSpeed = 8f;

    [Header("Shotgun")]
    [SerializeField]
    private int shotgunBasePellets = 5;

    [SerializeField]
    private float shotgunSpreadAngle = 32f;

    [SerializeField]
    private float shotgunPelletDamageMult = 0.75f;

    [SerializeField]
    private float shotgunSpeedMult = 0.95f;

    [Header("Laser")]
    [SerializeField]
    private float laserSpeedMult = 1.8f;

    [SerializeField]
    private float laserThickness = 0.12f;

    [SerializeField]
    private float laserLengthScale = 1.4f;

    [SerializeField]
    private float laserParallelSpacing = 0.3f;

    [SerializeField]
    private Color laserColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Chain Lightning")]
    [SerializeField]
    private int chainBaseJumps = 3;

    [SerializeField]
    private float chainJumpRangeMult = 0.7f;

    [SerializeField]
    private float chainLineWidth = 0.12f;

    [SerializeField]
    private float chainEffectDuration = 0.12f;

    [SerializeField]
    private Color chainColor = new Color(0.5f, 0.8f, 1f, 1f);

    [Header("Drone")]
    [SerializeField]
    private float droneOrbitRadius = 1.8f;

    [SerializeField]
    private float droneAngularSpeed = 3.2f;

    [SerializeField]
    private float droneLifetime = 6f;

    [SerializeField]
    private float droneDamageMult = 0.6f;

    [SerializeField]
    private Color droneColor = new Color(0.9f, 0.9f, 1f, 1f);

    [Header("Shuriken")]
    [SerializeField]
    private float shurikenSpeedMult = 1.4f;

    [SerializeField]
    private float shurikenSpinSpeed = 1080f;

    [SerializeField]
    private float shurikenDamageMult = 0.85f;

    [SerializeField]
    private Color shurikenColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("Frost Orb")]
    [SerializeField]
    private float frostSpeedMult = 0.8f;

    [SerializeField]
    private float frostDamageMult = 0.8f;

    [SerializeField]
    private float frostSlowMultiplier = 0.6f;

    [SerializeField]
    private float frostSlowDuration = 1.5f;

    [SerializeField]
    private Color frostColor = new Color(0.6f, 0.85f, 1f, 1f);

    [Header("Lightning Strike")]
    [SerializeField]
    private float lightningEffectDuration = 0.12f;

    [SerializeField]
    private float lightningLineWidth = 0.14f;

    [SerializeField]
    private float lightningLineLength = 1.6f;

    [SerializeField]
    private Color lightningColor = new Color(1f, 0.95f, 0.5f, 1f);

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
    private WeaponConfig _shotgun;
    private WeaponConfig _laser;
    private WeaponConfig _chain;
    private WeaponConfig _lightning;
    private WeaponConfig _drone;
    private WeaponConfig _shuriken;
    private WeaponConfig _frost;

    private float _nextFireStraight;
    private float _nextFireBoomerang;
    private float _nextFireNova;
    private float _nextFireShotgun;
    private float _nextFireLaser;
    private float _nextFireChain;
    private float _nextFireLightning;
    private float _nextFireDrone;
    private float _nextFireShuriken;
    private float _nextFireFrost;

    private void Awake()
    {
        ApplyStats(1f, 1f, 1f, 1f, 1f, 1, 0, 1f);
        _straight = CreateDefaultConfig(true);
        _boomerang = CreateDefaultConfig(false);
        _nova = CreateDefaultConfig(false);
        _shotgun = CreateDefaultConfig(false);
        _laser = CreateDefaultConfig(false);
        _chain = CreateDefaultConfig(false);
        _lightning = CreateDefaultConfig(false);
        _drone = CreateDefaultConfig(false);
        _shuriken = CreateDefaultConfig(false);
        _frost = CreateDefaultConfig(false);
    }

    private void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        bool needsTarget = _straight.Enabled || _boomerang.Enabled || _shotgun.Enabled || _laser.Enabled || _chain.Enabled || _shuriken.Enabled || _frost.Enabled;
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

        if (_nova.Enabled && Time.time >= _nextFireNova)
        {
            FireNova();
            _nextFireNova = Time.time + GetIntervalForWeapon(_nova.FireRateMult);
        }

        if (_shotgun.Enabled && Time.time >= _nextFireShotgun && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireShotgun(dir.normalized);
            _nextFireShotgun = Time.time + GetIntervalForWeapon(_shotgun.FireRateMult);
        }

        if (_laser.Enabled && Time.time >= _nextFireLaser && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireLaser(dir.normalized);
            _nextFireLaser = Time.time + GetIntervalForWeapon(_laser.FireRateMult);
        }

        if (_chain.Enabled && Time.time >= _nextFireChain && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireChainLightning(target);
            _nextFireChain = Time.time + GetIntervalForWeapon(_chain.FireRateMult);
        }

        if (_lightning.Enabled && Time.time >= _nextFireLightning)
        {
            FireLightningStrike();
            _nextFireLightning = Time.time + GetIntervalForWeapon(_lightning.FireRateMult);
        }

        if (_drone.Enabled && Time.time >= _nextFireDrone)
        {
            FireDrone();
            _nextFireDrone = Time.time + GetIntervalForWeapon(_drone.FireRateMult);
        }

        if (_shuriken.Enabled && Time.time >= _nextFireShuriken && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireShuriken(dir.normalized);
            _nextFireShuriken = Time.time + GetIntervalForWeapon(_shuriken.FireRateMult);
        }

        if (_frost.Enabled && Time.time >= _nextFireFrost && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireFrostOrb(dir.normalized);
            _nextFireFrost = Time.time + GetIntervalForWeapon(_frost.FireRateMult);
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
            case WeaponType.Shotgun:
                _shotgun = cfg;
                break;
            case WeaponType.Laser:
                _laser = cfg;
                break;
            case WeaponType.ChainLightning:
                _chain = cfg;
                break;
            case WeaponType.Lightning:
                _lightning = cfg;
                break;
            case WeaponType.Drone:
                _drone = cfg;
                break;
            case WeaponType.Shuriken:
                _shuriken = cfg;
                break;
            case WeaponType.FrostOrb:
                _frost = cfg;
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

        int count = GetWeaponCount(_straight);
        if (count <= 1)
        {
            SpawnProjectile(direction, _projectileDamage * _straight.DamageMult, 0f, lifetime);
            _range = savedRange;
            return;
        }

        Vector2 perp = new Vector2(-direction.y, direction.x);
        float start = -(count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float offsetIndex = start + i;
            Vector2 spawnOffset = perp * (offsetIndex * straightParallelSpacing);
            SpawnProjectile(direction, _projectileDamage * _straight.DamageMult, 0f, lifetime, spawnOffset);
        }

        _range = savedRange;
    }

    private void FireBoomerang(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _boomerang.RangeMult;
        float lifetime = CalculateBoomerangLifetimeForRange(_range);

        int count = GetWeaponCount(_boomerang);
        if (count <= 1)
        {
            SpawnBoomerang(direction, _projectileDamage * _boomerang.DamageMult, lifetime);
            _range = savedRange;
            return;
        }

        float angleStep = 20f;
        float start = -angleStep * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
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
            SpawnNovaProjectile(dir, lifetime, 1f);
        }

        _range = savedRange;
    }

    private void FireShotgun(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _shotgun.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        int count = Mathf.Max(1, shotgunBasePellets + _shotgun.BonusCount + Mathf.Max(0, _projectileCount - 1));
        float spread = Mathf.Max(0f, shotgunSpreadAngle);
        float step = count <= 1 ? 0f : spread / (count - 1);
        float start = -spread * 0.5f;
        float pelletDamage = _projectileDamage * _shotgun.DamageMult * shotgunPelletDamageMult;
        float speed = _projectileSpeed * Mathf.Max(0.1f, shotgunSpeedMult);

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnProjectile(dir, pelletDamage, 0f, lifetime, Vector2.zero, speed);
        }

        _range = savedRange;
    }

    private void FireLaser(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _laser.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        int count = GetWeaponCount(_laser);
        Vector2 perp = new Vector2(-direction.y, direction.x);
        float start = -(count - 1) * 0.5f;
        float speed = _projectileSpeed * Mathf.Max(0.1f, laserSpeedMult);

        for (int i = 0; i < count; i++)
        {
            float offsetIndex = start + i;
            Vector2 spawnOffset = perp * (offsetIndex * laserParallelSpacing);
            SpawnLaserProjectile(direction, _projectileDamage * _laser.DamageMult, lifetime, spawnOffset, speed);
        }

        _range = savedRange;
    }

    private void FireChainLightning(Transform target)
    {
        if (target == null)
        {
            return;
        }

        float savedRange = _range;
        _range *= _chain.RangeMult;
        float jumpRange = Mathf.Max(0.1f, _range * chainJumpRangeMult);
        int jumps = Mathf.Max(1, chainBaseJumps + _chain.BonusCount);

        var enemies = FindObjectsOfType<EnemyController>();
        var hit = new System.Collections.Generic.HashSet<Health>();
        var points = new System.Collections.Generic.List<Vector3>();
        points.Add(transform.position);

        Transform current = target;
        for (int i = 0; i < jumps; i++)
        {
            if (current == null)
            {
                break;
            }

            var health = current.GetComponent<Health>();
            if (health != null && !hit.Contains(health))
            {
                health.Damage(_projectileDamage * _chain.DamageMult);
                hit.Add(health);
            }

            points.Add(current.position);
            current = FindClosestEnemyToPoint(current.position, enemies, hit, jumpRange);
        }

        SpawnChainEffect(points);
        _range = savedRange;
    }

    private void FireLightningStrike()
    {
        var enemies = FindObjectsOfType<EnemyController>();
        if (enemies == null || enemies.Length == 0)
        {
            return;
        }

        int strikes = Mathf.Max(1, 1 + _lightning.BonusCount);
        var used = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < strikes; i++)
        {
            if (used.Count >= enemies.Length)
            {
                break;
            }

            int idx = Random.Range(0, enemies.Length);
            int safety = 0;
            while (used.Contains(idx) && safety < 10)
            {
                idx = Random.Range(0, enemies.Length);
                safety++;
            }

            used.Add(idx);
            var enemy = enemies[idx];
            if (enemy == null)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(_projectileDamage * _lightning.DamageMult);
            }

            SpawnLightningEffect(enemy.transform.position);
        }
    }

    private void FireDrone()
    {
        int count = Mathf.Max(1, _drone.BonusCount + 1);
        float radius = Mathf.Max(0.4f, droneOrbitRadius * _drone.RangeMult);
        float speed = droneAngularSpeed;
        float lifetime = Mathf.Max(0.2f, droneLifetime);
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f / count) * i;
            SpawnDroneProjectile(radius, speed, _projectileDamage * _drone.DamageMult * droneDamageMult, lifetime, angle);
        }
    }

    private void FireShuriken(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _shuriken.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        int count = GetWeaponCount(_shuriken);
        if (count <= 1)
        {
            SpawnColoredProjectile(direction, _projectileDamage * _shuriken.DamageMult * shurikenDamageMult, shurikenColor, shurikenSpinSpeed, lifetime, _projectileSpeed * shurikenSpeedMult);
            _range = savedRange;
            return;
        }

        Vector2 perp = new Vector2(-direction.y, direction.x);
        float start = -(count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float offsetIndex = start + i;
            Vector2 spawnOffset = perp * (offsetIndex * straightParallelSpacing);
            SpawnColoredProjectile(direction, _projectileDamage * _shuriken.DamageMult * shurikenDamageMult, shurikenColor, shurikenSpinSpeed, lifetime, _projectileSpeed * shurikenSpeedMult, spawnOffset);
        }

        _range = savedRange;
    }

    private void FireFrostOrb(Vector2 direction)
    {
        float savedRange = _range;
        _range *= _frost.RangeMult;
        float lifetime = CalculateLifetimeForRange(_range);

        int count = GetWeaponCount(_frost);
        for (int i = 0; i < count; i++)
        {
            var proj = SpawnColoredProjectile(direction, _projectileDamage * _frost.DamageMult * frostDamageMult, frostColor, 360f, lifetime, _projectileSpeed * frostSpeedMult);
            if (proj != null)
            {
                proj.SetSlowEffect(frostSlowMultiplier, frostSlowDuration);
            }
        }

        _range = savedRange;
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed = 0f, float lifetimeOverride = -1f)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, Vector2.zero);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, spawnOffset, _projectileSpeed);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset, float speedOverride)
    {
        var go = new GameObject("Projectile");
        go.transform.position = transform.position + (Vector3)spawnOffset;
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
        proj.Initialize(direction, speedOverride, damageOverride, life, _projectilePierce, spinSpeed);
    }

    private void SpawnNovaProjectile(Vector2 direction, float lifetime, float rotationSign)
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
        float angularSpeed = Mathf.Max(0.1f, novaOrbitAngularSpeed) * rotationSign;
        proj.InitializeOrbit(transform.position, direction, _projectileSpeed, angularSpeed, _projectileDamage * _nova.DamageMult, lifetime, _projectilePierce, 720f);
    }

    private void SpawnLaserProjectile(Vector2 direction, float damageOverride, float lifetime, Vector2 spawnOffset, float speedOverride)
    {
        var go = new GameObject("Laser");
        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.localScale = new Vector3(laserLengthScale, laserThickness, 1f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite();
        renderer.color = laserColor;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;

        var proj = go.AddComponent<Projectile>();
        proj.Initialize(direction, speedOverride, damageOverride, lifetime, _projectilePierce, 0f);
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride)
    {
        return SpawnColoredProjectile(direction, damageOverride, color, spinSpeed, lifetime, speedOverride, Vector2.zero);
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride, Vector2 spawnOffset)
    {
        var go = new GameObject("Projectile");
        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(_projectileSize);
        renderer.color = color;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.AddComponent<Projectile>();
        proj.Initialize(direction, speedOverride, damageOverride, lifetime, _projectilePierce, spinSpeed);
        return proj;
    }

    private void SpawnDroneProjectile(float radius, float angularSpeed, float damageOverride, float lifetime, float startAngle)
    {
        var go = new GameObject("Drone");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(_projectileSize);
        renderer.color = droneColor;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        var drone = go.AddComponent<DroneProjectile>();
        drone.Initialize(transform, radius, angularSpeed, damageOverride, lifetime, startAngle);
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

    private int GetWeaponCount(WeaponConfig config)
    {
        return Mathf.Max(1, _projectileCount + config.BonusCount);
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

    private static Sprite _solidSprite;

    private static Sprite CreateSolidSprite()
    {
        if (_solidSprite != null)
        {
            return _solidSprite;
        }

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _solidSprite;
    }

    private static Transform FindClosestEnemyToPoint(Vector3 position, EnemyController[] enemies, System.Collections.Generic.HashSet<Health> hit, float range)
    {
        float best = range * range;
        Transform bestTarget = null;
        if (enemies == null)
        {
            return null;
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null && hit != null && hit.Contains(health))
            {
                continue;
            }

            float dist = (enemy.transform.position - position).sqrMagnitude;
            if (dist <= best)
            {
                best = dist;
                bestTarget = enemy.transform;
            }
        }

        return bestTarget;
    }

    private void SpawnChainEffect(System.Collections.Generic.List<Vector3> points)
    {
        if (points == null || points.Count < 2)
        {
            return;
        }

        var go = new GameObject("ChainLightning");
        var line = go.AddComponent<LineRenderer>();
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.startWidth = chainLineWidth;
        line.endWidth = chainLineWidth;
        line.sortingOrder = 2000;
        line.material = GetChainMaterial();
        line.startColor = chainColor;
        line.endColor = chainColor;

        Destroy(go, chainEffectDuration);
    }

    private void SpawnLightningEffect(Vector3 position)
    {
        var go = new GameObject("LightningStrike");
        var line = go.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = lightningLineWidth;
        line.endWidth = lightningLineWidth * 0.6f;
        line.sortingOrder = 2200;
        line.material = GetChainMaterial();
        line.startColor = lightningColor;
        line.endColor = lightningColor;

        Vector3 top = position + Vector3.up * lightningLineLength;
        line.SetPosition(0, top);
        line.SetPosition(1, position);

        Destroy(go, lightningEffectDuration);
    }

    private static Material _chainMaterial;

    private static Material GetChainMaterial()
    {
        if (_chainMaterial != null)
        {
            return _chainMaterial;
        }

        _chainMaterial = new Material(Shader.Find("Sprites/Default"));
        return _chainMaterial;
    }
}

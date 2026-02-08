using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

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

    [Header("Sprites (Resources)")]
    [SerializeField]
    private string straightSpritePath;

    [SerializeField]
    private string boomerangSpritePath;

    [SerializeField]
    private string novaSpritePath;

    [SerializeField]
    private string shotgunSpritePath;

    [SerializeField]
    private string droneSpritePath;

    [SerializeField]
    private string shurikenSpritePath;

    [SerializeField]
    private string frostSpritePath;

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
    private float _nextTargetScanTime;
    private Transform _cachedTarget;
    private Vector2 _cachedDir;

    private static readonly System.Collections.Generic.Stack<GameObject> _circleProjectilePool = new System.Collections.Generic.Stack<GameObject>();
    private static readonly System.Collections.Generic.Stack<GameObject> _laserProjectilePool = new System.Collections.Generic.Stack<GameObject>();
    private static readonly System.Collections.Generic.Stack<GameObject> _boomerangPool = new System.Collections.Generic.Stack<GameObject>();
    private static readonly System.Collections.Generic.Stack<GameObject> _dronePool = new System.Collections.Generic.Stack<GameObject>();

    private const float TargetScanInterval = 0.1f;

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
    private bool _settingsApplied;

    private void Awake()
    {
        ApplySettings();
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

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.autoAttack;

        baseFireInterval = settings.baseFireInterval;
        baseProjectileSpeed = settings.baseProjectileSpeed;
        baseProjectileDamage = settings.baseProjectileDamage;
        baseRange = settings.baseRange;
        baseProjectileSize = settings.baseProjectileSize;
        baseProjectileLifetime = settings.baseProjectileLifetime;
        baseProjectilePierce = settings.baseProjectilePierce;
        straightParallelSpacing = settings.straightParallelSpacing;
        novaOrbitAngularSpeed = settings.novaOrbitAngularSpeed;

        shotgunBasePellets = settings.shotgunBasePellets;
        shotgunSpreadAngle = settings.shotgunSpreadAngle;
        shotgunPelletDamageMult = settings.shotgunPelletDamageMult;
        shotgunSpeedMult = settings.shotgunSpeedMult;

        laserSpeedMult = settings.laserSpeedMult;
        laserThickness = settings.laserThickness;
        laserLengthScale = settings.laserLengthScale;
        laserParallelSpacing = settings.laserParallelSpacing;
        laserColor = settings.laserColor;

        chainBaseJumps = settings.chainBaseJumps;
        chainJumpRangeMult = settings.chainJumpRangeMult;
        chainLineWidth = settings.chainLineWidth;
        chainEffectDuration = settings.chainEffectDuration;
        chainColor = settings.chainColor;

        droneOrbitRadius = settings.droneOrbitRadius;
        droneAngularSpeed = settings.droneAngularSpeed;
        droneLifetime = settings.droneLifetime;
        droneDamageMult = settings.droneDamageMult;
        droneColor = settings.droneColor;

        shurikenSpeedMult = settings.shurikenSpeedMult;
        shurikenSpinSpeed = settings.shurikenSpinSpeed;
        shurikenDamageMult = settings.shurikenDamageMult;
        shurikenColor = settings.shurikenColor;

        frostSpeedMult = settings.frostSpeedMult;
        frostDamageMult = settings.frostDamageMult;
        frostSlowMultiplier = settings.frostSlowMultiplier;
        frostSlowDuration = settings.frostSlowDuration;
        frostColor = settings.frostColor;

        lightningEffectDuration = settings.lightningEffectDuration;
        lightningLineWidth = settings.lightningLineWidth;
        lightningLineLength = settings.lightningLineLength;
        lightningColor = settings.lightningColor;

        straightSpritePath = settings.straightSpritePath;
        boomerangSpritePath = settings.boomerangSpritePath;
        novaSpritePath = settings.novaSpritePath;
        shotgunSpritePath = settings.shotgunSpritePath;
        droneSpritePath = settings.droneSpritePath;
        shurikenSpritePath = settings.shurikenSpritePath;
        frostSpritePath = settings.frostSpritePath;

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

        bool needsTarget = _straight.Enabled || _boomerang.Enabled || _shotgun.Enabled || _laser.Enabled || _chain.Enabled || _shuriken.Enabled || _frost.Enabled;
        Transform target = null;
        Vector2 dir = Vector2.zero;

        if (needsTarget)
        {
            if (_cachedTarget != null)
            {
                var cachedEnemy = _cachedTarget.GetComponent<EnemyController>();
                if (cachedEnemy != null && cachedEnemy.IsDead)
                {
                    _cachedTarget = null;
                }
            }

            if (Time.time >= _nextTargetScanTime || _cachedTarget == null)
            {
                _cachedTarget = FindClosestEnemy();
                _nextTargetScanTime = Time.time + TargetScanInterval;
            }

            target = _cachedTarget;
            if (target != null)
            {
                dir = (target.position - transform.position);
                _cachedDir = dir;
            }
            else
            {
                dir = _cachedDir;
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
        var enemies = EnemyController.Active;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }
            if (enemy.IsDead)
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
            SpawnProjectile(direction, _projectileDamage * _straight.DamageMult, 0f, lifetime, straightSpritePath);
            _range = savedRange;
            return;
        }

        Vector2 perp = new Vector2(-direction.y, direction.x);
        float start = -(count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float offsetIndex = start + i;
            Vector2 spawnOffset = perp * (offsetIndex * straightParallelSpacing);
            SpawnProjectile(direction, _projectileDamage * _straight.DamageMult, 0f, lifetime, spawnOffset, straightSpritePath);
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
            SpawnProjectile(dir, pelletDamage, 0f, lifetime, Vector2.zero, speed, shotgunSpritePath);
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

        var enemies = EnemyController.Active;
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
        var enemies = EnemyController.Active;
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }

        int strikes = Mathf.Max(1, 1 + _lightning.BonusCount);
        var used = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < strikes; i++)
        {
            if (used.Count >= enemies.Count)
            {
                break;
            }

            int idx = Random.Range(0, enemies.Count);
            int safety = 0;
            while (used.Contains(idx) && safety < 10)
            {
                idx = Random.Range(0, enemies.Count);
                safety++;
            }

            used.Add(idx);
            var enemy = enemies[idx];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null && !health.IsDead)
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
            SpawnColoredProjectile(direction, _projectileDamage * _shuriken.DamageMult * shurikenDamageMult, shurikenColor, shurikenSpinSpeed, lifetime, _projectileSpeed * shurikenSpeedMult, shurikenSpritePath, spawnOffset);
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
            var proj = SpawnColoredProjectile(direction, _projectileDamage * _frost.DamageMult * frostDamageMult, frostColor, 360f, lifetime, _projectileSpeed * frostSpeedMult, frostSpritePath);
            if (proj != null)
            {
                proj.SetSlowEffect(frostSlowMultiplier, frostSlowDuration);
            }
        }

        _range = savedRange;
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, string spritePath)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, Vector2.zero, _projectileSpeed, spritePath);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset, string spritePath)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, spawnOffset, _projectileSpeed, spritePath);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset, float speedOverride, string spritePath)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateProjectile();
        }
        else
        {
            go = GetPooledObject(_circleProjectilePool, "Projectile");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = ResolveProjectileSprite(spritePath, _projectileSize);
        var baseColor = new Color(0.9f, 0.9f, 0.2f, 1f);
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(baseColor);
            netColor.SetSpritePath(spritePath);
        }
        else
        {
            renderer.color = baseColor;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        float life = lifetimeOverride > 0f ? lifetimeOverride : _projectileLifetime;
        proj.Initialize(direction, speedOverride, damageOverride, life, _projectilePierce, spinSpeed);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));

        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private void SpawnNovaProjectile(Vector2 direction, float lifetime, float rotationSign)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateProjectile();
        }
        else
        {
            go = GetPooledObject(_circleProjectilePool, "NovaProjectile");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = ResolveProjectileSprite(novaSpritePath, _projectileSize);
        var novaColor = new Color(0.6f, 0.8f, 1f, 1f);
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(novaColor);
            netColor.SetSpritePath(novaSpritePath);
        }
        else
        {
            renderer.color = novaColor;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        float angularSpeed = Mathf.Max(0.1f, novaOrbitAngularSpeed) * rotationSign;
        proj.InitializeOrbit(transform.position, direction, _projectileSpeed, angularSpeed, _projectileDamage * _nova.DamageMult, lifetime, _projectilePierce, 720f);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private void SpawnLaserProjectile(Vector2 direction, float damageOverride, float lifetime, Vector2 spawnOffset, float speedOverride)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateLaser();
        }
        else
        {
            go = GetPooledObject(_laserProjectilePool, "Laser");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.localScale = new Vector3(laserLengthScale, laserThickness, 1f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = CreateSolidSprite();
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(laserColor);
        }
        else
        {
            renderer.color = laserColor;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        col.size = Vector2.one;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        proj.Initialize(direction, speedOverride, damageOverride, lifetime, _projectilePierce, 0f);
        proj.SetRelease(p => ReleaseProjectile(p, _laserProjectilePool));
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride, string spritePath)
    {
        return SpawnColoredProjectile(direction, damageOverride, color, spinSpeed, lifetime, speedOverride, spritePath, Vector2.zero);
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride, string spritePath, Vector2 spawnOffset)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return null;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateProjectile();
        }
        else
        {
            go = GetPooledObject(_circleProjectilePool, "Projectile");
        }

        if (go == null)
        {
            return null;
        }

        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = ResolveProjectileSprite(spritePath, _projectileSize);
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(color);
            netColor.SetSpritePath(spritePath);
        }
        else
        {
            renderer.color = color;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        proj.Initialize(direction, speedOverride, damageOverride, lifetime, _projectilePierce, spinSpeed);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));
        if (networked)
        {
            SpawnNetworkObject(go);
        }
        return proj;
    }

    private void SpawnDroneProjectile(float radius, float angularSpeed, float damageOverride, float lifetime, float startAngle)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateDrone();
        }
        else
        {
            go = GetPooledObject(_dronePool, "Drone");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = ResolveProjectileSprite(droneSpritePath, _projectileSize);
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(droneColor);
            netColor.SetSpritePath(droneSpritePath);
        }
        else
        {
            renderer.color = droneColor;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.45f;

        var drone = go.GetComponent<DroneProjectile>();
        if (drone == null)
        {
            drone = go.AddComponent<DroneProjectile>();
        }
        drone.Initialize(transform, radius, angularSpeed, damageOverride, lifetime, startAngle);
        drone.SetRelease(d => ReleaseDrone(d));
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private void SpawnBoomerang(Vector2 direction, float damageOverride, float lifetime)
    {
        bool networked = NetworkSession.IsActive;
        if (networked && !NetworkSession.IsServer)
        {
            return;
        }

        GameObject go = null;
        if (networked)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            go = RuntimeNetworkPrefabs.InstantiateBoomerang();
        }
        else
        {
            go = GetPooledObject(_boomerangPool, "Boomerang");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.45f;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = ResolveProjectileSprite(boomerangSpritePath, _projectileSize);
        var boomColor = new Color(0.2f, 0.9f, 0.9f, 1f);
        var netColor = go.GetComponent<NetworkColor>();
        if (netColor != null)
        {
            netColor.SetColor(boomColor);
            netColor.SetSpritePath(boomerangSpritePath);
        }
        else
        {
            renderer.color = boomColor;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        var boom = go.GetComponent<BoomerangProjectile>();
        if (boom == null)
        {
            boom = go.AddComponent<BoomerangProjectile>();
        }
        boom.Initialize(transform, direction, _projectileSpeed, damageOverride, lifetime, 9999);
        boom.SetRelease(b => ReleaseBoomerang(b));
        if (networked)
        {
            SpawnNetworkObject(go);
        }
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

    private static readonly System.Collections.Generic.Dictionary<int, Sprite> _circleCache = new System.Collections.Generic.Dictionary<int, Sprite>();
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _resourceSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();

    private static Sprite CreateCircleSprite(int size)
    {
        if (size <= 0)
        {
            size = 1;
        }

        if (_circleCache.TryGetValue(size, out var cached) && cached != null)
        {
            return cached;
        }

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
        var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _circleCache[size] = sprite;
        return sprite;
    }

    private static Sprite LoadResourceSprite(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (_resourceSpriteCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var sprite = Resources.Load<Sprite>(path);
        _resourceSpriteCache[path] = sprite;
        return sprite;
    }

    private static Sprite ResolveProjectileSprite(string path, int fallbackSize)
    {
        var sprite = LoadResourceSprite(path);
        return sprite != null ? sprite : CreateCircleSprite(fallbackSize);
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

    private static void SpawnNetworkObject(GameObject go)
    {
        if (!NetworkSession.IsActive || go == null)
        {
            return;
        }

        var netObj = go.GetComponent<Unity.Netcode.NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn();
        }
    }

    private static void DespawnNetworkObject(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            var netObj = go.GetComponent<Unity.Netcode.NetworkObject>();
            if (NetworkSession.IsServer && netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(go);
            }
            return;
        }

        Destroy(go);
    }

    private static void ReleaseProjectile(Projectile projectile, System.Collections.Generic.Stack<GameObject> pool)
    {
        if (projectile == null)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            DespawnNetworkObject(projectile.gameObject);
        }
        else
        {
            ReturnToPool(pool, projectile.gameObject);
        }
    }

    private static void ReleaseBoomerang(BoomerangProjectile boomerang)
    {
        if (boomerang == null)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            DespawnNetworkObject(boomerang.gameObject);
        }
        else
        {
            ReturnToPool(_boomerangPool, boomerang.gameObject);
        }
    }

    private static void ReleaseDrone(DroneProjectile drone)
    {
        if (drone == null)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            DespawnNetworkObject(drone.gameObject);
        }
        else
        {
            ReturnToPool(_dronePool, drone.gameObject);
        }
    }

    private static GameObject GetPooledObject(System.Collections.Generic.Stack<GameObject> pool, string name)
    {
        GameObject go = null;
        while (pool.Count > 0 && go == null)
        {
            go = pool.Pop();
        }

        if (go == null)
        {
            go = new GameObject(name);
        }
        else
        {
            go.name = name;
        }

        go.SetActive(true);
        return go;
    }

    private static void ReturnToPool(System.Collections.Generic.Stack<GameObject> pool, GameObject go)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(false);
        pool.Push(go);
    }

    private static Transform FindClosestEnemyToPoint(Vector3 position, System.Collections.Generic.IList<EnemyController> enemies, System.Collections.Generic.HashSet<Health> hit, float range)
    {
        float best = range * range;
        Transform bestTarget = null;
        if (enemies == null)
        {
            return null;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }
            if (enemy.IsDead)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null && hit != null && hit.Contains(health))
            {
                continue;
            }
            if (health != null && health.IsDead)
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

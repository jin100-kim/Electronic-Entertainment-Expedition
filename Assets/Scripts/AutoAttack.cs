using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    public enum WeaponType
    {
        SingleShot,
        MultiShot,
        PiercingShot,
        Aura,
        HomingShot,
        Grenade,
        Melee
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
    private float singleShotParallelSpacing = 0.35f;

    [SerializeField]
    private float straightBurstShotInterval = 0.06f;

    [SerializeField]
    private float radialSpreadStepAngle = 15f;

    [SerializeField]
    private float pierceBurstShotInterval = 0.06f;

    [SerializeField]
    private int pierceBaseCount = 10;

    [SerializeField]
    private float auraDamageTickMult = 1f;

    [SerializeField]
    private float homingTurnSpeed = 720f;

    [SerializeField]
    private float homingRetargetRangeMult = 1.2f;

    [SerializeField]
    private float thrownTravelSpeed = 10f;

    [SerializeField]
    private float thrownExplosionRadius = 1.8f;

    [SerializeField]
    private float grenadeArcHeight = 0.75f;

    [SerializeField]
    private Color auraIndicatorColor = new Color(0.2f, 0.9f, 0.5f, 0.2f);

    [SerializeField]
    private int auraIndicatorSortingOrder = -2;

    [SerializeField]
    private Color grenadeTargetIndicatorColor = new Color(1f, 0.4f, 0.2f, 0.3f);

    [SerializeField]
    private int grenadeIndicatorSortingOrder = 20;

    [SerializeField]
    private Color grenadeProjectileColor = new Color(1f, 0.8f, 0.3f, 0.9f);

    [SerializeField]
    private float meleeConeAngle = 120f;

    [SerializeField]
    private float piercingShotOrbitAngularSpeed = 8f;

    [Header("Aura")]
    [SerializeField]
    private int auraBasePellets = 5;

    [SerializeField]
    private float auraSpreadAngle = 32f;

    [SerializeField]
    private float auraPelletDamageMult = 0.75f;

    [SerializeField]
    private float auraSpeedMult = 0.95f;

    [Header("Homing Shot")]
    [SerializeField]
    private float homingShotSpeedMult = 1.8f;

    [SerializeField]
    private float homingShotThickness = 0.12f;

    [SerializeField]
    private float homingShotLengthScale = 1.4f;

    [SerializeField]
    private float homingShotParallelSpacing = 0.3f;

    [SerializeField]
    private Color homingShotColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Grenade")]
    [SerializeField]
    private int grenadeBaseJumps = 3;

    [SerializeField]
    private float grenadeJumpRangeMult = 0.7f;

    [SerializeField]
    private float grenadeLineWidth = 0.12f;

    [SerializeField]
    private float grenadeEffectDuration = 0.12f;

    [SerializeField]
    private Color grenadeColor = new Color(0.5f, 0.8f, 1f, 1f);

    [Header("Melee")]
    [SerializeField]
    private float meleeEffectDuration = 0.12f;

    [SerializeField]
    private float meleeLineWidth = 0.14f;

    [SerializeField]
    private float meleeLineLength = 1.6f;

    [SerializeField]
    private Color meleeColor = new Color(1f, 0.95f, 0.5f, 1f);

    [Header("Sprites (Resources)")]
    [SerializeField]
    private string singleShotSpritePath;

    [SerializeField]
    private string multiShotSpritePath;

    [SerializeField]
    private string piercingShotSpritePath;

    [SerializeField]
    private string auraSpritePath;

    [SerializeField]
    private string homingShotSpritePath;

    [SerializeField]
    private string grenadeSpritePath;

    [SerializeField]
    private float projectileSpriteScale = 2.5f;

    [SerializeField]
    private float piercingShotVisualScaleMult = 2f;

    [SerializeField]
    private float directionalProjectileBaseAngle = 45f;

    [Header("Combat VFX")]
    [SerializeField]
    private float muzzleFxDuration = 0.08f;

    [SerializeField]
    private float auraPulseDuration = 0.2f;

    [SerializeField]
    private float grenadeExplosionFxDuration = 0.38f;

    [SerializeField]
    private int combatFxSortingOrder = 2200;

    [SerializeField]
    private Color muzzleFxColor = new Color(1f, 0.95f, 0.7f, 0.9f);

    [SerializeField]
    private Color auraPulseColor = new Color(0.45f, 1f, 0.75f, 0.3f);

    [SerializeField]
    private Color grenadeExplosionColor = new Color(1f, 0.75f, 0.35f, 0.55f);

    [SerializeField]
    private Color meleeSlashColor = new Color(1f, 0.95f, 0.8f, 0.95f);

    [SerializeField]
    private string meleeSlashSpritePath = "Art/Items/icon_weapon_melee";

    [SerializeField]
    private float meleeSwordVisualScale = 3f;

    [SerializeField]
    private float meleeSwordSpriteAngleOffset = 135f;

    [SerializeField]
    private bool showMeleeHitboxGizmo = true;

    [SerializeField]
    private string auraPulseSpritePath = "Art/Projectiles/projectile_aura";

    [SerializeField]
    private string grenadeExplosionSpritePath = "Art/Projectiles/projectile_grenade";

    [SerializeField]
    private string muzzleSpritePath = "Art/Projectiles/projectile_single_shot";

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
    private float _attackAreaMult = 1f;
    private float _nextTargetScanTime;
    private Transform _cachedTarget;
    private Vector2 _cachedDir;
    private ElementLoadout _elementLoadout;
    private PlayerController _ownerPlayer;

    private static readonly System.Collections.Generic.Stack<GameObject> _circleProjectilePool = new System.Collections.Generic.Stack<GameObject>();
    private static readonly System.Collections.Generic.Stack<GameObject> _homingShotProjectilePool = new System.Collections.Generic.Stack<GameObject>();
    private static readonly System.Collections.Generic.Stack<GameObject> _multiShotPool = new System.Collections.Generic.Stack<GameObject>();
    private const float TargetScanInterval = 0.1f;
    private const byte WeaponIdNone = 255;

    private static byte ToWeaponId(WeaponType type) => (byte)type;

    private struct WeaponConfig
    {
        public bool Enabled;
        public float DamageMult;
        public float FireRateMult;
        public float RangeMult;
        public float AreaMult;
        public int BonusCount;
        public float HitStunDuration;
        public float KnockbackDistance;
    }

    private WeaponConfig _singleShot;
    private WeaponConfig _multiShot;
    private WeaponConfig _piercingShot;
    private WeaponConfig _aura;
    private WeaponConfig _homingShot;
    private WeaponConfig _grenade;
    private WeaponConfig _melee;

    private float _nextFireSingleShot;
    private float _nextFireMultiShot;
    private float _nextFirePiercingShot;
    private float _nextFireAura;
    private float _nextFireHomingShot;
    private float _nextFireGrenade;
    private float _nextFireMelee;
    private bool _settingsApplied;
    private GameObject _auraIndicator;
    private SpriteRenderer _auraIndicatorRenderer;
    private Vector2 _meleeGizmoCenter;
    private Vector2 _meleeGizmoSize;
    private float _meleeGizmoAngle;
    private float _meleeGizmoExpireTime;
    private float _meleeHorizontalFacingSign = 1f;

    private void Awake()
    {
        ApplySettings();
        ApplyStats(1f, 1f, 1f, 1f, 1f, 1f, 1, 0, 1f);
        _elementLoadout = GetComponent<ElementLoadout>();
        _ownerPlayer = GetComponent<PlayerController>();
        _singleShot = CreateDefaultConfig(true);
        _multiShot = CreateDefaultConfig(false);
        _piercingShot = CreateDefaultConfig(false);
        _aura = CreateDefaultConfig(false);
        _homingShot = CreateDefaultConfig(false);
        _grenade = CreateDefaultConfig(false);
        _melee = CreateDefaultConfig(false);
    }

    private void OnDisable()
    {
        if (_auraIndicator != null)
        {
            Destroy(_auraIndicator);
            _auraIndicator = null;
            _auraIndicatorRenderer = null;
        }

        _meleeGizmoExpireTime = 0f;
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
        singleShotParallelSpacing = settings.singleShotParallelSpacing;
        piercingShotOrbitAngularSpeed = settings.piercingShotOrbitAngularSpeed;

        auraBasePellets = settings.auraBasePellets;
        auraSpreadAngle = settings.auraSpreadAngle;
        auraPelletDamageMult = settings.auraPelletDamageMult;
        auraSpeedMult = settings.auraSpeedMult;

        homingShotSpeedMult = settings.homingShotSpeedMult;
        homingShotThickness = settings.homingShotThickness;
        homingShotLengthScale = settings.homingShotLengthScale;
        homingShotParallelSpacing = settings.homingShotParallelSpacing;
        homingShotColor = settings.homingShotColor;

        grenadeBaseJumps = settings.grenadeBaseJumps;
        grenadeJumpRangeMult = settings.grenadeJumpRangeMult;
        grenadeLineWidth = settings.grenadeLineWidth;
        grenadeEffectDuration = settings.grenadeEffectDuration;
        grenadeColor = settings.grenadeColor;


        meleeEffectDuration = settings.meleeEffectDuration;
        meleeLineWidth = settings.meleeLineWidth;
        meleeLineLength = settings.meleeLineLength;
        meleeColor = settings.meleeColor;
        meleeConeAngle = Mathf.Clamp(settings.meleeConeAngle, 10f, 180f);
        meleeSwordVisualScale = settings.meleeSwordVisualScale;
        meleeSwordSpriteAngleOffset = settings.meleeSwordSpriteAngleOffset;

        singleShotSpritePath = settings.singleShotSpritePath;
        multiShotSpritePath = settings.multiShotSpritePath;
        piercingShotSpritePath = settings.piercingShotSpritePath;
        auraSpritePath = settings.auraSpritePath;
        homingShotSpritePath = settings.homingShotSpritePath;
        grenadeSpritePath = settings.grenadeSpritePath;
        projectileSpriteScale = settings.projectileSpriteScale;
        piercingShotVisualScaleMult = settings.piercingShotVisualScaleMult;

        _settingsApplied = true;
    }

    private void Update()
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            UpdateAuraIndicator(forceHide: true);
            return;
        }

        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            UpdateAuraIndicator(forceHide: true);
            return;
        }

        if (GameSession.Instance != null && !GameSession.Instance.IsGameplayActive)
        {
            UpdateAuraIndicator(forceHide: true);
            return;
        }

        UpdateAuraIndicator(forceHide: false);

        bool needsTarget = _singleShot.Enabled || _multiShot.Enabled || _piercingShot.Enabled || _homingShot.Enabled;
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

        if (_singleShot.Enabled && Time.time >= _nextFireSingleShot && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireSingleShot(dir.normalized);
            _nextFireSingleShot = Time.time + GetIntervalForWeapon(_singleShot.FireRateMult);
        }

        if (_multiShot.Enabled && Time.time >= _nextFireMultiShot && target != null && dir.sqrMagnitude > 0.0001f)
        {
            FireMultiShot(dir.normalized);
            _nextFireMultiShot = Time.time + GetIntervalForWeapon(_multiShot.FireRateMult);
        }

        if (_piercingShot.Enabled && Time.time >= _nextFirePiercingShot)
        {
            FirePiercingShot();
            _nextFirePiercingShot = Time.time + GetIntervalForWeapon(_piercingShot.FireRateMult);
        }

        if (_aura.Enabled && Time.time >= _nextFireAura)
        {
            FireAura();
            _nextFireAura = Time.time + GetIntervalForWeapon(_aura.FireRateMult);
        }

        if (_homingShot.Enabled && Time.time >= _nextFireHomingShot)
        {
            FireHomingShot(dir.normalized);
            _nextFireHomingShot = Time.time + GetIntervalForWeapon(_homingShot.FireRateMult);
        }

        if (_grenade.Enabled && Time.time >= _nextFireGrenade)
        {
            FireGrenade();
            _nextFireGrenade = Time.time + GetIntervalForWeapon(_grenade.FireRateMult);
        }

        if (_melee.Enabled && Time.time >= _nextFireMelee)
        {
            FireMelee();
            _nextFireMelee = Time.time + GetIntervalForWeapon(_melee.FireRateMult);
        }
    }

    public void ApplyStats(float damageMult, float fireRateMult, float rangeMult, float sizeMult, float attackAreaMult, float lifetimeMult, int projectileCount, int pierceBonus, float weaponDamageMult)
    {
        _weaponDamageMult = Mathf.Max(0.1f, weaponDamageMult);
        _projectileDamage = baseProjectileDamage * Mathf.Max(0.1f, damageMult) * _weaponDamageMult;
        _baseInterval = Mathf.Max(0.05f, baseFireInterval / Mathf.Max(0.1f, fireRateMult));
        _fireInterval = _baseInterval;
        _range = baseRange * Mathf.Max(0.1f, rangeMult);
        _projectileSpeed = baseProjectileSpeed;
        _projectileSize = Mathf.Max(2, Mathf.RoundToInt(baseProjectileSize * Mathf.Max(0.2f, sizeMult)));
        _attackAreaMult = Mathf.Max(0.1f, attackAreaMult);
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
            case WeaponType.MultiShot:
                _multiShot = cfg;
                break;
            case WeaponType.PiercingShot:
                _piercingShot = cfg;
                break;
            case WeaponType.Aura:
                _aura = cfg;
                break;
            case WeaponType.HomingShot:
                _homingShot = cfg;
                break;
            case WeaponType.Grenade:
                _grenade = cfg;
                break;
            case WeaponType.Melee:
                _melee = cfg;
                break;
            default:
                _singleShot = cfg;
                break;
        }
    }

    private WeaponConfig GetWeaponConfig(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.MultiShot:
                return _multiShot;
            case WeaponType.PiercingShot:
                return _piercingShot;
            case WeaponType.Aura:
                return _aura;
            case WeaponType.HomingShot:
                return _homingShot;
            case WeaponType.Grenade:
                return _grenade;
            case WeaponType.Melee:
                return _melee;
            default:
                return _singleShot;
        }
    }

    private void ApplyHitReactionToProjectile(Projectile projectile, WeaponType type)
    {
        if (projectile == null)
        {
            return;
        }

        var cfg = GetWeaponConfig(type);
        if (cfg.HitStunDuration <= 0f && cfg.KnockbackDistance <= 0f)
        {
            return;
        }

        projectile.SetHitReaction(cfg.KnockbackDistance, cfg.HitStunDuration);
    }

    private void ApplyHitReactionToBoomerang(BoomerangProjectile boomerang, WeaponType type)
    {
        if (boomerang == null)
        {
            return;
        }

        var cfg = GetWeaponConfig(type);
        if (cfg.HitStunDuration <= 0f && cfg.KnockbackDistance <= 0f)
        {
            return;
        }

        boomerang.SetHitReaction(cfg.KnockbackDistance, cfg.HitStunDuration);
    }

    private void ApplyHitReactionToTarget(Transform target, WeaponType type)
    {
        if (target == null)
        {
            return;
        }

        var cfg = GetWeaponConfig(type);
        if (cfg.HitStunDuration <= 0f && cfg.KnockbackDistance <= 0f)
        {
            return;
        }

        var enemy = target.GetComponent<EnemyController>();
        if (enemy == null)
        {
            return;
        }

        Vector2 dir = (Vector2)(target.position - transform.position);
        enemy.ApplyHitReaction(dir, cfg.KnockbackDistance, cfg.HitStunDuration);
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

    private void FireSingleShot(Vector2 direction)
    {
        float savedRange = _range;
        float weaponRange = _range * Mathf.Max(0.1f, _singleShot.RangeMult);
        _range = weaponRange;
        float lifetime = CalculateLifetimeForRange(weaponRange);
        byte weaponId = ToWeaponId(WeaponType.SingleShot);
        float damage = _projectileDamage * _singleShot.DamageMult;

        int count = GetWeaponCount(_singleShot);
        if (count <= 1)
        {
            SpawnProjectile(direction, damage, 0f, lifetime, singleShotSpritePath, weaponId, WeaponType.SingleShot);
            _range = savedRange;
            return;
        }

        StartCoroutine(FireSingleShotBurst(direction, damage, lifetime, weaponId, count));

        _range = savedRange;
    }

    private System.Collections.IEnumerator FireSingleShotBurst(Vector2 direction, float damage, float lifetime, byte weaponId, int count)
    {
        if (count <= 0)
        {
            yield break;
        }

        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        float interval = Mathf.Max(0f, straightBurstShotInterval);

        for (int i = 0; i < count; i++)
        {
            SpawnProjectile(dir, damage, 0f, lifetime, singleShotSpritePath, weaponId, WeaponType.SingleShot);

            if (i < count - 1 && interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private void FireMultiShot(Vector2 direction)
    {
        float savedRange = _range;
        float weaponRange = _range * Mathf.Max(0.1f, _multiShot.RangeMult);
        _range = weaponRange;
        float lifetime = CalculateLifetimeForRange(weaponRange);
        byte weaponId = ToWeaponId(WeaponType.MultiShot);
        float damage = _projectileDamage * _multiShot.DamageMult;

        int count = GetWeaponCount(_multiShot);
        float angleStep = Mathf.Max(1f, radialSpreadStepAngle);
        float start = -angleStep * (count - 1) * 0.5f;
        Vector2 baseDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : ResolveFacingDirection();
        for (int i = 0; i < count; i++)
        {
            float angle = start + angleStep * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDirection;
            SpawnProjectile(dir, damage, 0f, lifetime, multiShotSpritePath, weaponId, WeaponType.MultiShot);
        }

        _range = savedRange;
    }

    private void FirePiercingShot()
    {
        var target = FindLowestHealthRatioEnemy();
        if (target == null)
        {
            return;
        }

        float savedRange = _range;
        float weaponRange = _range * Mathf.Max(0.1f, _piercingShot.RangeMult);
        _range = weaponRange;
        float lifetime = CalculateLifetimeForRange(weaponRange);
        byte weaponId = ToWeaponId(WeaponType.PiercingShot);
        float damage = _projectileDamage * _piercingShot.DamageMult;
        int count = GetWeaponCount(_piercingShot);
        int pierceCount = Mathf.Max(_projectilePierce, Mathf.Max(0, pierceBaseCount));
        Vector2 direction = ((Vector2)(target.position - transform.position)).normalized;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = ResolveFacingDirection();
        }

        if (count <= 1)
        {
            SpawnProjectile(direction, damage, 0f, lifetime, piercingShotSpritePath, weaponId, WeaponType.PiercingShot, pierceCount);
        }
        else
        {
            StartCoroutine(FirePierceBurst(direction, damage, lifetime, weaponId, count, pierceCount));
        }

        _range = savedRange;
    }

    private System.Collections.IEnumerator FirePierceBurst(Vector2 direction, float damage, float lifetime, byte weaponId, int count, int pierceCount)
    {
        float interval = Mathf.Max(0f, pierceBurstShotInterval);
        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : ResolveFacingDirection();
        for (int i = 0; i < count; i++)
        {
            SpawnProjectile(dir, damage, 0f, lifetime, piercingShotSpritePath, weaponId, WeaponType.PiercingShot, pierceCount);
            if (i < count - 1 && interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private void FireAura()
    {
        float radius = GetAuraRadius();
        if (radius <= 0.1001f)
        {
            return;
        }
        float damage = _projectileDamage * _aura.DamageMult * auraDamageTickMult;
        int elementCount = GetWeaponElements(WeaponType.Aura, out var e0, out var e1, out var e2);
        var enemies = EnemyController.Active;
        Vector2 origin = transform.position;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!TryGetEnemyHealthWithinRadius(enemy, origin, radius, out var health, out _))
            {
                continue;
            }

            health.Damage(damage);
            ApplyElementsToTarget(enemy.transform, e0, e1, e2, elementCount);
            ApplyHitReactionToTarget(enemy.transform, WeaponType.Aura);
        }
        SpawnAuraPulseEffect(origin, radius);
    }

    private void UpdateAuraIndicator(bool forceHide)
    {
        bool shouldShow = !forceHide && _aura.Enabled;
        if (!shouldShow)
        {
            if (_auraIndicator != null)
            {
                _auraIndicator.SetActive(false);
            }
            return;
        }

        EnsureAuraIndicator();
        if (_auraIndicator == null)
        {
            return;
        }

        float radius = GetAuraRadius();
        if (radius <= 0.1001f)
        {
            _auraIndicator.SetActive(false);
            return;
        }
        float diameter = radius * 2f;
        _auraIndicator.transform.position = transform.position;
        _auraIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        _auraIndicator.SetActive(true);
    }

    private void EnsureAuraIndicator()
    {
        if (_auraIndicator != null)
        {
            return;
        }

        _auraIndicator = new GameObject("AuraRangeIndicator");
        _auraIndicatorRenderer = _auraIndicator.AddComponent<SpriteRenderer>();
        _auraIndicatorRenderer.sprite = CreateCircleSprite(128);
        _auraIndicatorRenderer.color = auraIndicatorColor;
        _auraIndicatorRenderer.sortingOrder = Mathf.Min(auraIndicatorSortingOrder, -2);
    }

    // Aura radius intentionally ignores range upgrades and only scales with aura area + attack-area upgrades.
    private float GetAuraRadius()
    {
        float auraArea = Mathf.Max(0f, _aura.AreaMult);
        return Mathf.Max(0.1f, baseRange * auraArea * _attackAreaMult);
    }

    private void FireHomingShot(Vector2 direction)
    {
        float savedRange = _range;
        float weaponRange = _range * Mathf.Max(0.1f, _homingShot.RangeMult);
        _range = weaponRange;
        float lifetime = CalculateLifetimeForRange(weaponRange);
        byte weaponId = ToWeaponId(WeaponType.HomingShot);
        float damage = _projectileDamage * _homingShot.DamageMult;
        float speed = _projectileSpeed * Mathf.Max(0.1f, homingShotSpeedMult);
        float retargetRange = Mathf.Max(1f, weaponRange * homingRetargetRangeMult);

        int count = GetWeaponCount(_homingShot);
        Vector2 baseDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : ResolveFacingDirection();
        float step = count <= 1 ? 0f : 360f / count;

        for (int i = 0; i < count; i++)
        {
            Vector2 shotDir = Quaternion.Euler(0f, 0f, step * i) * baseDir;
            var proj = SpawnColoredProjectile(shotDir, damage, homingShotColor, 0f, lifetime, speed, homingShotSpritePath, weaponId, Vector2.zero, 1);
            ApplyElementsToProjectile(proj, WeaponType.HomingShot);
            ApplyHitReactionToProjectile(proj, WeaponType.HomingShot);
            if (proj != null)
            {
                Transform preferred = FindClosestEnemyToPoint(transform.position, EnemyController.Active, null, retargetRange);
                proj.SetHoming(preferred, homingTurnSpeed, retargetRange);
            }
        }

        _range = savedRange;
    }

    private void FireGrenade()
    {
        float throwRange = _range * Mathf.Max(0.1f, _grenade.RangeMult);
        int count = GetWeaponCount(_grenade);
        float damage = _projectileDamage * _grenade.DamageMult;
        float radius = GetGrenadeExplosionRadius();
        if (radius <= 0.1001f)
        {
            return;
        }
        float speed = Mathf.Max(0.1f, thrownTravelSpeed);
        for (int i = 0; i < count; i++)
        {
            Transform target = FindRandomEnemy(throwRange);
            if (target == null)
            {
                continue;
            }

            Vector3 targetPos = target.position;
            float distance = Vector3.Distance(transform.position, targetPos);
            float travelTime = distance / speed;
            StartCoroutine(ThrownProjectileAndExplode(targetPos, travelTime, damage, radius));
        }

    }

    // Grenade explosion radius intentionally ignores range upgrades and only scales with grenade area + attack-area upgrades.
    private float GetGrenadeExplosionRadius()
    {
        float grenadeArea = Mathf.Max(0f, _grenade.AreaMult);
        return Mathf.Max(0.1f, thrownExplosionRadius * grenadeArea * _attackAreaMult);
    }

    private System.Collections.IEnumerator ThrownProjectileAndExplode(Vector3 targetPos, float delay, float damage, float radius)
    {
        Vector3 startPos = transform.position;
        float travelTime = Mathf.Max(0.02f, delay);
        var projectile = CreateGrenadeVisual(startPos);
        float spinSpeed = Random.Range(-540f, 540f);

        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            if (projectile != null)
            {
                Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
                float arc = grenadeArcHeight * 4f * t * (1f - t);
                pos.y += arc;
                projectile.transform.position = pos;
                projectile.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
            }

            yield return null;
        }

        if (projectile != null)
        {
            Destroy(projectile);
        }

        int elementCount = GetWeaponElements(WeaponType.Grenade, out var e0, out var e1, out var e2);
        var enemies = EnemyController.Active;
        Vector2 explosionCenter = targetPos;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!TryGetEnemyHealthWithinRadius(enemy, explosionCenter, radius, out var health, out _))
            {
                continue;
            }

            health.Damage(damage);
            ApplyElementsToTarget(enemy.transform, e0, e1, e2, elementCount);
            ApplyHitReactionToTarget(enemy.transform, WeaponType.Grenade);
        }
        SpawnGrenadeExplosionEffect(targetPos, radius);
    }

    private GameObject CreateGrenadeVisual(Vector3 position)
    {
        var go = new GameObject("GrenadeProjectileVisual");
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        bool hasSprite = TryResolveProjectileSprite(grenadeSpritePath, _projectileSize, out var sprite);
        renderer.sprite = sprite;
        renderer.color = ResolveSpriteTint(grenadeProjectileColor, hasSprite);
        renderer.sortingOrder = Mathf.Max(combatFxSortingOrder, grenadeIndicatorSortingOrder) + 1;
        go.transform.localScale = Vector3.one * (hasSprite ? projectileSpriteScale * 0.85f : 0.35f);
        return go;
    }

    private GameObject CreateAreaIndicator(string name, Vector3 position, float radius, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(128);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private void SpawnAuraPulseEffect(Vector2 origin, float radius)
    {
        float duration = Mathf.Max(0.06f, auraPulseDuration);
        var sprite = ResolveEffectSprite(auraPulseSpritePath, auraSpritePath, 96);
        int sorting = Mathf.Min(auraIndicatorSortingOrder - 1, -3);
        var go = CreateFxSprite("AuraPulseFx", origin, sprite, auraPulseColor, sorting, 0f);
        if (go == null)
        {
            return;
        }

        // Keep pulse near full aura size so it reads as a range pulse, not a center spark.
        float start = Mathf.Max(0.1f, radius * 0.9f);
        float end = Mathf.Max(start, radius * 1.03f);
        StartCoroutine(AnimateFxScaleAndFade(
            go,
            new Vector3(start * 2f, start * 2f, 1f),
            new Vector3(end * 2f, end * 2f, 1f),
            duration));
    }

    private void SpawnGrenadeExplosionEffect(Vector2 position, float radius)
    {
        float duration = Mathf.Max(0.12f, grenadeExplosionFxDuration);
        var baseSprite = ResolveEffectSprite(grenadeExplosionSpritePath, grenadeSpritePath, 96);

        int sortingBase = Mathf.Max(combatFxSortingOrder, grenadeIndicatorSortingOrder);
        var main = CreateFxSprite("GrenadeExplosionFx", position, baseSprite, grenadeExplosionColor, sortingBase + 2, 0f);
        if (main != null)
        {
            float start = Mathf.Max(0.12f, radius * 0.35f);
            float end = Mathf.Max(start + 0.05f, radius * 1.2f);
            StartCoroutine(AnimateFxScaleAndFade(
                main,
                new Vector3(start * 2f, start * 2f, 1f),
                new Vector3(end * 2f, end * 2f, 1f),
                duration));
        }

        var flashColor = new Color(1f, 1f, 0.92f, 0.8f);
        var flash = CreateFxSprite("GrenadeExplosionFlashFx", position, CreateCircleSprite(64), flashColor, sortingBase + 3, 0f);
        if (flash != null)
        {
            float start = Mathf.Max(0.08f, radius * 0.2f);
            float end = Mathf.Max(start + 0.05f, radius * 0.75f);
            StartCoroutine(AnimateFxScaleAndFade(
                flash,
                new Vector3(start * 2f, start * 2f, 1f),
                new Vector3(end * 2f, end * 2f, 1f),
                duration * 0.6f));
        }
    }

    private void SpawnProjectileMuzzleEffect(Vector2 position, Vector2 direction, Color tint, string preferredSpritePath)
    {
        float duration = Mathf.Max(0.03f, muzzleFxDuration);
        var sprite = ResolveEffectSprite(preferredSpritePath, muzzleSpritePath, 64);
        Vector2 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : ResolveFacingDirection();
        float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        var color = muzzleFxColor;
        color.r = Mathf.Clamp01((color.r + tint.r) * 0.5f);
        color.g = Mathf.Clamp01((color.g + tint.g) * 0.5f);
        color.b = Mathf.Clamp01((color.b + tint.b) * 0.5f);

        var go = CreateFxSprite("MuzzleFx", position, sprite, color, combatFxSortingOrder + 4, angle);
        if (go == null)
        {
            return;
        }

        float baseScale = Mathf.Max(0.18f, projectileSpriteScale * 0.36f);
        var fromScale = new Vector3(baseScale * 0.65f, baseScale * 0.45f, 1f);
        var toScale = new Vector3(baseScale * 1.2f, baseScale * 0.65f, 1f);
        StartCoroutine(AnimateFxScaleAndFade(go, fromScale, toScale, duration));
    }

    private GameObject CreateFxSprite(string name, Vector2 position, Sprite sprite, Color color, int sortingOrder, float angleDeg)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : CreateCircleSprite(64);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private System.Collections.IEnumerator AnimateFxScaleAndFade(GameObject go, Vector3 fromScale, Vector3 toScale, float duration)
    {
        if (go == null)
        {
            yield break;
        }

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Destroy(go);
            yield break;
        }

        float life = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        Color baseColor = renderer.color;
        go.transform.localScale = fromScale;

        while (elapsed < life && go != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / life);
            go.transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
            var c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0f, t);
            renderer.color = c;
            yield return null;
        }

        if (go != null)
        {
            Destroy(go);
        }
    }

    private Sprite ResolveEffectSprite(string primaryPath, string fallbackPath, int fallbackSize)
    {
        var sprite = LoadResourceSprite(primaryPath);
        if (sprite != null)
        {
            return sprite;
        }

        sprite = LoadResourceSprite(fallbackPath);
        if (sprite != null)
        {
            return sprite;
        }

        return CreateCircleSprite(fallbackSize);
    }

    private void FireMelee()
    {
        // Melee range intentionally ignores range upgrades and only scales with melee area + attack-area upgrades.
        float range = Mathf.Max(0.1f, baseRange * Mathf.Max(0f, _melee.AreaMult) * _attackAreaMult);
        if (range <= 0.1001f)
        {
            return;
        }

        Vector2 facing = ResolveMeleeFacingDirection();
        float halfAngle = Mathf.Clamp(meleeConeAngle, 10f, 180f) * 0.5f;
        float damage = _projectileDamage * _melee.DamageMult;
        int elementCount = GetWeaponElements(WeaponType.Melee, out var e0, out var e1, out var e2);
        StartCoroutine(PerformMeleeSwordSwing(facing, range, halfAngle, damage, elementCount, e0, e1, e2));

    }

    private System.Collections.IEnumerator PerformMeleeSwordSwing(
        Vector2 facing,
        float range,
        float halfAngle,
        float damage,
        int elementCount,
        ElementType e0,
        ElementType e1,
        ElementType e2)
    {
        float duration = Mathf.Max(0.08f, meleeEffectDuration * 1.35f);
        Vector2 forward = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector2.right;
        float facingAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        float startAngle = facingAngle - halfAngle;
        float endAngle = facingAngle + halfAngle;

        float reach = Mathf.Max(0.25f, range * 0.55f);
        float bladeLength = Mathf.Max(0.55f, range * 1.1f);
        float bladeWidth = Mathf.Max(0.2f, range * 0.34f);
        float visualScale = Mathf.Max(0.1f, meleeSwordVisualScale);
        Vector2 hitboxSize = new Vector2(bladeLength, bladeWidth);

        var sprite = ResolveEffectSprite(meleeSlashSpritePath, "Art/Items/icon_weapon_melee", 96);
        float targetVisualLength = hitboxSize.x * visualScale;
        float uniformVisualScale = GetUniformSpriteScale(sprite, targetVisualLength);
        var go = CreateFxSprite("MeleeSwordSwingFx", transform.position, sprite, Color.white, combatFxSortingOrder + 5, startAngle);
        SpriteRenderer renderer = go != null ? go.GetComponent<SpriteRenderer>() : null;

        var hitTargets = new System.Collections.Generic.HashSet<Health>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 origin = transform.position;
            Vector2 center = origin + dir * reach;
            UpdateMeleeHitboxGizmo(center, hitboxSize, angle);

            ApplyMeleeHitboxDamage(center, hitboxSize, angle, damage, elementCount, e0, e1, e2, hitTargets);

            if (go != null)
            {
                go.transform.position = center;
                go.transform.rotation = Quaternion.Euler(0f, 0f, angle + meleeSwordSpriteAngleOffset);
                go.transform.localScale = new Vector3(uniformVisualScale, uniformVisualScale, 1f);
            }

            if (renderer != null)
            {
                Color c = meleeSlashColor;
                float pulse = Mathf.Sin(t * Mathf.PI);
                c.a = Mathf.Clamp01((0.35f + (0.65f * pulse)) * Mathf.Clamp01(meleeSlashColor.a));
                renderer.color = c;
            }

            yield return null;
        }

        if (go != null)
        {
            Destroy(go);
        }
    }

    private void UpdateMeleeHitboxGizmo(Vector2 center, Vector2 size, float angleDeg)
    {
        _meleeGizmoCenter = center;
        _meleeGizmoSize = size;
        _meleeGizmoAngle = angleDeg;
        _meleeGizmoExpireTime = Time.time + 0.08f;
    }

    private void OnDrawGizmos()
    {
        if (!showMeleeHitboxGizmo || !Application.isPlaying)
        {
            return;
        }

        if (Time.time > _meleeGizmoExpireTime || _meleeGizmoSize.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        var prev = Gizmos.matrix;
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.95f);
        Gizmos.matrix = Matrix4x4.TRS(_meleeGizmoCenter, Quaternion.Euler(0f, 0f, _meleeGizmoAngle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_meleeGizmoSize.x, _meleeGizmoSize.y, 0f));
        Gizmos.matrix = prev;
    }

    private void ApplyMeleeHitboxDamage(
        Vector2 center,
        Vector2 size,
        float angleDeg,
        float damage,
        int elementCount,
        ElementType e0,
        ElementType e1,
        ElementType e2,
        System.Collections.Generic.HashSet<Health> hitTargets)
    {
        var enemies = EnemyController.Active;
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }

        float halfX = size.x * 0.5f;
        float halfY = size.y * 0.5f;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                continue;
            }

            if (hitTargets != null && hitTargets.Contains(health))
            {
                continue;
            }

            float enemyRadius = GetEnemyCollisionRadius(enemy);
            Vector2 delta = (Vector2)enemy.transform.position - center;
            Vector2 local = Rotate(delta, -angleDeg);
            float closestX = Mathf.Clamp(local.x, -halfX, halfX);
            float closestY = Mathf.Clamp(local.y, -halfY, halfY);
            float dx = local.x - closestX;
            float dy = local.y - closestY;
            if ((dx * dx + dy * dy) > (enemyRadius * enemyRadius))
            {
                continue;
            }

            hitTargets?.Add(health);
            health.Damage(damage);
            ApplyElementsToTarget(enemy.transform, e0, e1, e2, elementCount);
            ApplyHitReactionToTarget(enemy.transform, WeaponType.Melee);
        }
    }

    private Vector2 ResolveMeleeFacingDirection()
    {
        if (_ownerPlayer == null)
        {
            _ownerPlayer = GetComponent<PlayerController>();
        }

        float horizontal = 0f;
        if (_ownerPlayer != null)
        {
            horizontal = _ownerPlayer.FacingDirection.x;
        }

        if (Mathf.Abs(horizontal) < 0.001f && Mathf.Abs(_cachedDir.x) > 0.001f)
        {
            horizontal = _cachedDir.x;
        }

        if (Mathf.Abs(horizontal) > 0.001f)
        {
            _meleeHorizontalFacingSign = horizontal < 0f ? -1f : 1f;
        }

        return _meleeHorizontalFacingSign < 0f ? Vector2.left : Vector2.right;
    }

                private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, string spritePath, byte weaponId, WeaponType weaponType, int pierceOverride = -1)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, Vector2.zero, _projectileSpeed, spritePath, weaponId, weaponType, pierceOverride);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset, string spritePath, byte weaponId, WeaponType weaponType, int pierceOverride = -1)
    {
        SpawnProjectile(direction, damageOverride, spinSpeed, lifetimeOverride, spawnOffset, _projectileSpeed, spritePath, weaponId, weaponType, pierceOverride);
    }

    private void SpawnProjectile(Vector2 direction, float damageOverride, float spinSpeed, float lifetimeOverride, Vector2 spawnOffset, float speedOverride, string spritePath, byte weaponId, WeaponType weaponType, int pierceOverride = -1)
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
        if (ShouldAlignDirectionalProjectile(weaponType))
        {
            go.transform.rotation = Quaternion.Euler(0f, 0f, GetDirectionalProjectileRotation(direction));
        }

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        const float fallbackScale = 0.4f;
        bool hasSprite = TryResolveProjectileSprite(spritePath, _projectileSize, out var projectileSprite);
        renderer.sprite = projectileSprite;
        float spriteScale = Mathf.Max(0.01f, projectileSpriteScale);
        if (weaponType == WeaponType.PiercingShot)
        {
            spriteScale *= Mathf.Max(0.1f, piercingShotVisualScaleMult);
        }
        go.transform.localScale = Vector3.one * (hasSprite ? spriteScale : fallbackScale);
        var baseColor = new Color(0.9f, 0.9f, 0.2f, 1f);
        var projectileTint = ResolveSpriteTint(baseColor, hasSprite);
        var netColor = go.GetComponent<NetworkColor>();

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
        col.radius = hasSprite
            ? 0.5f * fallbackScale / Mathf.Max(0.01f, spriteScale)
            : 0.5f;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        float life = lifetimeOverride > 0f ? lifetimeOverride : _projectileLifetime;
        int resolvedPierce = pierceOverride >= 0 ? pierceOverride : _projectilePierce;
        proj.Initialize(direction, speedOverride, damageOverride, life, resolvedPierce, spinSpeed);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));
        ApplyElementsToProjectile(proj, weaponType);
        ApplyHitReactionToProjectile(proj, weaponType);

        ApplyNetworkVisual(renderer, netColor, projectileTint, spritePath, weaponId);
        SpawnProjectileMuzzleEffect((Vector2)transform.position + spawnOffset, direction, baseColor, spritePath);
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private bool ShouldAlignDirectionalProjectile(WeaponType weaponType)
    {
        return weaponType == WeaponType.SingleShot
            || weaponType == WeaponType.MultiShot
            || weaponType == WeaponType.PiercingShot;
    }

    private float GetDirectionalProjectileRotation(Vector2 direction)
    {
        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return angle - directionalProjectileBaseAngle;
    }

    private void SpawnNovaProjectile(Vector2 direction, float lifetime, float rotationSign, byte weaponId, WeaponType weaponType)
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

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        const float fallbackScale = 0.4f;
        bool hasSprite = TryResolveProjectileSprite(piercingShotSpritePath, _projectileSize, out var novaSprite);
        renderer.sprite = novaSprite;
        float spriteScale = Mathf.Max(0.01f, projectileSpriteScale) * Mathf.Max(0.1f, piercingShotVisualScaleMult);
        go.transform.localScale = Vector3.one * (hasSprite ? spriteScale : fallbackScale);
        var novaColor = new Color(0.6f, 0.8f, 1f, 1f);
        var projectileTint = ResolveSpriteTint(novaColor, hasSprite);
        var netColor = go.GetComponent<NetworkColor>();

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
        col.radius = hasSprite
            ? 0.5f * fallbackScale / Mathf.Max(0.01f, spriteScale)
            : 0.5f;

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        float angularSpeed = Mathf.Max(0.1f, piercingShotOrbitAngularSpeed) * rotationSign;
        proj.InitializeOrbit(transform.position, direction, _projectileSpeed, angularSpeed, _projectileDamage * _piercingShot.DamageMult, lifetime, _projectilePierce, 720f);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));
        ApplyElementsToProjectile(proj, weaponType);
        ApplyHitReactionToProjectile(proj, weaponType);
        ApplyNetworkVisual(renderer, netColor, projectileTint, piercingShotSpritePath, weaponId);
        SpawnProjectileMuzzleEffect(transform.position, direction, novaColor, piercingShotSpritePath);
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private void SpawnLaserProjectile(Vector2 direction, float damageOverride, float lifetime, Vector2 spawnOffset, float speedOverride, byte weaponId, WeaponType weaponType)
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
            go = GetPooledObject(_homingShotProjectilePool, "Laser");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position + (Vector3)spawnOffset;
        go.transform.localScale = new Vector3(homingShotLengthScale, homingShotThickness, 1f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = CreateSolidSprite();
        var netColor = go.GetComponent<NetworkColor>();

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
        proj.SetRelease(p => ReleaseProjectile(p, _homingShotProjectilePool));
        ApplyElementsToProjectile(proj, weaponType);
        ApplyHitReactionToProjectile(proj, weaponType);
        ApplyNetworkVisual(renderer, netColor, homingShotColor, null, weaponId);
        SpawnProjectileMuzzleEffect((Vector2)transform.position + spawnOffset, direction, homingShotColor, homingShotSpritePath);
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride, string spritePath, byte weaponId)
    {
        return SpawnColoredProjectile(direction, damageOverride, color, spinSpeed, lifetime, speedOverride, spritePath, weaponId, Vector2.zero, -1);
    }

    private Projectile SpawnColoredProjectile(Vector2 direction, float damageOverride, Color color, float spinSpeed, float lifetime, float speedOverride, string spritePath, byte weaponId, Vector2 spawnOffset, int pierceOverride = -1)
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

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        const float fallbackScale = 0.4f;
        bool hasSprite = TryResolveProjectileSprite(spritePath, _projectileSize, out var coloredSprite);
        renderer.sprite = coloredSprite;
        go.transform.localScale = Vector3.one * (hasSprite ? projectileSpriteScale : fallbackScale);
        var projectileTint = ResolveSpriteTint(color, hasSprite);
        var netColor = go.GetComponent<NetworkColor>();

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
        col.radius = GetScaledColliderRadius(0.5f, fallbackScale, hasSprite);

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = go.AddComponent<Projectile>();
        }
        int resolvedPierce = pierceOverride >= 0 ? pierceOverride : _projectilePierce;
        proj.Initialize(direction, speedOverride, damageOverride, lifetime, resolvedPierce, spinSpeed);
        proj.SetRelease(p => ReleaseProjectile(p, _circleProjectilePool));
        ApplyNetworkVisual(renderer, netColor, projectileTint, spritePath, weaponId);
        SpawnProjectileMuzzleEffect((Vector2)transform.position + spawnOffset, direction, color, spritePath);
        if (networked)
        {
            SpawnNetworkObject(go);
        }
        return proj;
    }

        private void SpawnBoomerang(Vector2 direction, float damageOverride, float lifetime, byte weaponId)
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
            go = GetPooledObject(_multiShotPool, "Boomerang");
        }

        if (go == null)
        {
            return;
        }

        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }
        const float fallbackScale = 0.45f;
        bool hasSprite = TryResolveProjectileSprite(multiShotSpritePath, _projectileSize, out var boomerangSprite);
        renderer.sprite = boomerangSprite;
        go.transform.localScale = Vector3.one * (hasSprite ? projectileSpriteScale : fallbackScale);
        var boomColor = new Color(0.2f, 0.9f, 0.9f, 1f);
        var projectileTint = ResolveSpriteTint(boomColor, hasSprite);
        var netColor = go.GetComponent<NetworkColor>();

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
        col.radius = GetScaledColliderRadius(0.5f, fallbackScale, hasSprite);

        var boom = go.GetComponent<BoomerangProjectile>();
        if (boom == null)
        {
            boom = go.AddComponent<BoomerangProjectile>();
        }
        boom.Initialize(transform, direction, _projectileSpeed, damageOverride, lifetime, 9999);
        boom.SetRelease(b => ReleaseBoomerang(b));
        ApplyElementsToBoomerang(boom, WeaponType.MultiShot);
        ApplyHitReactionToBoomerang(boom, WeaponType.MultiShot);
        ApplyNetworkVisual(renderer, netColor, projectileTint, multiShotSpritePath, weaponId);
        SpawnProjectileMuzzleEffect(transform.position, direction, boomColor, multiShotSpritePath);
        if (networked)
        {
            SpawnNetworkObject(go);
        }
    }

    private int GetWeaponElements(WeaponType type, out ElementType first, out ElementType second, out ElementType third)
    {
        if (_elementLoadout == null)
        {
            _elementLoadout = GetComponent<ElementLoadout>();
        }

        if (_elementLoadout == null)
        {
            first = ElementType.None;
            second = ElementType.None;
            third = ElementType.None;
            return 0;
        }

        return _elementLoadout.GetElements(type, out first, out second, out third);
    }

    private void ApplyElementsToProjectile(Projectile proj, WeaponType type)
    {
        if (proj == null)
        {
            return;
        }

        int count = GetWeaponElements(type, out var first, out var second, out var third);
        proj.SetElements(first, second, third, count);
    }

    private void ApplyElementsToBoomerang(BoomerangProjectile proj, WeaponType type)
    {
        if (proj == null)
        {
            return;
        }

        int count = GetWeaponElements(type, out var first, out var second, out var third);
        proj.SetElements(first, second, third, count);
    }

        private static void ApplyElementsToTarget(Transform target, ElementType first, ElementType second, ElementType third, int count)
    {
        if (target == null || count <= 0)
        {
            return;
        }

        var status = target.GetComponent<ElementStatus>();
        if (status == null)
        {
            return;
        }

        ElementSystem.ApplyElementsOnHit(first, second, third, count, status);
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
            AreaMult = 0f,
            BonusCount = 0,
            HitStunDuration = 0f,
            KnockbackDistance = 0f
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
            RangeMult = Mathf.Max(0f, stats.rangeMult),
            AreaMult = Mathf.Max(0f, stats.areaMult),
            BonusCount = Mathf.Max(0, stats.bonusProjectiles),
            HitStunDuration = Mathf.Max(0f, stats.hitStunDuration),
            KnockbackDistance = Mathf.Max(0f, stats.knockbackDistance)
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
        if (sprite == null)
        {
            var sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0)
            {
                sprite = sprites[0];
            }
        }
        _resourceSpriteCache[path] = sprite;
        return sprite;
    }

    private static bool TryResolveProjectileSprite(string path, int fallbackSize, out Sprite sprite)
    {
        sprite = LoadResourceSprite(path);
        if (sprite != null)
        {
            return true;
        }

        sprite = CreateCircleSprite(fallbackSize);
        return false;
    }

    private float GetScaledColliderRadius(float baseRadius, float fallbackScale, bool hasSprite)
    {
        if (!hasSprite)
        {
            return baseRadius;
        }

        float scale = Mathf.Max(0.01f, projectileSpriteScale);
        return baseRadius * fallbackScale / scale;
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
            ReturnToPool(_multiShotPool, boomerang.gameObject);
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

    private Transform FindLowestHealthRatioEnemy()
    {
        float limit = _range * _range;
        Transform bestTarget = null;
        float bestRatio = float.MaxValue;
        var enemies = EnemyController.Active;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Vector3 delta = enemy.transform.position - transform.position;
            if (delta.sqrMagnitude > limit)
            {
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health == null || health.MaxHealth <= 0f || health.IsDead)
            {
                continue;
            }

            float ratio = health.CurrentHealth / health.MaxHealth;
            if (ratio < bestRatio)
            {
                bestRatio = ratio;
                bestTarget = enemy.transform;
            }
        }

        return bestTarget;
    }

    private Transform FindRandomEnemy(float range)
    {
        float limit = range * range;
        var candidates = new System.Collections.Generic.List<Transform>();
        var enemies = EnemyController.Active;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Vector3 delta = enemy.transform.position - transform.position;
            if (delta.sqrMagnitude > limit)
            {
                continue;
            }

            candidates.Add(enemy.transform);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    private Vector2 ResolveFacingDirection()
    {
        if (_ownerPlayer == null)
        {
            _ownerPlayer = GetComponent<PlayerController>();
        }

        if (_ownerPlayer != null)
        {
            Vector2 facing = _ownerPlayer.FacingDirection;
            if (facing.sqrMagnitude > 0.0001f)
            {
                return facing.normalized;
            }
        }

        if (_cachedDir.sqrMagnitude > 0.0001f)
        {
            return _cachedDir.normalized;
        }

        return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
    }

    private static bool TryGetContactPointWithinRadius(EnemyController enemy, Vector2 center, float radius, out Vector2 contactPoint)
    {
        contactPoint = center;
        if (enemy == null || enemy.IsDead)
        {
            return false;
        }

        float sqrRadius = radius * radius;
        var col = enemy.GetComponent<Collider2D>();
        if (col != null && col.enabled)
        {
            Vector2 closest = col.ClosestPoint(center);
            Vector2 delta = closest - center;
            if (delta.sqrMagnitude > sqrRadius)
            {
                return false;
            }

            // ClosestPoint can equal center when origin is inside the collider.
            contactPoint = delta.sqrMagnitude > 0.0001f ? closest : (Vector2)enemy.transform.position;
            return true;
        }

        Vector2 enemyCenter = enemy.transform.position;
        if ((enemyCenter - center).sqrMagnitude > sqrRadius)
        {
            return false;
        }

        contactPoint = enemyCenter;
        return true;
    }

    private static bool TryGetEnemyHealthWithinRadius(EnemyController enemy, Vector2 center, float radius, out Health health, out Vector2 contactPoint)
    {
        health = null;
        if (!TryGetContactPointWithinRadius(enemy, center, radius, out contactPoint))
        {
            return false;
        }

        health = enemy.GetComponent<Health>();
        if (health == null || health.IsDead)
        {
            health = null;
            return false;
        }

        return true;
    }

    private static bool IsPointInsideSector(Vector2 point, Vector2 origin, Vector2 facing, float range, float halfAngle)
    {
        Vector2 toPoint = point - origin;
        if (toPoint.sqrMagnitude > range * range)
        {
            return false;
        }

        if (toPoint.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        Vector2 forward = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector2.right;
        float dotThreshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        Vector2 dir = toPoint.normalized;
        return Vector2.Dot(forward, dir) >= dotThreshold;
    }

    private static Vector2 Rotate(Vector2 direction, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        return new Vector2(
            (direction.x * c) - (direction.y * s),
            (direction.x * s) + (direction.y * c));
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
        line.startWidth = grenadeLineWidth;
        line.endWidth = grenadeLineWidth;
        line.sortingOrder = 2000;
        line.material = GetChainMaterial();
        line.startColor = grenadeColor;
        line.endColor = grenadeColor;

        Destroy(go, grenadeEffectDuration);
    }

    private void SpawnLightningEffect(Vector3 position)
    {
        var go = new GameObject("LightningStrike");
        var line = go.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = meleeLineWidth;
        line.endWidth = meleeLineWidth * 0.6f;
        line.sortingOrder = 2200;
        line.material = GetChainMaterial();
        line.startColor = meleeColor;
        line.endColor = meleeColor;

        Vector3 top = position + Vector3.up * meleeLineLength;
        line.SetPosition(0, top);
        line.SetPosition(1, position);

        Destroy(go, meleeEffectDuration);
    }

    private static Material _grenadeMaterial;

    private static Material GetChainMaterial()
    {
        if (_grenadeMaterial != null)
        {
            return _grenadeMaterial;
        }

        _grenadeMaterial = new Material(Shader.Find("Sprites/Default"));
        return _grenadeMaterial;
    }

    private static void ApplyNetworkVisual(SpriteRenderer renderer, NetworkColor netColor, Color color, string spritePath, byte weaponId)
    {
        if (netColor != null)
        {
            netColor.SetColor(color);
            if (!string.IsNullOrWhiteSpace(spritePath))
            {
                netColor.SetSpritePath(spritePath);
            }
            if (weaponId != WeaponIdNone)
            {
                netColor.SetWeaponId(weaponId);
            }
            return;
        }

        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    private static Color ResolveSpriteTint(Color fallbackColor, bool hasSprite)
    {
        return hasSprite ? Color.white : fallbackColor;
    }

    private static float GetUniformSpriteScale(Sprite sprite, float targetLength)
    {
        float safeTarget = Mathf.Max(0.05f, targetLength);
        if (sprite == null)
        {
            return safeTarget;
        }

        Vector2 size = sprite.bounds.size;
        float spriteLongestAxis = Mathf.Max(0.001f, Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y)));
        return safeTarget / spriteLongestAxis;
    }

    private static float GetEnemyCollisionRadius(EnemyController enemy)
    {
        if (enemy == null)
        {
            return 0.28f;
        }

        var circle = enemy.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Vector3 lossy = enemy.transform.lossyScale;
            float scale = Mathf.Max(0.01f, Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y)));
            return Mathf.Max(0.05f, circle.radius * scale);
        }

        var col = enemy.GetComponent<Collider2D>();
        if (col != null)
        {
            var ext = col.bounds.extents;
            return Mathf.Max(0.05f, Mathf.Max(ext.x, ext.y));
        }

        return 0.28f;
    }
}





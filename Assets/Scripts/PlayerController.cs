using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using Unity.Collections;

[RequireComponent(typeof(NetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    public static readonly System.Collections.Generic.List<PlayerController> Active = new System.Collections.Generic.List<PlayerController>();

    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float colliderRadius = 0.28f;

    [SerializeField]
    private float damageInvulnerabilityDuration = 0.35f;

    [SerializeField]
    private Color playerColor = new Color(0.2f, 0.9f, 0.3f, 1f);

    [SerializeField]
    private Color[] playerPalette = new[]
    {
        new Color(0.2f, 0.9f, 0.3f, 1f),
        new Color(1f, 0.85f, 0.2f, 1f),
        new Color(0.3f, 0.7f, 1f, 1f),
        new Color(1f, 0.4f, 0.6f, 1f)
    };

    [SerializeField]
    private Vector3 shadowOffset = new Vector3(0f, -0.35f, 0f);

    [SerializeField]
    private Vector3 shadowScale = new Vector3(0.6f, 0.25f, 1f);

    [SerializeField]
    private float shadowAlpha = 0.6f;

    [SerializeField]
    private bool allowOfflineControl = true;

    [SerializeField]
    private bool autoPlayEnabled = false;

    [SerializeField]
    private float visualScale = 2.5f;

    [Header("Auto Play")]
    [SerializeField]
    private bool autoSeekXp = true;

    [SerializeField]
    private bool autoXpPriority = false;

    [SerializeField]
    private float autoXpSeekRange = 5f;

    [SerializeField]
    private float autoMinDistance = 2.5f;

    [SerializeField]
    private float autoMaxDistance = 4.0f;

    [SerializeField]
    private float autoOrbitStrength = 0.8f;

    [SerializeField]
    private float autoKeepDistanceStrength = 0.6f;

    [SerializeField]
    private float autoCenterPull = 0.9f;

    [SerializeField]
    private float autoSmooth = 10f;

    [SerializeField]
    private float autoMidCenterPull = 0.1f;

    private bool _showColliderGizmos;

    private const int SpriteSize = 50;
    private NetworkVariable<int> _startCharacterIndex = new NetworkVariable<int>(-1);
    private NetworkVariable<bool> _gameStartedSignal = new NetworkVariable<bool>(false);
    private readonly NetworkVariable<float> _moveSpeedMultNet = new NetworkVariable<float>(1f);
    private float _moveSpeedMult = 1f;
    private Vector2 _autoInputCurrent;
    private Transform _visualRoot;
    private SpriteRenderer _visualRenderer;
    private SpriteRenderer _shadowRenderer;
    private bool _settingsApplied;
    private Vector2 _facingDirection = Vector2.right;
    private static readonly Vector2[] AutoSampleDirections = new[]
    {
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down,
        new Vector2(1f, 1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2(1f, -1f).normalized,
        new Vector2(-1f, -1f).normalized
    };

    private void Awake()
    {
        ApplySettings();
        EnsureNetworkTransform();
        EnsureVisual();
        EnsureShadow();
        EnsurePhysics();
        EnsureColliderGizmos();
        EnsureExperience();
        EnsureElementLoadout();
        EnsureAutoAttack();
        EnsureHealth();
        EnsureElementStatus();
        EnsureStatusBars();
        EnsureDamageVignette();
        EnsureVisuals();
    }

    private void OnEnable()
    {
        if (!Active.Contains(this))
        {
            Active.Add(this);
        }
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    private void Start()
    {
        ApplyPlayerColor();
    }

    private void EnsureNetworkTransform()
    {
        if (!NetworkSession.IsActive)
        {
            return;
        }

        var transformSync = GetComponent<NetworkTransform>();
        if (transformSync == null)
        {
            transformSync = gameObject.AddComponent<NetworkTransform>();
        }

        transformSync.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.player;
        _showColliderGizmos = config.game.showColliderGizmos;

        moveSpeed = settings.moveSpeed;
        colliderRadius = settings.colliderRadius > 0f ? settings.colliderRadius : 0.28f;
        damageInvulnerabilityDuration = settings.damageInvulnerabilityDuration;
        playerColor = settings.playerColor;
        playerPalette = settings.playerPalette;
        shadowOffset = settings.shadowOffset;
        shadowScale = settings.shadowScale;
        shadowAlpha = settings.shadowAlpha;
        allowOfflineControl = settings.allowOfflineControl;
        autoPlayEnabled = settings.autoPlayEnabled;
        visualScale = settings.visualScale;

        autoSeekXp = settings.autoSeekXp;
        autoXpPriority = settings.autoXpPriority;
        autoXpSeekRange = settings.autoXpSeekRange;
        autoMinDistance = settings.autoMinDistance;
        autoMaxDistance = settings.autoMaxDistance;
        autoOrbitStrength = settings.autoOrbitStrength;
        autoKeepDistanceStrength = settings.autoKeepDistanceStrength;
        autoCenterPull = settings.autoCenterPull;
        autoSmooth = settings.autoSmooth;
        autoMidCenterPull = settings.autoMidCenterPull;

        _settingsApplied = true;
    }

    private void EnsureColliderGizmos()
    {
        if (!_showColliderGizmos)
        {
            return;
        }

        if (GetComponent<ColliderGizmos>() == null)
        {
            gameObject.AddComponent<ColliderGizmos>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _startCharacterIndex.OnValueChanged += OnStartCharacterChanged;
        _moveSpeedMultNet.OnValueChanged += OnMoveSpeedChanged;
        if (IsServer)
        {
            _moveSpeedMultNet.Value = _moveSpeedMult;
        }
        ApplyPlayerColor();
        if (_startCharacterIndex.Value >= 0)
        {
            ApplyStartCharacterVisuals((StartCharacterType)_startCharacterIndex.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        _startCharacterIndex.OnValueChanged -= OnStartCharacterChanged;
        _moveSpeedMultNet.OnValueChanged -= OnMoveSpeedChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        if (GameSession.Instance != null && !GameSession.Instance.IsGameplayActive)
        {
            UpdateVisibility(false);
            return;
        }

        UpdateVisibility(true);

        if (!CanReadInput())
        {
            return;
        }

        Vector2 input = autoPlayEnabled ? GetAutoInput() : ReadMovement();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float mult = NetworkSession.IsActive ? _moveSpeedMultNet.Value : _moveSpeedMult;
        float speed = moveSpeed * mult;
        Vector2 delta = input * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);
        UpdateFacingFromInput(input);

        if (GameSession.Instance != null)
        {
            transform.position = GameSession.Instance.ClampToBounds(transform.position);
        }
    }

    public void SetMoveSpeedMultiplier(float value)
    {
        float clamped = Mathf.Max(0.1f, value);
        _moveSpeedMult = clamped;
        if (NetworkSession.IsActive)
        {
            if (IsServer)
            {
                _moveSpeedMultNet.Value = clamped;
            }
        }
    }

    public void SetAutoPlay(bool enabled)
    {
        autoPlayEnabled = enabled;
    }

    public bool IsAutoPlayEnabled => autoPlayEnabled;
    public Vector2 FacingDirection => _facingDirection;

    public bool HasStartCharacterSelection => _startCharacterIndex.Value >= 0;
    public bool GameStartedSignal => _gameStartedSignal.Value;

    public StartCharacterType StartCharacterSelection
    {
        get
        {
            if (_startCharacterIndex.Value < 0)
            {
                return StartCharacterType.SingleShot;
            }
            return (StartCharacterType)_startCharacterIndex.Value;
        }
    }

    public void SetStartCharacterSelection(StartCharacterType weapon)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (IsOwner)
            {
                SubmitStartCharacterSelectionServerRpc((int)weapon);
            }
        }
        else
        {
            _startCharacterIndex.Value = (int)weapon;
            ApplyStartCharacterVisuals(weapon);
        }
    }

    public void SetGameStartedSignal(bool value)
    {
        if (!IsServer)
        {
            return;
        }

        _gameStartedSignal.Value = value;
    }

    [ServerRpc]
    private void SubmitStartCharacterSelectionServerRpc(int weaponIndex)
    {
        _startCharacterIndex.Value = weaponIndex;
    }

    [ClientRpc]
    public void SyncSharedXpClientRpc(int level, float currentXp, float xpToNext)
    {
        var xp = GetComponent<Experience>();
        if (xp != null)
        {
            xp.SetSharedState(level, currentXp, xpToNext);
        }
    }

    [ClientRpc]
    public void ShowUpgradeUIClientRpc(FixedString128Bytes[] titles, FixedString512Bytes[] descs, int roundId, bool rerollAvailable, ClientRpcParams rpcParams = default)
    {
        GameSession.Instance?.ShowUpgradeChoicesFromNetwork(ToStringArray(titles), ToStringArray(descs), roundId, rerollAvailable);
    }

    [ClientRpc]
    public void HideUpgradeUIClientRpc(int roundId, ClientRpcParams rpcParams = default)
    {
        GameSession.Instance?.HideUpgradeChoicesFromNetwork(roundId);
    }

    [ClientRpc]
    public void SyncUpgradeIconStateClientRpc(GameSession.UpgradeIconState state, ClientRpcParams rpcParams = default)
    {
        GameSession.Instance?.ApplyUpgradeIconState(state);
    }

    [ServerRpc]
    public void SubmitUpgradeSelectionServerRpc(int index, int roundId)
    {
        GameSession.Instance?.ReceiveUpgradeSelectionServer(OwnerClientId, index, roundId);
    }

    [ServerRpc]
    public void RequestUpgradeRerollServerRpc(int roundId)
    {
        GameSession.Instance?.RequestUpgradeRerollServer(OwnerClientId, roundId);
    }

    private static string[] ToStringArray(FixedString128Bytes[] values)
    {
        if (values == null)
        {
            return null;
        }

        var result = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToString();
        }
        return result;
    }

    private static string[] ToStringArray(FixedString512Bytes[] values)
    {
        if (values == null)
        {
            return null;
        }

        var result = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToString();
        }
        return result;
    }

    private void OnStartCharacterChanged(int previous, int next)
    {
        if (next < 0)
        {
            return;
        }

        ApplyStartCharacterVisuals((StartCharacterType)next);
    }

    private void ApplyStartCharacterVisuals(StartCharacterType weapon)
    {
        var visuals = GetComponent<PlayerVisuals>();
        if (visuals == null)
        {
            visuals = gameObject.AddComponent<PlayerVisuals>();
        }

        switch (weapon)
        {
            case StartCharacterType.Melee:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.Warrior);
                break;
            case StartCharacterType.Aura:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.DemonLord);
                break;
            default:
                visuals.SetVisual(PlayerVisuals.PlayerVisualType.Mage);
                break;
        }
    }

    private bool CanReadInput()
    {
        if (IsOwner)
        {
            return true;
        }

        if (!allowOfflineControl)
        {
            return false;
        }

        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }

    private static Vector2 ReadMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

        return new Vector2(x, y);
    }

    private Vector2 GetAutoInput()
    {
        EnemyController closest = null;
        float bestSqr = float.MaxValue;
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

            float sqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = enemy;
            }
        }

        ExperiencePickup closestXp = null;
        float bestXpSqr = float.MaxValue;
        if (autoSeekXp)
        {
            float rangeSqr = autoXpSeekRange * autoXpSeekRange;
            var pickups = ExperiencePickup.Active;
            for (int i = 0; i < pickups.Count; i++)
            {
                var pickup = pickups[i];
                if (pickup == null)
                {
                    continue;
                }

                float sqr = (pickup.transform.position - transform.position).sqrMagnitude;
                if (sqr <= rangeSqr && sqr < bestXpSqr)
                {
                    bestXpSqr = sqr;
                    closestXp = pickup;
                }
            }
        }

        bool hasEnemy = false;
        bool enemyInBand = false;
        Vector2 toEnemy = Vector2.zero;
        float enemyDist = 0f;
        if (closest != null)
        {
            toEnemy = (closest.transform.position - transform.position);
            enemyDist = toEnemy.magnitude;
            if (enemyDist > 0.001f)
            {
                hasEnemy = true;
                enemyInBand = enemyDist >= autoMinDistance && enemyDist <= autoMaxDistance;
            }
        }

        Vector2 desired = Vector2.zero;
        bool hasTarget = false;
        Vector2 enemyDesire = Vector2.zero;
        bool hasXpTarget = false;

        if (closestXp != null)
        {
            Vector2 toXp = (closestXp.transform.position - transform.position);
            float dist = toXp.magnitude;
            if (dist > 0.001f)
            {
                desired = toXp / dist;
                hasTarget = true;
                hasXpTarget = true;
            }
        }

        // Move in a tighter band so auto-play stays closer to enemies while collecting XP.
        float engageMinDistance = Mathf.Max(0.4f, autoMinDistance * (hasXpTarget ? 0.65f : 0.8f));
        float engageMaxDistance = Mathf.Max(engageMinDistance + 0.45f, autoMaxDistance * (hasXpTarget ? 0.8f : 0.9f));

        if (hasEnemy && enemyDist > 0.001f)
        {
            Vector2 dir = toEnemy / enemyDist;
            float danger = Mathf.Clamp01((engageMinDistance - enemyDist) / Mathf.Max(0.01f, engageMinDistance));
            if (enemyDist < engageMinDistance)
            {
                enemyDesire = -dir * Mathf.Lerp(autoKeepDistanceStrength * 0.35f, autoKeepDistanceStrength * 0.9f, danger);
            }
            else if (enemyDist > engageMaxDistance)
            {
                enemyDesire = dir * autoKeepDistanceStrength * (hasXpTarget ? 1.05f : 0.75f);
            }
            else
            {
                if (!hasXpTarget)
                {
                    Vector2 orbit = new Vector2(-dir.y, dir.x);
                    float orbitSign = Mathf.Sin(Time.time * 0.7f) >= 0f ? 1f : -1f;
                    enemyDesire = orbit * autoOrbitStrength * orbitSign;
                }
                else if (enemyDist > engageMinDistance * 1.1f)
                {
                    enemyDesire = dir * autoKeepDistanceStrength * 0.45f;
                }
            }
        }

        if (hasEnemy && enemyDesire != Vector2.zero)
        {
            bool shouldOverride = !hasTarget || !autoXpPriority || enemyDist < engageMinDistance * 0.9f;
            if (shouldOverride)
            {
                desired = enemyDesire;
                hasTarget = true;
            }
            else if (desired != Vector2.zero)
            {
                float blend = hasXpTarget ? 0.2f : 0.35f;
                desired = Vector2.Lerp(desired, enemyDesire, blend);
            }
        }

        float avoidRadius = Mathf.Max(autoMaxDistance, autoMinDistance) * 1.6f;
        int threatCount;
        Vector2 avoidDir = GetAvoidDirection(transform.position, avoidRadius, enemies, out threatCount);
        if (avoidDir != Vector2.zero)
        {
            float weight = Mathf.Lerp(0.1f, 0.35f, Mathf.Clamp01(threatCount / 5f));
            if (hasXpTarget)
            {
                weight *= 0.6f;
            }
            desired = Vector2.Lerp(desired, avoidDir, weight);
        }

        if (threatCount >= 4)
        {
            Vector2 safeDir = GetSafestDirection(transform.position, avoidRadius, enemies);
            if (safeDir != Vector2.zero)
            {
                float weight = Mathf.Lerp(0.2f, 0.5f, Mathf.Clamp01((threatCount - 3f) / 5f));
                if (hasXpTarget)
                {
                    weight *= 0.6f;
                }
                desired = Vector2.Lerp(desired, safeDir, weight);
            }
        }

        if (GameSession.Instance != null)
        {
            if (hasTarget && desired == Vector2.zero && autoMidCenterPull > 0f)
            {
                Vector2 toCenter = -((Vector2)transform.position);
                if (toCenter.sqrMagnitude > 0.0001f)
                {
                    desired += toCenter.normalized * autoMidCenterPull;
                }
            }

            Vector2 bounds = GameSession.Instance.MapHalfSize;
            Vector2 pos = transform.position;
            float wallRange = 3f;
            Vector2 wallPush = Vector2.zero;
            float distX = bounds.x - Mathf.Abs(pos.x);
            if (distX < wallRange)
            {
                float strength = Mathf.Clamp01(1f - (distX / wallRange));
                wallPush.x = -Mathf.Sign(pos.x) * strength;
            }

            float distY = bounds.y - Mathf.Abs(pos.y);
            if (distY < wallRange)
            {
                float strength = Mathf.Clamp01(1f - (distY / wallRange));
                wallPush.y = -Mathf.Sign(pos.y) * strength;
            }

            if (wallPush != Vector2.zero)
            {
                float wallWeight = Mathf.Lerp(0.2f, 0.65f, Mathf.Clamp01(wallPush.magnitude));
                if (hasXpTarget)
                {
                    wallWeight *= 0.6f;
                }
                desired = Vector2.Lerp(desired, wallPush.normalized, wallWeight);
                desired += wallPush * (autoCenterPull * 0.6f);
            }
        }

        if (desired == Vector2.zero && !hasTarget)
        {
            desired = new Vector2(Mathf.Sin(Time.time * 0.7f), Mathf.Cos(Time.time * 0.7f));
        }

        if (desired.sqrMagnitude > 1f)
        {
            desired.Normalize();
        }

        float lerp = 1f - Mathf.Exp(-autoSmooth * Time.deltaTime);
        _autoInputCurrent = Vector2.Lerp(_autoInputCurrent, desired, lerp);
        return _autoInputCurrent;
    }

    private static Vector2 GetAvoidDirection(Vector2 origin, float radius, System.Collections.Generic.IList<EnemyController> enemies, out int threatCount)
    {
        threatCount = 0;
        if (enemies == null || radius <= 0f)
        {
            return Vector2.zero;
        }

        float radiusSqr = radius * radius;
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
            float distSqr = toEnemy.sqrMagnitude;
            if (distSqr > radiusSqr || distSqr < 0.0001f)
            {
                continue;
            }

            float dist = Mathf.Sqrt(distSqr);
            float weight = 1f - Mathf.Clamp01(dist / radius);
            sum += (toEnemy / dist) * weight;
            threatCount++;
        }

        if (sum.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        return -sum.normalized;
    }

    private static Vector2 GetSafestDirection(Vector2 origin, float radius, System.Collections.Generic.IList<EnemyController> enemies)
    {
        if (enemies == null || radius <= 0f)
        {
            return Vector2.zero;
        }

        bool hasThreat = false;
        float radiusSqr = radius * radius;
        float bestScore = float.MaxValue;
        Vector2 bestDir = Vector2.zero;

        for (int d = 0; d < AutoSampleDirections.Length; d++)
        {
            Vector2 dir = AutoSampleDirections[d];
            float score = 0f;

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
                float distSqr = toEnemy.sqrMagnitude;
                if (distSqr > radiusSqr || distSqr < 0.0001f)
                {
                    continue;
                }

                float dist = Mathf.Sqrt(distSqr);
                float proximity = 1f - Mathf.Clamp01(dist / radius);
                Vector2 toEnemyDir = toEnemy / dist;
                float toward = Mathf.Max(0f, Vector2.Dot(dir, toEnemyDir));
                score += toward * proximity;
                hasThreat = true;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        return hasThreat ? bestDir : Vector2.zero;
    }

    private void EnsureVisual()
    {
        var rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        _visualRoot = GetOrCreateVisualRoot();
        _visualRoot.localScale = Vector3.one * visualScale;

        _visualRenderer = _visualRoot.GetComponent<SpriteRenderer>();
        if (_visualRenderer == null)
        {
            _visualRenderer = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
        }

        var animator = _visualRoot.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            _visualRenderer.color = Color.white;
            return;
        }

        if (_visualRenderer.sprite == null)
        {
            _visualRenderer.sprite = CreateCircleSprite(SpriteSize);
        }

        _visualRenderer.color = Color.white;
    }

    private void EnsureShadow()
    {
        var existing = transform.Find("Shadow");
        if (existing == null)
        {
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = shadowOffset;
            shadow.transform.localScale = shadowScale;
            _shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        }
        else
        {
            _shadowRenderer = existing.GetComponent<SpriteRenderer>();
            if (_shadowRenderer == null)
            {
                _shadowRenderer = existing.gameObject.AddComponent<SpriteRenderer>();
            }
            existing.localPosition = shadowOffset;
            existing.localScale = shadowScale;
        }

        if (_shadowRenderer.sprite == null)
        {
            _shadowRenderer.sprite = CreateCircleSprite(SpriteSize);
        }

        UpdateShadowColor();
        _shadowRenderer.sortingOrder = -1;
    }

    private void UpdateFacingFromInput(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
        {
            return;
        }

        SetFacing(input.normalized);
    }

    private void SetFacing(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        _facingDirection = direction.normalized;

        if (_visualRenderer == null)
        {
            return;
        }

        if (Mathf.Abs(_facingDirection.x) > 0.001f)
        {
            _visualRenderer.flipX = _facingDirection.x < 0f;
        }
    }

    private void ApplyPlayerColor()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (playerPalette != null && playerPalette.Length > 0)
            {
                int index = (int)(OwnerClientId % (ulong)playerPalette.Length);
                playerColor = playerPalette[index];
            }
        }

        UpdateShadowColor();
    }

    private void OnMoveSpeedChanged(float previous, float next)
    {
        _moveSpeedMult = next;
    }

    private void UpdateShadowColor()
    {
        if (_shadowRenderer == null)
        {
            return;
        }

        var shadowColor = playerColor;
        shadowColor.a = Mathf.Clamp01(shadowAlpha);
        _shadowRenderer.color = shadowColor;
    }

    private void UpdateVisibility(bool visible)
    {
        if (_visualRenderer != null)
        {
            _visualRenderer.enabled = visible;
        }

        if (_shadowRenderer != null)
        {
            _shadowRenderer.enabled = visible;
        }
    }

    private void EnsurePhysics()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }

        col.isTrigger = true;
        col.radius = Mathf.Max(0.05f, colliderRadius);
    }

    private void EnsureHealth()
    {
        var health = GetComponent<Health>();
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
        }

        if (health != null)
        {
            health.SetDamageInvulnerabilityDuration(damageInvulnerabilityDuration);
        }

        if (NetworkSession.IsActive && GetComponent<Unity.Netcode.NetworkObject>() != null && GetComponent<NetworkHealth>() == null)
        {
            gameObject.AddComponent<NetworkHealth>();
        }
    }

    private void EnsureElementStatus()
    {
        if (GetComponent<ElementStatus>() == null)
        {
            gameObject.AddComponent<ElementStatus>();
        }
    }

    private void EnsureElementLoadout()
    {
        if (GetComponent<ElementLoadout>() == null)
        {
            gameObject.AddComponent<ElementLoadout>();
        }

        if (NetworkSession.IsActive && GetComponent<Unity.Netcode.NetworkObject>() != null && GetComponent<NetworkElementLoadout>() == null)
        {
            gameObject.AddComponent<NetworkElementLoadout>();
        }
    }

    private void EnsureAutoAttack()
    {
        if (GetComponent<AutoAttack>() == null)
        {
            gameObject.AddComponent<AutoAttack>();
        }
    }

    private void EnsureExperience()
    {
        if (GetComponent<Experience>() == null)
        {
            gameObject.AddComponent<Experience>();
        }
    }

    private void EnsureStatusBars()
    {
        if (GetComponent<PlayerStatusBars>() == null)
        {
            gameObject.AddComponent<PlayerStatusBars>();
        }
    }

    private void EnsureDamageVignette()
    {
        if (GetComponent<PlayerDamageVignette>() == null)
        {
            gameObject.AddComponent<PlayerDamageVignette>();
        }
    }

    private void EnsureVisuals()
    {
        if (GetComponent<PlayerVisuals>() == null)
        {
            gameObject.AddComponent<PlayerVisuals>();
        }
    }

    private Transform GetOrCreateVisualRoot()
    {
        var existing = transform.Find("Visuals");
        if (existing != null)
        {
            EnsureAligner(existing.gameObject);
            return existing;
        }

        var root = new GameObject("Visuals");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        EnsureAligner(root);
        return root.transform;
    }

    private static void EnsureAligner(GameObject root)
    {
        if (root.GetComponent<VisualsAligner>() == null)
        {
            root.AddComponent<VisualsAligner>();
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



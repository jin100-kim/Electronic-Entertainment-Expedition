using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<EnemyController> Active = new System.Collections.Generic.List<EnemyController>();

    [SerializeField]
    private float moveSpeed = 2.5f;

    [SerializeField]
    private float contactDamage = 10f;

    [SerializeField]
    private float damageCooldown = 0.5f;

    [SerializeField]
    private int xpReward = 1;

    [SerializeField]
    private float maxHealth = 40f;

    [Header("Death")]
    [SerializeField]
    private float deathFadeDelay = 0.2f;

    [SerializeField]
    private float deathFadeDuration = 0.6f;

    [Header("Targeting")]
    [SerializeField]
    private float targetScanInterval = 1f;

    private float _nextDamageTime;
    private Health _health;
    private float _slowMultiplier = 1f;
    private float _slowTimer;
    private float _stunTimer;
    private float _knockbackTimer;
    private Vector2 _knockbackVelocity;
    private bool _dead;
    private SpriteRenderer[] _visualRenderers;
    private float _nextTargetScanTime;

    public bool IsDead => _dead;
    public bool IsStunned => _stunTimer > 0f || _knockbackTimer > 0f;

    public Transform Target { get; set; }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public float ContactDamage
    {
        get => contactDamage;
        set => contactDamage = value;
    }

    public float DamageCooldown
    {
        get => damageCooldown;
        set => damageCooldown = value;
    }

    public int XpReward
    {
        get => xpReward;
        set => xpReward = Mathf.Max(1, value);
    }

    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = Mathf.Max(1f, value);
            if (_health != null)
            {
                _health.SetMaxHealth(maxHealth, true);
            }
        }
    }

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            _health = gameObject.AddComponent<Health>();
        }

        if (GetComponent<ElementStatus>() == null)
        {
            gameObject.AddComponent<ElementStatus>();
        }


        if (NetworkSession.IsActive && GetComponent<Unity.Netcode.NetworkObject>() != null && GetComponent<NetworkHealth>() == null)
        {
            gameObject.AddComponent<NetworkHealth>();
        }

        _health.SetMaxHealth(maxHealth, true);

        if (GetComponent<EnemyHealthBar>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar>();
        }

        if (GetComponent<DamageTextOnHit>() == null)
        {
            gameObject.AddComponent<DamageTextOnHit>();
        }

        EnsureColliderGizmos();
        ResolveVisualRenderers();
        _health.OnDied += OnDied;
    }

    private void Start()
    {
        ResolveVisualRenderers();
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

    private void Update()
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (_dead)
        {
            return;
        }

        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        if (Target != null)
        {
            var targetHealth = Target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.IsDead)
            {
                Target = null;
            }
        }

        if (Target == null || Time.time >= _nextTargetScanTime)
        {
            Target = FindClosestPlayer();
            _nextTargetScanTime = Time.time + Mathf.Max(0.1f, targetScanInterval);
        }

        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0f)
            {
                _slowMultiplier = 1f;
            }
        }

        float dt = Time.deltaTime;
        if (_knockbackTimer > 0f)
        {
            _knockbackTimer = Mathf.Max(0f, _knockbackTimer - dt);
            transform.position += (Vector3)(_knockbackVelocity * dt);
            if (GameSession.Instance != null)
            {
                transform.position = GameSession.Instance.ClampToBounds(transform.position);
            }

            if (_knockbackVelocity.sqrMagnitude > 0.0001f)
            {
                UpdateFacing(_knockbackVelocity);
            }
        }

        if (_stunTimer > 0f)
        {
            _stunTimer = Mathf.Max(0f, _stunTimer - dt);
        }

        if (_knockbackTimer > 0f || _stunTimer > 0f)
        {
            return;
        }

        if (Target == null)
        {
            return;
        }

        Vector3 toTarget = Target.position - transform.position;
        toTarget.z = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 dir = toTarget.normalized;
        UpdateFacing(dir);
        float speed = moveSpeed * _slowMultiplier;
        transform.position += dir * speed * Time.deltaTime;

        if (GameSession.Instance != null)
        {
            transform.position = GameSession.Instance.ClampToBounds(transform.position);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (_dead)
        {
            return;
        }

        if (Time.time < _nextDamageTime)
        {
            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        var health = player.GetComponent<Health>();
        if (health == null)
        {
            return;
        }

        _nextDamageTime = Time.time + damageCooldown;
        health.Damage(contactDamage);
    }

    private void OnDied()
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (_dead)
        {
            return;
        }

        _dead = true;
        if (GameSession.Instance != null)
        {
            GameSession.Instance.RegisterKill(transform.position);
        }
        SpawnXp();

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        StartCoroutine(DeathFade());
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        float clamped = Mathf.Clamp(multiplier, 0.1f, 1f);
        if (clamped < _slowMultiplier)
        {
            _slowMultiplier = clamped;
        }

        if (duration > _slowTimer)
        {
            _slowTimer = duration;
        }
    }

    public void ApplyHitReaction(Vector2 direction, float knockbackDistance, float stunDuration)
    {
        if (_dead)
        {
            return;
        }

        float stun = Mathf.Max(0f, stunDuration);
        if (stun > _stunTimer)
        {
            _stunTimer = stun;
        }

        float distance = Mathf.Max(0f, knockbackDistance);
        if (distance <= 0f)
        {
            return;
        }

        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
        if (dir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float duration = stun > 0f ? stun : 0.08f;
        _knockbackVelocity = dir * (distance / Mathf.Max(0.01f, duration));
        _knockbackTimer = Mathf.Max(_knockbackTimer, duration);
    }

    private Transform FindClosestPlayer()
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            var health = player.GetComponent<Health>();
            if (health != null && health.IsDead)
            {
                continue;
            }

            Vector3 delta = player.transform.position - transform.position;
            float sqr = delta.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = player.transform;
            }
        }

        return best;
    }

    private void ResolveVisualRenderers()
    {
        var visualRoot = transform.Find("Visuals");
        if (visualRoot != null)
        {
            _visualRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
            if (_visualRenderers != null && _visualRenderers.Length > 0)
            {
                return;
            }
        }

        var rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            _visualRenderers = new[] { rootRenderer };
        }
    }

    private void UpdateFacing(Vector3 direction)
    {
        if (_visualRenderers == null || _visualRenderers.Length == 0)
        {
            ResolveVisualRenderers();
        }

        if (_visualRenderers == null || _visualRenderers.Length == 0)
        {
            return;
        }

        if (Mathf.Abs(direction.x) < 0.001f)
        {
            return;
        }

        bool flip = direction.x < 0f;
        for (int i = 0; i < _visualRenderers.Length; i++)
        {
            var renderer = _visualRenderers[i];
            if (renderer != null)
            {
                renderer.flipX = flip;
            }
        }
    }

    private System.Collections.IEnumerator DeathFade()
    {
        float delay = Mathf.Max(0f, deathFadeDelay);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        float duration = Mathf.Max(0.05f, deathFadeDuration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - t / duration);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null)
                {
                    continue;
                }

                var c = r.color;
                c.a = alpha;
                r.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void SpawnXp()
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        ExperiencePickup.Spawn(transform.position, xpReward);
    }

    private void EnsureColliderGizmos()
    {
        var config = GameConfig.LoadOrCreate();
        if (config == null || !config.game.showColliderGizmos)
        {
            return;
        }

        if (GetComponent<ColliderGizmos>() == null)
        {
            gameObject.AddComponent<ColliderGizmos>();
        }
    }

}

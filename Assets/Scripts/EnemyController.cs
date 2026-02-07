using UnityEngine;

public class EnemyController : MonoBehaviour
{
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
    private float deathFadeDelay = 1f;

    [SerializeField]
    private float deathFadeDuration = 0.6f;

    private float _nextDamageTime;
    private Health _health;
    private float _slowMultiplier = 1f;
    private float _slowTimer;
    private bool _dead;

    public bool IsDead => _dead;

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

        _health.SetMaxHealth(maxHealth, true);

        if (GetComponent<EnemyHealthBar>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar>();
        }

        _health.OnDied += OnDied;
    }

    private void Update()
    {
        if (_dead)
        {
            return;
        }

        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
        {
            return;
        }

        if (Target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                Target = player.transform;
            }
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

        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0f)
            {
                _slowMultiplier = 1f;
            }
        }

        Vector3 dir = toTarget.normalized;
        float speed = moveSpeed * _slowMultiplier;
        transform.position += dir * speed * Time.deltaTime;

        if (GameSession.Instance != null)
        {
            transform.position = GameSession.Instance.ClampToBounds(transform.position);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
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
        if (_dead)
        {
            return;
        }

        _dead = true;
        deathFadeDelay = 0f;
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
        var go = new GameObject("XP");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(50);
        renderer.color = new Color(0.2f, 0.8f, 1f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        var pickup = go.AddComponent<ExperiencePickup>();
        pickup.SetAmount(xpReward);
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

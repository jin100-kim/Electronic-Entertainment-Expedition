using UnityEngine;
using Unity.Netcode;

public class ExperiencePickup : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<ExperiencePickup> Active = new System.Collections.Generic.List<ExperiencePickup>();
    private static readonly System.Collections.Generic.Stack<ExperiencePickup> _pool = new System.Collections.Generic.Stack<ExperiencePickup>();
    private static Sprite _cachedSprite;
    private static int _cachedSpriteSize = -1;
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _resourceSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();

    [SerializeField]
    private float amount = 1f;

    [SerializeField]
    private float magnetScanInterval = 0.2f;

    private Experience _magnetTarget;
    private float _nextScanTime;
    private bool _settingsApplied;

    public static ExperiencePickup Spawn(Vector3 position, float value)
    {
        if (NetworkSession.IsActive)
        {
            if (!NetworkSession.IsServer)
            {
                return null;
            }

            RuntimeNetworkPrefabs.EnsureRegistered();
            var netGo = RuntimeNetworkPrefabs.InstantiateXp();
            if (netGo == null)
            {
                return null;
            }

            var netPickup = netGo.GetComponent<ExperiencePickup>();
            netGo.SetActive(true);
            netGo.transform.position = position;
            netPickup.ApplySettings();
            netGo.transform.localScale = Vector3.one * GetSettings().xpPickupScale;
            netPickup.EnsureVisuals();
            netPickup.SetAmount(value);

            var netObj = netGo.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }

            return netPickup;
        }

        ExperiencePickup pickup = null;
        while (_pool.Count > 0 && pickup == null)
        {
            pickup = _pool.Pop();
        }

        if (pickup == null)
        {
            pickup = CreateInstance();
        }

        var go = pickup.gameObject;
        go.SetActive(true);
        go.transform.position = position;
        var settings = GetSettings();
        go.transform.localScale = Vector3.one * settings.xpPickupScale;
        pickup.EnsureVisuals();
        pickup.SetAmount(value);
        return pickup;
    }

    public void SetAmount(float value)
    {
        amount = Mathf.Max(0.01f, value);
    }

    private void OnEnable()
    {
        if (!Active.Contains(this))
        {
            Active.Add(this);
        }
        _magnetTarget = null;
        _nextScanTime = 0f;
        EnsureVisuals();
    }

    private void Awake()
    {
        ApplySettings();
        EnsureVisuals();
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

        var session = GameSession.Instance;
        if (session != null)
        {
            if (session.IsGameOver || session.IsChoosingUpgrade)
            {
                return;
            }
        }

        if (Time.time >= _nextScanTime || _magnetTarget == null)
        {
            _magnetTarget = FindClosestTarget();
            _nextScanTime = Time.time + magnetScanInterval;
        }

        if (_magnetTarget == null)
        {
            return;
        }

        float range = _magnetTarget.MagnetRange;
        if (range <= 0f)
        {
            return;
        }

        Vector3 toTarget = _magnetTarget.transform.position - transform.position;
        float dist = toTarget.magnitude;
        if (dist > range)
        {
            return;
        }

        float speed = _magnetTarget.MagnetSpeed;
        if (speed <= 0f)
        {
            return;
        }

        Vector3 step = toTarget.normalized * speed * Time.deltaTime;
        if (step.sqrMagnitude >= toTarget.sqrMagnitude)
        {
            transform.position = _magnetTarget.transform.position;
        }
        else
        {
            transform.position += step;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        var xp = other.GetComponent<Experience>();
        if (xp == null)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            GameSession.Instance?.AddSharedXp(amount);
        }
        else
        {
            xp.AddXp(amount);
        }
        Despawn();
    }

    private Experience FindClosestTarget()
    {
        Experience closest = null;
        float bestSqr = float.MaxValue;
        var targets = Experience.Active;
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target == null)
            {
                continue;
            }

            Vector3 delta = target.transform.position - transform.position;
            float sqr = delta.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = target;
            }
        }

        return closest;
    }

    private void Despawn()
    {
        if (NetworkSession.IsActive)
        {
            var netObj = GetComponent<NetworkObject>();
            if (NetworkSession.IsServer && netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        gameObject.SetActive(false);
        _pool.Push(this);
    }

    private static ExperiencePickup CreateInstance()
    {
        var go = new GameObject("XP");
        var settings = GetSettings();
        go.transform.localScale = Vector3.one * settings.xpPickupScale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = ResolvePickupSprite(settings.xpSpritePath, settings.xpSpriteSize);
        renderer.color = settings.xpColor;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        ApplyWorldRadius(col, settings.xpColliderRadius, go.transform.localScale.x);

        var pickup = go.AddComponent<ExperiencePickup>();
        return pickup;
    }

    private static Sprite GetCachedSprite(int size)
    {
        if (_cachedSprite != null && _cachedSpriteSize == size)
        {
            return _cachedSprite;
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
        _cachedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _cachedSpriteSize = size;
        return _cachedSprite;
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

    private static Sprite ResolvePickupSprite(string path, int fallbackSize)
    {
        var sprite = LoadResourceSprite(path);
        return sprite != null ? sprite : GetCachedSprite(fallbackSize);
    }

    private static bool TryResolvePickupSprite(string path, int fallbackSize, out Sprite sprite)
    {
        sprite = LoadResourceSprite(path);
        if (sprite != null)
        {
            return true;
        }

        sprite = GetCachedSprite(fallbackSize);
        return false;
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var settings = GetSettings();
        magnetScanInterval = settings.xpMagnetScanInterval;
        _settingsApplied = true;
    }

    private static PickupConfig GetSettings()
    {
        var config = GameConfig.LoadOrCreate();
        return config.pickups;
    }

    private void EnsureVisuals()
    {
        var settings = GetSettings();
        transform.localScale = Vector3.one * settings.xpPickupScale;
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        bool hasSprite = TryResolvePickupSprite(settings.xpSpritePath, settings.xpSpriteSize, out var sprite);
        renderer.sprite = sprite;
        Color resolvedColor = ResolvePickupTint(settings.xpColor, hasSprite);
        if (NetworkSession.IsActive)
        {
            var netColor = GetComponent<NetworkColor>();
            if (netColor != null)
            {
                if (NetworkSession.IsServer)
                {
                    netColor.SetColor(resolvedColor);
                }
                else
                {
                    renderer.color = resolvedColor;
                }
            }
            else
            {
                renderer.color = resolvedColor;
            }
        }
        else
        {
            renderer.color = resolvedColor;
        }

        var col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            ApplyWorldRadius(col, settings.xpColliderRadius, transform.localScale.x);
        }
    }

    private static void ApplyWorldRadius(CircleCollider2D collider, float worldRadius, float localScale)
    {
        if (collider == null)
        {
            return;
        }

        float safeScale = Mathf.Max(0.01f, Mathf.Abs(localScale));
        float targetWorldRadius = Mathf.Max(0.02f, worldRadius);
        collider.radius = targetWorldRadius / safeScale;
    }

    private static Color ResolvePickupTint(Color fallbackColor, bool hasSprite)
    {
        return hasSprite ? Color.white : fallbackColor;
    }
}

using UnityEngine;
using Unity.Netcode;

public class CoinPickup : MonoBehaviour
{
    private static readonly System.Collections.Generic.Stack<CoinPickup> _pool = new System.Collections.Generic.Stack<CoinPickup>();
    private static Sprite _cachedSprite;
    private static int _cachedSpriteSize = -1;
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _resourceSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();

    [SerializeField]
    private int amount = 1;

    [SerializeField]
    private float magnetScanInterval = 0.2f;

    private Experience _magnetTarget;
    private float _nextScanTime;
    private bool _settingsApplied;

    public static CoinPickup Spawn(Vector3 position, int value)
    {
        if (NetworkSession.IsActive)
        {
            if (!NetworkSession.IsServer)
            {
                return null;
            }

            RuntimeNetworkPrefabs.EnsureRegistered();
            var netGo = RuntimeNetworkPrefabs.InstantiateCoin();
            if (netGo == null)
            {
                return null;
            }

            var netPickup = netGo.GetComponent<CoinPickup>();
            netGo.SetActive(true);
            netGo.transform.position = position;
            netPickup.ApplySettings();
            netPickup.EnsureVisuals();
            netGo.transform.localScale = Vector3.one * GetSettings().coinPickupScale;
            netPickup.SetAmount(value);

            var netObj = netGo.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }

            return netPickup;
        }

        CoinPickup pickup = null;
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
        go.transform.localScale = Vector3.one * settings.coinPickupScale;
        pickup.SetAmount(value);
        return pickup;
    }

    public void SetAmount(int value)
    {
        amount = Mathf.Max(1, value);
    }

    private void OnEnable()
    {
        _magnetTarget = null;
        _nextScanTime = 0f;
        EnsureVisuals();
    }

    private void Awake()
    {
        ApplySettings();
        EnsureVisuals();
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

        GameSession.Instance?.AddCoins(amount);
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

    private static CoinPickup CreateInstance()
    {
        var go = new GameObject("Coin");
        var settings = GetSettings();
        go.transform.localScale = Vector3.one * settings.coinPickupScale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = ResolvePickupSprite(settings.coinSpritePath, settings.coinSpriteSize);
        renderer.color = settings.coinColor;
        renderer.sortingOrder = settings.coinSortingOrder;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = settings.coinColliderRadius;

        var pickup = go.AddComponent<CoinPickup>();
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

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var settings = GetSettings();
        magnetScanInterval = settings.coinMagnetScanInterval;
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
        transform.localScale = Vector3.one * settings.coinPickupScale;
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = ResolvePickupSprite(settings.coinSpritePath, settings.coinSpriteSize);
        if (NetworkSession.IsActive)
        {
            var netColor = GetComponent<NetworkColor>();
            if (netColor != null)
            {
                if (NetworkSession.IsServer)
                {
                    netColor.SetColor(settings.coinColor);
                }
                else
                {
                    renderer.color = settings.coinColor;
                }
            }
        }
        else
        {
            renderer.color = settings.coinColor;
        }
        renderer.sortingOrder = settings.coinSortingOrder;

        var col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = settings.coinColliderRadius;
        }
    }
}

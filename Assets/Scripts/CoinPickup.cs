using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private static readonly System.Collections.Generic.Stack<CoinPickup> _pool = new System.Collections.Generic.Stack<CoinPickup>();
    private static Sprite _cachedSprite;

    [SerializeField]
    private int amount = 1;

    [SerializeField]
    private float magnetScanInterval = 0.2f;

    private Experience _magnetTarget;
    private float _nextScanTime;

    public static CoinPickup Spawn(Vector3 position, int value)
    {
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
        go.transform.localScale = Vector3.one * 0.4f;
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
    }

    private void Update()
    {
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
        gameObject.SetActive(false);
        _pool.Push(this);
    }

    private static CoinPickup CreateInstance()
    {
        var go = new GameObject("Coin");
        go.transform.localScale = Vector3.one * 0.4f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = GetCachedSprite();
        renderer.color = new Color(1f, 0.85f, 0.2f, 1f);
        renderer.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.12f;

        var pickup = go.AddComponent<CoinPickup>();
        return pickup;
    }

    private static Sprite GetCachedSprite()
    {
        if (_cachedSprite != null)
        {
            return _cachedSprite;
        }

        const int size = 40;
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
        return _cachedSprite;
    }
}

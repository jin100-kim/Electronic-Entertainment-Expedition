using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class RestShrineSpawner : MonoBehaviour
{
    [SerializeField]
    private bool enableSpawning = true;

    [SerializeField]
    private RestShrineInteractable shrinePrefab;

    [SerializeField]
    private float spawnIntervalSeconds = 60f;

    [SerializeField]
    private int maxActiveShrines = 3;

    [SerializeField]
    private float edgePadding = 2f;

    [SerializeField]
    private float minDistanceFromPlayer = 4f;

    [SerializeField]
    private float minDistanceBetweenShrines = 3f;

    [SerializeField]
    private int spawnTryCount = 12;

    [SerializeField]
    private float retryDelaySeconds = 5f;

    [Header("Fallback Visual")]
    [SerializeField]
    private string fallbackSpritePath = "Art/Items/icon_reward_heal";

    [SerializeField]
    private int fallbackSpriteSize = 48;

    [SerializeField]
    private float fallbackSpriteScale = 1.8f;

    [SerializeField]
    private float fallbackColliderRadius = 0.45f;

    [SerializeField]
    private Color fallbackColor = new Color(0.55f, 1f, 0.75f, 1f);

    [SerializeField]
    private int fallbackSortingOrder = 25;

    private static Sprite _cachedFallbackSprite;
    private static int _cachedFallbackSize = -1;
    private static readonly Dictionary<string, Sprite> _resourceSpriteCache = new Dictionary<string, Sprite>();

    private readonly List<RestShrineInteractable> _activeShrines = new List<RestShrineInteractable>();
    private float _nextSpawnTime;
    private GameSession _session;

    private void Awake()
    {
        _session = GetComponent<GameSession>();
        _nextSpawnTime = Time.time + Mathf.Max(1f, spawnIntervalSeconds);
    }

    private void Update()
    {
        if (!enableSpawning)
        {
            return;
        }

        if (NetworkSession.IsActive)
        {
            return;
        }

        if (_session == null)
        {
            _session = GetComponent<GameSession>();
            if (_session == null)
            {
                return;
            }
        }

        if (!_session.IsGameplayActive || _session.IsGameOver || _session.IsChoosingUpgrade)
        {
            return;
        }

        CleanupDeadShrines();
        if (maxActiveShrines > 0 && _activeShrines.Count >= maxActiveShrines)
        {
            return;
        }

        if (Time.time < _nextSpawnTime)
        {
            return;
        }

        if (TrySpawnShrine())
        {
            _nextSpawnTime = Time.time + Mathf.Max(1f, spawnIntervalSeconds);
        }
        else
        {
            _nextSpawnTime = Time.time + Mathf.Max(1f, retryDelaySeconds);
        }
    }

    private void CleanupDeadShrines()
    {
        for (int i = _activeShrines.Count - 1; i >= 0; i--)
        {
            if (_activeShrines[i] == null)
            {
                _activeShrines.RemoveAt(i);
            }
        }
    }

    private bool TrySpawnShrine()
    {
        Vector2 half = _session.MapHalfSize;
        float minX = -half.x + edgePadding;
        float maxX = half.x - edgePadding;
        float minY = -half.y + edgePadding;
        float maxY = half.y - edgePadding;

        if (minX >= maxX || minY >= maxY)
        {
            return false;
        }

        Transform player = GetSpawnReferencePlayer();

        int tries = Mathf.Max(1, spawnTryCount);
        for (int i = 0; i < tries; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0f);

            if (player != null)
            {
                float playerSqr = (player.position - pos).sqrMagnitude;
                if (playerSqr < minDistanceFromPlayer * minDistanceFromPlayer)
                {
                    continue;
                }
            }

            if (!IsFarEnoughFromOtherShrines(pos))
            {
                continue;
            }

            var spawned = SpawnShrineAt(pos);
            if (spawned != null)
            {
                _activeShrines.Add(spawned);
                return true;
            }
        }

        return false;
    }

    private Transform GetSpawnReferencePlayer()
    {
        var players = PlayerController.Active;
        bool networkActive = NetworkSession.IsActive;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            if (networkActive && !player.IsOwner)
            {
                continue;
            }

            return player.transform;
        }

        return null;
    }

    private bool IsFarEnoughFromOtherShrines(Vector3 position)
    {
        float minSqr = minDistanceBetweenShrines * minDistanceBetweenShrines;
        for (int i = 0; i < _activeShrines.Count; i++)
        {
            var shrine = _activeShrines[i];
            if (shrine == null)
            {
                continue;
            }

            if ((shrine.transform.position - position).sqrMagnitude < minSqr)
            {
                return false;
            }
        }

        return true;
    }

    private RestShrineInteractable SpawnShrineAt(Vector3 position)
    {
        if (shrinePrefab != null)
        {
            return Instantiate(shrinePrefab, position, Quaternion.identity);
        }

        var go = new GameObject("RestShrine");
        go.transform.SetParent(transform, true);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * Mathf.Max(0.1f, fallbackSpriteScale);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = ResolveFallbackSprite();
        renderer.color = fallbackColor;
        renderer.sortingOrder = fallbackSortingOrder;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.05f, fallbackColliderRadius);

        return go.AddComponent<RestShrineInteractable>();
    }

    private Sprite ResolveFallbackSprite()
    {
        Sprite resourceSprite = LoadResourceSprite(fallbackSpritePath);
        if (resourceSprite != null)
        {
            return resourceSprite;
        }

        return GetFallbackSprite(Mathf.Max(8, fallbackSpriteSize));
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

    private static Sprite GetFallbackSprite(int size)
    {
        if (_cachedFallbackSprite != null && _cachedFallbackSize == size)
        {
            return _cachedFallbackSprite;
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
        _cachedFallbackSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _cachedFallbackSize = size;
        return _cachedFallbackSprite;
    }
}

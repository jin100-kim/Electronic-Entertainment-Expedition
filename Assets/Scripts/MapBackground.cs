using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapBackground : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private string backgroundSpritePath;

    [SerializeField]
    private float tileScale = 1f;

    [SerializeField]
    private Color tint = Color.white;

    [SerializeField]
    private int sortingOrder = -10;

    [SerializeField]
    private bool useGrid = true;

    [SerializeField]
    private int gridCellSize = 32;

    [SerializeField]
    private int gridLineThickness = 1;

    [SerializeField]
    private Color gridLineColor = new Color(0.3f, 0.35f, 0.45f, 0.35f);

    [SerializeField]
    private Color gridBackgroundColor = new Color(0.16f, 0.18f, 0.22f, 1f);

    [SerializeField]
    private bool useChecker = false;

    [SerializeField]
    private int checkerCellSize = 32;

    [SerializeField]
    private Color checkerColorA = new Color(0.6f, 0.85f, 0.55f, 1f);

    [SerializeField]
    private Color checkerColorB = Color.white;

    private SpriteRenderer _renderer;
    private bool _settingsApplied;
    private static Sprite _solidSprite;
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _gridSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _checkerSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();

    private void Awake()
    {
        ApplySettings();
        EnsureRenderer();
        ApplyVisuals();
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.mapBackground;
        backgroundSpritePath = settings.backgroundSpritePath;
        tileScale = settings.tileScale;
        tint = settings.tint;
        sortingOrder = settings.sortingOrder;
        useGrid = settings.useGrid;
        gridCellSize = settings.gridCellSize;
        gridLineThickness = settings.gridLineThickness;
        gridLineColor = settings.gridLineColor;
        gridBackgroundColor = settings.gridBackgroundColor;
        useChecker = settings.useChecker;
        checkerCellSize = settings.checkerCellSize;
        checkerColorA = settings.checkerColorA;
        checkerColorB = settings.checkerColorB;
        _settingsApplied = true;
    }

    private void EnsureRenderer()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        if (_renderer == null)
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void SetBounds(Vector2 halfSize)
    {
        ApplySettings();
        EnsureRenderer();
        ApplyVisuals();

        var size = new Vector2(Mathf.Max(0.1f, halfSize.x * 2f), Mathf.Max(0.1f, halfSize.y * 2f));
        float scale = Mathf.Max(0.1f, tileScale);
        _renderer.drawMode = SpriteDrawMode.Tiled;
        _renderer.size = size * scale;
        _renderer.sortingOrder = sortingOrder;
        _renderer.color = tint;
        transform.position = Vector3.zero;
    }

    private void ApplyVisuals()
    {
        if (_renderer == null)
        {
            return;
        }

        var sprite = ResolveBackgroundSprite();
        _renderer.sprite = sprite != null ? sprite : GetSolidSprite();

        _renderer.drawMode = SpriteDrawMode.Tiled;
        _renderer.sortingOrder = sortingOrder;
        _renderer.color = tint;
    }

    private Sprite ResolveBackgroundSprite()
    {
        if (useChecker)
        {
            return GetCheckerSprite(checkerCellSize, checkerColorA, checkerColorB);
        }

        if (useGrid)
        {
            return GetGridSprite(gridCellSize, gridLineThickness, gridLineColor, gridBackgroundColor);
        }

        var sprite = LoadResourceSprite(backgroundSpritePath);
        return sprite != null ? sprite : GetSolidSprite();
    }

    private static Sprite LoadResourceSprite(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Resources.Load<Sprite>(path);
    }

    private static Sprite GetGridSprite(int cellSize, int thickness, Color line, Color background)
    {
        int size = Mathf.Max(4, cellSize);
        int lineThickness = Mathf.Clamp(thickness, 1, size / 2);
        var lineColor = (Color32)line;
        var bgColor = (Color32)background;

        string key = $"{size}:{lineThickness}:{lineColor.r},{lineColor.g},{lineColor.b},{lineColor.a}:{bgColor.r},{bgColor.g},{bgColor.b},{bgColor.a}";
        if (_gridSpriteCache.TryGetValue(key, out var cached) && cached != null)
        {
            return cached;
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isLine = x < lineThickness || y < lineThickness;
                pixels[y * size + x] = isLine ? lineColor : bgColor;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _gridSpriteCache[key] = sprite;
        return sprite;
    }

    private static Sprite GetCheckerSprite(int cellSize, Color colorA, Color colorB)
    {
        int size = Mathf.Max(4, cellSize * 2);
        int half = size / 2;
        var a = (Color32)colorA;
        var b = (Color32)colorB;
        string key = $"{size}:{a.r},{a.g},{a.b},{a.a}:{b.r},{b.g},{b.b},{b.a}";

        if (_checkerSpriteCache.TryGetValue(key, out var cached) && cached != null)
        {
            return cached;
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool useA = (x < half && y < half) || (x >= half && y >= half);
                pixels[y * size + x] = useA ? a : b;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _checkerSpriteCache[key] = sprite;
        return sprite;
    }

    private static Sprite GetSolidSprite()
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
}

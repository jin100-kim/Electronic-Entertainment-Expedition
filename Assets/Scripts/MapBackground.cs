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

    private SpriteRenderer _renderer;
    private bool _settingsApplied;

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

        var sprite = LoadResourceSprite(backgroundSpritePath);
        if (sprite != null)
        {
            _renderer.sprite = sprite;
        }

        _renderer.drawMode = SpriteDrawMode.Tiled;
        _renderer.sortingOrder = sortingOrder;
        _renderer.color = tint;
    }

    private static Sprite LoadResourceSprite(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Resources.Load<Sprite>(path);
    }
}

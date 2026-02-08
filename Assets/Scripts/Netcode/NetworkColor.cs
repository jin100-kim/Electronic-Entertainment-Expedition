using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NetworkColor : NetworkBehaviour
{
    private const int DefaultCircleSize = 50;
    private static readonly System.Collections.Generic.Dictionary<int, Sprite> _circleCache = new System.Collections.Generic.Dictionary<int, Sprite>();
    private static Sprite _solidSprite;

    private readonly NetworkVariable<Color32> _color = new NetworkVariable<Color32>(new Color32(255, 255, 255, 255));
    private SpriteRenderer _renderer;

    public override void OnNetworkSpawn()
    {
        _renderer = GetComponent<SpriteRenderer>();
        EnsureDefaultSprite();
        ApplyColor(_color.Value);
        _color.OnValueChanged += OnColorChanged;
    }

    public override void OnNetworkDespawn()
    {
        _color.OnValueChanged -= OnColorChanged;
        base.OnNetworkDespawn();
    }

    public void SetColor(Color color)
    {
        if (NetworkSession.IsActive && !IsServer)
        {
            return;
        }

        var c = (Color32)color;
        _color.Value = c;
        ApplyColor(c);
    }

    private void OnColorChanged(Color32 previous, Color32 next)
    {
        ApplyColor(next);
    }

    private void ApplyColor(Color32 color)
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        _renderer.color = color;
    }

    private void EnsureDefaultSprite()
    {
        if (_renderer == null)
        {
            return;
        }

        if (_renderer.sprite != null)
        {
            return;
        }

        if (GetComponent<BoxCollider2D>() != null)
        {
            _renderer.sprite = GetSolidSprite();
            return;
        }

        _renderer.sprite = GetCircleSprite(DefaultCircleSize);
    }

    private static Sprite GetCircleSprite(int size)
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

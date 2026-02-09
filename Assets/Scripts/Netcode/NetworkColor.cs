using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class NetworkColor : NetworkBehaviour
{
    private const int DefaultCircleSize = 50;
    private const byte WeaponIdNone = 255;
    private static readonly System.Collections.Generic.Dictionary<int, Sprite> _circleCache = new System.Collections.Generic.Dictionary<int, Sprite>();
    private static Sprite _solidSprite;
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _resourceSpriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();

    private readonly NetworkVariable<Color32> _color = new NetworkVariable<Color32>(new Color32(255, 255, 255, 255));
    private readonly NetworkVariable<FixedString64Bytes> _spritePath = new NetworkVariable<FixedString64Bytes>(new FixedString64Bytes());
    private readonly NetworkVariable<byte> _weaponId = new NetworkVariable<byte>(WeaponIdNone);
    private SpriteRenderer _renderer;

    public override void OnNetworkSpawn()
    {
        _renderer = GetComponent<SpriteRenderer>();
        EnsureDefaultSprite();
        ApplySprite(_spritePath.Value.ToString());
        ApplyWeaponSprite(_weaponId.Value);
        ApplyColor(_color.Value);
        _color.OnValueChanged += OnColorChanged;
        _spritePath.OnValueChanged += OnSpritePathChanged;
        _weaponId.OnValueChanged += OnWeaponIdChanged;
    }

    public override void OnNetworkDespawn()
    {
        _color.OnValueChanged -= OnColorChanged;
        _spritePath.OnValueChanged -= OnSpritePathChanged;
        _weaponId.OnValueChanged -= OnWeaponIdChanged;
        base.OnNetworkDespawn();
    }

    public void SetColor(Color color)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        var c = (Color32)color;
        _color.Value = c;
        ApplyColor(c);
    }

    public void SetSpritePath(string resourcePath)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        var path = resourcePath ?? string.Empty;
        _spritePath.Value = path;
        ApplySprite(path);
    }

    public void SetWeaponId(byte weaponId)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        _weaponId.Value = weaponId;
        ApplyWeaponSprite(weaponId);
    }

    private void OnColorChanged(Color32 previous, Color32 next)
    {
        ApplyColor(next);
    }

    private void OnSpritePathChanged(FixedString64Bytes previous, FixedString64Bytes next)
    {
        ApplySprite(next.ToString());
    }

    private void OnWeaponIdChanged(byte previous, byte next)
    {
        ApplyWeaponSprite(next);
    }

    private void ApplyColor(Color32 color)
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        _renderer.color = color;
    }

    private void ApplySprite(string resourcePath)
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        if (string.IsNullOrEmpty(resourcePath))
        {
            if (_renderer.sprite == null)
            {
                EnsureDefaultSprite();
            }
            return;
        }

        var sprite = LoadResourceSprite(resourcePath);
        if (sprite != null)
        {
            _renderer.sprite = sprite;
        }
        else if (_renderer.sprite == null)
        {
            EnsureDefaultSprite();
        }
    }

    private void ApplyWeaponSprite(byte weaponId)
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        if (weaponId == WeaponIdNone)
        {
            ApplySprite(_spritePath.Value.ToString());
            return;
        }

        var path = GetWeaponSpritePath(weaponId);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var sprite = LoadResourceSprite(path);
        if (sprite != null)
        {
            _renderer.sprite = sprite;
        }
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
        if (sprite == null)
        {
            var sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0)
            {
                sprite = sprites[0];
            }
        }
        _resourceSpriteCache[path] = sprite;
        return sprite;
    }

    private static string GetWeaponSpritePath(byte weaponId)
    {
        var config = GameConfig.LoadOrCreate();
        var settings = config.autoAttack;
        switch ((AutoAttack.WeaponType)weaponId)
        {
            case AutoAttack.WeaponType.Straight:
                return settings.straightSpritePath;
            case AutoAttack.WeaponType.Boomerang:
                return settings.boomerangSpritePath;
            case AutoAttack.WeaponType.Nova:
                return settings.novaSpritePath;
            case AutoAttack.WeaponType.Shotgun:
                return settings.shotgunSpritePath;
            case AutoAttack.WeaponType.Drone:
                return settings.droneSpritePath;
            case AutoAttack.WeaponType.Shuriken:
                return settings.shurikenSpritePath;
            case AutoAttack.WeaponType.FrostOrb:
                return settings.frostSpritePath;
            default:
                return null;
        }
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

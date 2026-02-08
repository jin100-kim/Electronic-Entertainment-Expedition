using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    private bool useUGUI = true;
    private Vector2 size = new Vector2(180f, 180f);
    private Vector2 margin = new Vector2(12f, 12f);
    private Vector2 referenceResolution = new Vector2(1280f, 720f);
    private Font uiFont;
    private bool showCameraRect = true;
    private Color cameraRectColor = new Color(1f, 1f, 1f, 0.6f);
    private float cameraRectThickness = 1f;
    private float playerDotSize = 6f;
    private float enemyDotSize = 2f;
    private float weaponDotSize = 2f;
    private float borderThickness = 2f;
    private Color borderColor = new Color(1f, 1f, 1f, 0.5f);
    private Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);
    private int labelFontSize = 12;
    private string labelText = "미니맵";

    private Texture2D _dotTex;
    private Texture2D _bgTex;

    private Canvas _canvas;
    private RectTransform _imageRect;
    private RawImage _image;
    private Text _label;
    private Texture2D _mapTexture;
    private Color32[] _pixels;
    private int _texWidth;
    private int _texHeight;
    private bool _uiReady;
    private bool _settingsApplied;

    private void Awake()
    {
        ApplySettings();
        if (useUGUI)
        {
            BuildUGUI();
        }
        else
        {
            BuildOnGUITextures();
        }
    }

    private void Update()
    {
        if (!_settingsApplied)
        {
            ApplySettings();
        }

        if (!useUGUI)
        {
            return;
        }

        UpdateUGUI();
    }

    private void OnGUI()
    {
        if (useUGUI)
        {
            return;
        }

        var session = GameSession.Instance;
        if (session == null || !session.IsGameplayActive)
        {
            return;
        }

        Rect rect = new Rect(
            Screen.width - size.x - margin.x,
            Screen.height - size.y - margin.y,
            size.x,
            size.y);

        GUI.DrawTexture(rect, _bgTex);
        GUI.Box(rect, labelText);
        DrawBorder(rect);

        Vector2 half = session.MapHalfSize;
        if (half.x <= 0f || half.y <= 0f)
        {
            return;
        }

        DrawPlayerDot(rect, half);
        DrawEnemyDots(rect, half);
        DrawWeaponDots(rect, half);
        DrawCameraRect(rect, half);
    }

    private void BuildUGUI()
    {
        if (_uiReady)
        {
            return;
        }

        var canvasGo = new GameObject("MinimapCanvas");
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1050;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var imageGo = new GameObject("MinimapImage");
        imageGo.transform.SetParent(canvasGo.transform, false);
        _image = imageGo.AddComponent<RawImage>();
        _imageRect = imageGo.GetComponent<RectTransform>();
        _imageRect.anchorMin = new Vector2(1f, 0f);
        _imageRect.anchorMax = new Vector2(1f, 0f);
        _imageRect.pivot = new Vector2(1f, 0f);
        _imageRect.anchoredPosition = new Vector2(-margin.x, margin.y);
        _imageRect.sizeDelta = size;

        var fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label = CreateText(_imageRect, "Label", fontToUse, labelFontSize, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.9f));
        var labelRect = _label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(6f, -4f);
        labelRect.sizeDelta = new Vector2(120f, 18f);
        _label.text = labelText;

        EnsureTexture();
        _uiReady = true;
    }

    private void UpdateUGUI()
    {
        if (!_uiReady)
        {
            BuildUGUI();
        }

        var session = GameSession.Instance;
        if (session == null || !session.IsGameplayActive)
        {
            if (_imageRect != null)
            {
                _imageRect.gameObject.SetActive(false);
            }
            return;
        }

        if (_imageRect != null && !_imageRect.gameObject.activeSelf)
        {
            _imageRect.gameObject.SetActive(true);
        }

        EnsureTexture();
        RenderMinimap(session);
    }

    private void ApplySettings()
    {
        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.minimap;

        useUGUI = settings.useUGUI;
        size = settings.size;
        margin = settings.margin;
        referenceResolution = settings.referenceResolution;
        uiFont = settings.uiFont;
        showCameraRect = settings.showCameraRect;
        cameraRectColor = settings.cameraRectColor;
        cameraRectThickness = settings.cameraRectThickness;
        playerDotSize = settings.playerDotSize;
        enemyDotSize = settings.enemyDotSize;
        weaponDotSize = settings.weaponDotSize;
        borderThickness = settings.borderThickness;
        borderColor = settings.borderColor;
        backgroundColor = settings.backgroundColor;
        labelFontSize = settings.labelFontSize;
        labelText = string.IsNullOrEmpty(settings.labelText) ? "미니맵" : settings.labelText;
        _settingsApplied = true;
    }

    private void EnsureTexture()
    {
        int width = Mathf.Max(8, Mathf.RoundToInt(size.x));
        int height = Mathf.Max(8, Mathf.RoundToInt(size.y));
        if (_mapTexture != null && width == _texWidth && height == _texHeight)
        {
            return;
        }

        _texWidth = width;
        _texHeight = height;
        _mapTexture = new Texture2D(_texWidth, _texHeight, TextureFormat.RGBA32, false);
        _mapTexture.filterMode = FilterMode.Point;
        _mapTexture.wrapMode = TextureWrapMode.Clamp;
        _pixels = new Color32[_texWidth * _texHeight];
        _mapTexture.SetPixels32(_pixels);
        _mapTexture.Apply(false);

        if (_image != null)
        {
            _image.texture = _mapTexture;
        }
    }

    private void RenderMinimap(GameSession session)
    {
        if (_mapTexture == null || _pixels == null)
        {
            return;
        }

        var bg = (Color32)backgroundColor;
        for (int i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = bg;
        }

        DrawBorderPixels();

        Vector2 half = session.MapHalfSize;
        if (half.x > 0f && half.y > 0f)
        {
            var player = GetFirstPlayer();
            if (player != null)
            {
                DrawDot(WorldToPixel(player.transform.position, half), playerDotSize, new Color32(51, 255, 76, 255));
            }

            var enemies = EnemyController.Active;
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                DrawDot(WorldToPixel(enemy.transform.position, half), enemyDotSize, new Color32(255, 64, 64, 255));
            }

            DrawWeaponDots(half);
            DrawCameraRectPixels(half);
        }

        _mapTexture.SetPixels32(_pixels);
        _mapTexture.Apply(false);
    }

    private void DrawWeaponDots(Vector2 half)
    {
        var projectiles = Projectile.Active;
        for (int i = 0; i < projectiles.Count; i++)
        {
            var proj = projectiles[i];
            if (proj == null)
            {
                continue;
            }

            DrawDot(WorldToPixel(proj.transform.position, half), weaponDotSize, (Color32)Color.white);
        }

        var boomerangs = BoomerangProjectile.Active;
        for (int i = 0; i < boomerangs.Count; i++)
        {
            var boom = boomerangs[i];
            if (boom == null)
            {
                continue;
            }

            DrawDot(WorldToPixel(boom.transform.position, half), weaponDotSize, (Color32)Color.white);
        }

        var drones = DroneProjectile.Active;
        for (int i = 0; i < drones.Count; i++)
        {
            var drone = drones[i];
            if (drone == null)
            {
                continue;
            }

            DrawDot(WorldToPixel(drone.transform.position, half), weaponDotSize, (Color32)Color.white);
        }
    }

    private void DrawCameraRectPixels(Vector2 half)
    {
        if (!showCameraRect || _mapTexture == null)
        {
            return;
        }

        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Rect worldRect;
        if (!TryGetCameraWorldRect(cam, out worldRect))
        {
            return;
        }

        float minX = Mathf.Max(-half.x, worldRect.xMin);
        float maxX = Mathf.Min(half.x, worldRect.xMax);
        float minY = Mathf.Max(-half.y, worldRect.yMin);
        float maxY = Mathf.Min(half.y, worldRect.yMax);
        if (maxX <= minX || maxY <= minY)
        {
            return;
        }

        Vector2Int minPixel = WorldToPixel(new Vector3(minX, minY, 0f), half);
        Vector2Int maxPixel = WorldToPixel(new Vector3(maxX, maxY, 0f), half);
        int left = Mathf.Min(minPixel.x, maxPixel.x);
        int right = Mathf.Max(minPixel.x, maxPixel.x);
        int bottom = Mathf.Min(minPixel.y, maxPixel.y);
        int top = Mathf.Max(minPixel.y, maxPixel.y);

        int thickness = Mathf.Clamp(Mathf.RoundToInt(cameraRectThickness), 1, Mathf.Min(_texWidth, _texHeight));
        var color = (Color32)cameraRectColor;

        for (int x = left; x <= right; x++)
        {
            for (int t = 0; t < thickness; t++)
            {
                SetPixel(x, bottom + t, color);
                SetPixel(x, top - t, color);
            }
        }

        for (int y = bottom; y <= top; y++)
        {
            for (int t = 0; t < thickness; t++)
            {
                SetPixel(left + t, y, color);
                SetPixel(right - t, y, color);
            }
        }
    }

    private void DrawBorderPixels()
    {
        int thickness = Mathf.Clamp(Mathf.RoundToInt(borderThickness), 1, Mathf.Min(_texWidth, _texHeight));
        var color = (Color32)borderColor;

        for (int x = 0; x < _texWidth; x++)
        {
            for (int y = 0; y < thickness; y++)
            {
                SetPixel(x, y, color);
                SetPixel(x, _texHeight - 1 - y, color);
            }
        }

        for (int y = 0; y < _texHeight; y++)
        {
            for (int x = 0; x < thickness; x++)
            {
                SetPixel(x, y, color);
                SetPixel(_texWidth - 1 - x, y, color);
            }
        }
    }

    private void SetPixel(int x, int y, Color32 color)
    {
        if (x < 0 || x >= _texWidth || y < 0 || y >= _texHeight)
        {
            return;
        }

        _pixels[y * _texWidth + x] = color;
    }

    private void DrawDot(Vector2Int pixel, float sizeValue, Color32 color)
    {
        int radius = Mathf.Max(1, Mathf.RoundToInt(sizeValue * 0.5f));
        for (int y = -radius; y <= radius; y++)
        {
            int py = pixel.y + y;
            if (py < 0 || py >= _texHeight)
            {
                continue;
            }

            for (int x = -radius; x <= radius; x++)
            {
                int px = pixel.x + x;
                if (px < 0 || px >= _texWidth)
                {
                    continue;
                }

                _pixels[py * _texWidth + px] = color;
            }
        }
    }

    private Vector2Int WorldToPixel(Vector3 world, Vector2 half)
    {
        float nx = Mathf.InverseLerp(-half.x, half.x, world.x);
        float ny = Mathf.InverseLerp(-half.y, half.y, world.y);
        int x = Mathf.RoundToInt(Mathf.Clamp01(nx) * (_texWidth - 1));
        int y = Mathf.RoundToInt(Mathf.Clamp01(ny) * (_texHeight - 1));
        return new Vector2Int(x, y);
    }

    private PlayerController GetFirstPlayer()
    {
        var players = PlayerController.Active;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                return players[i];
            }
        }

        return null;
    }

    private void BuildOnGUITextures()
    {
        if (_dotTex == null)
        {
            _dotTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _dotTex.SetPixel(0, 0, Color.white);
            _dotTex.Apply();
        }

        if (_bgTex == null)
        {
            _bgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _bgTex.SetPixel(0, 0, backgroundColor);
            _bgTex.Apply();
        }
    }

    private void DrawBorder(Rect rect)
    {
        if (_dotTex == null)
        {
            return;
        }

        var prev = GUI.color;
        GUI.color = borderColor;

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, borderThickness), _dotTex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - borderThickness, rect.width, borderThickness), _dotTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, borderThickness, rect.height), _dotTex);
        GUI.DrawTexture(new Rect(rect.xMax - borderThickness, rect.y, borderThickness, rect.height), _dotTex);

        GUI.color = prev;
    }

    private void DrawPlayerDot(Rect rect, Vector2 half)
    {
        var player = GetFirstPlayer();
        if (player == null)
        {
            return;
        }

        Vector2 p = WorldToMinimap(rect, half, player.transform.position);
        DrawDot(p, playerDotSize, new Color(0.2f, 1f, 0.3f, 1f));
    }

    private void DrawEnemyDots(Rect rect, Vector2 half)
    {
        var enemies = EnemyController.Active;
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }
            if (enemy.IsDead)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, enemy.transform.position);
            DrawDot(p, enemyDotSize, new Color(1f, 0.2f, 0.2f, 1f));
        }
    }

    private void DrawWeaponDots(Rect rect, Vector2 half)
    {
        var projectiles = Projectile.Active;
        for (int i = 0; i < projectiles.Count; i++)
        {
            var proj = projectiles[i];
            if (proj == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, proj.transform.position);
            DrawDot(p, weaponDotSize, Color.white);
        }

        var boomerangs = BoomerangProjectile.Active;
        for (int i = 0; i < boomerangs.Count; i++)
        {
            var boom = boomerangs[i];
            if (boom == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, boom.transform.position);
            DrawDot(p, weaponDotSize, Color.white);
        }

        var drones = DroneProjectile.Active;
        for (int i = 0; i < drones.Count; i++)
        {
            var drone = drones[i];
            if (drone == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, drone.transform.position);
            DrawDot(p, weaponDotSize, Color.white);
        }
    }

    private void DrawCameraRect(Rect rect, Vector2 half)
    {
        if (!showCameraRect)
        {
            return;
        }

        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Rect worldRect;
        if (!TryGetCameraWorldRect(cam, out worldRect))
        {
            return;
        }

        float minX = Mathf.Max(-half.x, worldRect.xMin);
        float maxX = Mathf.Min(half.x, worldRect.xMax);
        float minY = Mathf.Max(-half.y, worldRect.yMin);
        float maxY = Mathf.Min(half.y, worldRect.yMax);
        if (maxX <= minX || maxY <= minY)
        {
            return;
        }

        Vector2 min = WorldToMinimap(rect, half, new Vector3(minX, minY, 0f));
        Vector2 max = WorldToMinimap(rect, half, new Vector3(maxX, maxY, 0f));
        float xMin = Mathf.Min(min.x, max.x);
        float xMax = Mathf.Max(min.x, max.x);
        float yMin = Mathf.Min(min.y, max.y);
        float yMax = Mathf.Max(min.y, max.y);
        Rect camRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

        DrawRectBorder(camRect, cameraRectThickness, cameraRectColor);
    }

    private Vector2 WorldToMinimap(Rect rect, Vector2 half, Vector3 world)
    {
        float nx = Mathf.InverseLerp(-half.x, half.x, world.x);
        float ny = Mathf.InverseLerp(-half.y, half.y, world.y);
        float x = rect.x + nx * rect.width;
        float y = rect.y + (1f - ny) * rect.height;
        return new Vector2(x, y);
    }

    private void DrawDot(Vector2 center, float sizeValue, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        Rect r = new Rect(center.x - sizeValue * 0.5f, center.y - sizeValue * 0.5f, sizeValue, sizeValue);
        GUI.DrawTexture(r, _dotTex);
        GUI.color = prev;
    }

    private void DrawRectBorder(Rect rect, float thicknessValue, Color color)
    {
        if (_dotTex == null)
        {
            return;
        }

        float thickness = Mathf.Max(1f, thicknessValue);
        var prev = GUI.color;
        GUI.color = color;

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), _dotTex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), _dotTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), _dotTex);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), _dotTex);

        GUI.color = prev;
    }

    private bool TryGetCameraWorldRect(Camera cam, out Rect worldRect)
    {
        worldRect = default;
        if (cam == null)
        {
            return false;
        }

        if (cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 pos = cam.transform.position;
            worldRect = new Rect(pos.x - halfWidth, pos.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
            return true;
        }

        float depth = Mathf.Abs(cam.transform.position.z);
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        worldRect = Rect.MinMaxRect(bl.x, bl.y, tr.x, tr.y);
        return true;
    }

    private static Text CreateText(Transform parent, string name, Font font, int fontSize, TextAnchor alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}

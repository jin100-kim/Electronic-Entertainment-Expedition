using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField]
    private Vector2 size = new Vector2(180f, 180f);

    [SerializeField]
    private Vector2 margin = new Vector2(12f, 12f);

    [SerializeField]
    private float playerDotSize = 6f;

    [SerializeField]
    private float enemyDotSize = 4f;

    [SerializeField]
    private float weaponDotSize = 3f;

    [SerializeField]
    private float borderThickness = 2f;

    [SerializeField]
    private Color borderColor = new Color(1f, 1f, 1f, 0.5f);

    private Texture2D _dotTex;
    private Texture2D _bgTex;

    private void Awake()
    {
        _dotTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _dotTex.SetPixel(0, 0, Color.white);
        _dotTex.Apply();

        _bgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.35f));
        _bgTex.Apply();
    }

    private void OnGUI()
    {
        var session = GameSession.Instance;
        if (session == null)
        {
            return;
        }

        Rect rect = new Rect(
            Screen.width - size.x - margin.x,
            margin.y,
            size.x,
            size.y);

        GUI.DrawTexture(rect, _bgTex);
        GUI.Box(rect, "MINIMAP");
        DrawBorder(rect);

        Vector2 half = session.MapHalfSize;
        if (half.x <= 0f || half.y <= 0f)
        {
            return;
        }

        DrawPlayerDot(rect, half);
        DrawEnemyDots(rect, half);
        DrawWeaponDots(rect, half);
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
        var player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            return;
        }

        Vector2 p = WorldToMinimap(rect, half, player.transform.position);
        DrawDot(p, playerDotSize, new Color(0.2f, 1f, 0.3f, 1f));
    }

    private void DrawEnemyDots(Rect rect, Vector2 half)
    {
        var enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, enemy.transform.position);
            DrawDot(p, enemyDotSize, new Color(1f, 0.2f, 0.2f, 1f));
        }
    }

    private void DrawWeaponDots(Rect rect, Vector2 half)
    {
        var projectiles = FindObjectsOfType<Projectile>();
        foreach (var proj in projectiles)
        {
            if (proj == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, proj.transform.position);
            DrawDot(p, weaponDotSize, Color.white);
        }

        var boomerangs = FindObjectsOfType<BoomerangProjectile>();
        foreach (var boom in boomerangs)
        {
            if (boom == null)
            {
                continue;
            }

            Vector2 p = WorldToMinimap(rect, half, boom.transform.position);
            DrawDot(p, weaponDotSize, Color.white);
        }
    }

    private Vector2 WorldToMinimap(Rect rect, Vector2 half, Vector3 world)
    {
        float nx = Mathf.InverseLerp(-half.x, half.x, world.x);
        float ny = Mathf.InverseLerp(-half.y, half.y, world.y);
        float x = rect.x + nx * rect.width;
        float y = rect.y + (1f - ny) * rect.height;
        return new Vector2(x, y);
    }

    private void DrawDot(Vector2 center, float size, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        Rect r = new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        GUI.DrawTexture(r, _dotTex);
        GUI.color = prev;
    }
}

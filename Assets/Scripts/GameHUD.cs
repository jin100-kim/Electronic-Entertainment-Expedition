using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [SerializeField]
    private float margin = 12f;

    [SerializeField]
    private float xpBarHeight = 18f;

    [SerializeField]
    private float iconSize = 28f;

    [SerializeField]
    private float iconGap = 6f;

    private GUIStyle _labelStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _iconStyle;
    private Texture2D _solidTex;
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _upgradeIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _weaponIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _statIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();

    private void OnGUI()
    {
        var session = GameSession.Instance;
        if (session == null)
        {
            return;
        }
        if (!session.IsGameplayActive)
        {
            return;
        }

        EnsureStyles();

        DrawTopXpBar(session);
        DrawTopTime(session);
        DrawTopRightInfo(session);
        DrawUpgradeIcons(session);
    }

    private void EnsureStyles()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.white;
        }

        if (_smallStyle == null)
        {
            _smallStyle = new GUIStyle(GUI.skin.label);
            _smallStyle.fontSize = 12;
            _smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
        }

        if (_iconStyle == null)
        {
            _iconStyle = new GUIStyle(GUI.skin.label);
            _iconStyle.fontSize = 11;
            _iconStyle.alignment = TextAnchor.MiddleCenter;
            _iconStyle.normal.textColor = Color.white;
        }

        if (_solidTex == null)
        {
            _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _solidTex.SetPixel(0, 0, Color.white);
            _solidTex.Apply();
        }
    }

    private void DrawTopXpBar(GameSession session)
    {
        int level = session.PlayerExperience != null ? session.PlayerExperience.Level : 1;
        float xp = session.PlayerExperience != null ? session.PlayerExperience.CurrentXp : 0f;
        float xpNext = session.PlayerExperience != null ? session.PlayerExperience.XpToNext : 0f;
        float ratio = xpNext <= 0f ? 0f : Mathf.Clamp01(xp / xpNext);

        float width = Screen.width - margin * 2f;
        Rect bg = new Rect(margin, margin, width, xpBarHeight);
        Rect fill = new Rect(margin, margin, width * ratio, xpBarHeight);

        var prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(bg, _solidTex);
        GUI.color = new Color(0.2f, 0.8f, 1f, 0.95f);
        GUI.DrawTexture(fill, _solidTex);
        GUI.color = prev;

        GUI.Label(new Rect(margin + 6f, margin - 2f, 120f, xpBarHeight + 4f), $"레벨 {level}", _labelStyle);
    }

    private void DrawTopTime(GameSession session)
    {
        int totalSeconds = Mathf.FloorToInt(session.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string timeText = $"{minutes:00}:{seconds:00}";

        var size = _labelStyle.CalcSize(new GUIContent(timeText));
        float x = (Screen.width - size.x) * 0.5f;
        float y = margin + xpBarHeight + 2f;
        GUI.Label(new Rect(x, y, size.x, size.y), timeText, _labelStyle);
    }

    private void DrawTopRightInfo(GameSession session)
    {
        string info = $"처치 {session.KillCount}  코인 {session.CoinCount}";
        var size = _smallStyle.CalcSize(new GUIContent(info));
        float x = Screen.width - margin - size.x;
        float y = margin - 2f;
        GUI.Label(new Rect(x, y, size.x, size.y), info, _smallStyle);
    }

    private void DrawUpgradeIcons(GameSession session)
    {
        session.GetUpgradeIconData(_upgradeIcons);
        if (_upgradeIcons.Count == 0)
        {
            return;
        }

        _weaponIcons.Clear();
        _statIcons.Clear();
        for (int i = 0; i < _upgradeIcons.Count; i++)
        {
            if (_upgradeIcons[i].IsWeapon)
            {
                _weaponIcons.Add(_upgradeIcons[i]);
            }
            else
            {
                _statIcons.Add(_upgradeIcons[i]);
            }
        }

        float startX = margin;
        float startY = margin + xpBarHeight + 28f;
        int perRow = Mathf.Max(1, Mathf.FloorToInt((Screen.width - margin * 2f) / (iconSize + iconGap)));
        int weaponRows = Mathf.Max(1, Mathf.CeilToInt(_weaponIcons.Count / (float)perRow));

        DrawIconRow(_weaponIcons, startX, startY, perRow);
        DrawIconRow(_statIcons, startX, startY + weaponRows * (iconSize + iconGap), perRow);
    }

    private void DrawIconRow(System.Collections.Generic.List<GameSession.UpgradeIconData> icons, float startX, float startY, int perRow)
    {
        if (icons == null || icons.Count == 0)
        {
            return;
        }

        for (int i = 0; i < icons.Count; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            float x = startX + col * (iconSize + iconGap);
            float y = startY + row * (iconSize + iconGap);
            Rect rect = new Rect(x, y, iconSize, iconSize);

            Color color;
            string label;
            GetIconStyle(icons[i], out color, out label);

            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _solidTex);
            GUI.color = prev;

            GUI.Label(rect, label, _iconStyle);

            string levelText = $"Lv{icons[i].Level}";
            var levelSize = _smallStyle.CalcSize(new GUIContent(levelText));
            GUI.Label(new Rect(rect.xMax - levelSize.x - 2f, rect.yMax - levelSize.y - 1f, levelSize.x, levelSize.y), levelText, _smallStyle);
        }
    }

    private void GetIconStyle(GameSession.UpgradeIconData data, out Color color, out string label)
    {
        if (data.IsWeapon)
        {
            label = data.Key;
            if (label.Length > 2)
            {
                label = label.Substring(0, 2);
            }

            color = new Color(0.85f, 0.55f, 0.2f, 0.95f);

            if (data.Key.Contains("총"))
            {
                color = new Color(0.8f, 0.7f, 0.2f, 0.95f);
                label = "총";
            }
            else if (data.Key.Contains("부메랑"))
            {
                color = new Color(0.2f, 0.9f, 0.5f, 0.95f);
                label = "부";
            }
            else if (data.Key.Contains("노바"))
            {
                color = new Color(0.5f, 0.6f, 0.95f, 0.95f);
                label = "노";
            }
            else if (data.Key.Contains("샷건"))
            {
                color = new Color(0.9f, 0.6f, 0.3f, 0.95f);
                label = "샷";
            }
            else if (data.Key.Contains("레이저"))
            {
                color = new Color(0.7f, 0.4f, 1f, 0.95f);
                label = "레";
            }
            else if (data.Key.Contains("체인"))
            {
                color = new Color(0.3f, 0.7f, 1f, 0.95f);
                label = "체";
            }
            else if (data.Key.Contains("드론"))
            {
                color = new Color(0.6f, 0.9f, 0.9f, 0.95f);
                label = "드";
            }
            else if (data.Key.Contains("수리"))
            {
                color = new Color(0.95f, 0.8f, 0.3f, 0.95f);
                label = "수";
            }
            else if (data.Key.Contains("빙결"))
            {
                color = new Color(0.3f, 0.8f, 1f, 0.95f);
                label = "빙";
            }
            else if (data.Key.Contains("번개"))
            {
                color = new Color(1f, 0.9f, 0.2f, 0.95f);
                label = "번";
            }
        }
        else
        {
            label = "업";
            color = new Color(0.2f, 0.6f, 0.3f, 0.95f);

            if (data.Key.Contains("공격력"))
            {
                label = "공";
                color = new Color(0.9f, 0.3f, 0.3f, 0.95f);
            }
            else if (data.Key.Contains("공격속도"))
            {
                label = "속";
                color = new Color(0.9f, 0.6f, 0.2f, 0.95f);
            }
            else if (data.Key.Contains("이동속도"))
            {
                label = "이";
                color = new Color(0.3f, 0.8f, 0.4f, 0.95f);
            }
            else if (data.Key.Contains("체력강화"))
            {
                label = "체";
                color = new Color(0.3f, 0.9f, 0.3f, 0.95f);
            }
            else if (data.Key.Contains("사거리"))
            {
                label = "사";
                color = new Color(0.4f, 0.7f, 1f, 0.95f);
            }
            else if (data.Key.Contains("경험치"))
            {
                label = "경";
                color = new Color(0.2f, 0.7f, 1f, 0.95f);
            }
            else if (data.Key.Contains("자석"))
            {
                label = "자";
                color = new Color(0.5f, 0.85f, 1f, 0.95f);
            }
            else if (data.Key.Contains("투사체수"))
            {
                label = "수";
                color = new Color(0.85f, 0.85f, 0.3f, 0.95f);
            }
            else if (data.Key.Contains("투사체크기") || data.Key.Contains("크기"))
            {
                label = "크";
                color = new Color(0.7f, 0.75f, 0.8f, 0.95f);
            }
            else if (data.Key.Contains("관통"))
            {
                label = "관";
                color = new Color(0.9f, 0.55f, 0.2f, 0.95f);
            }
        }
    }
}

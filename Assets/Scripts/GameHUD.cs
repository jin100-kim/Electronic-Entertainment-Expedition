using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField]
    private bool useUGUI = true;

    [SerializeField]
    private Vector2 referenceResolution = new Vector2(1280f, 720f);

    [SerializeField]
    private Font uiFont;

    [SerializeField]
    private float margin = 12f;

    [SerializeField]
    private float xpBarHeight = 18f;

    [SerializeField]
    private float iconSize = 28f;

    [SerializeField]
    private float iconGap = 6f;

    [SerializeField]
    private float iconStartOffsetX = 0f;

    [SerializeField]
    private int labelFontSize = 24;

    [SerializeField]
    private int smallFontSize = 20;

    [SerializeField]
    private int iconFontSize = 11;

    private GUIStyle _labelStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _iconStyle;
    private Texture2D _solidTex;
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _upgradeIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _weaponIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _statIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();

    private Canvas _canvas;
    private RectTransform _canvasRoot;
    private Image _xpBarBg;
    private RectTransform _xpFillRect;
    private Text _levelText;
    private Text _timeText;
    private Text _infoText;
    private RectTransform _iconRoot;
    private bool _uiReady;

    private class UpgradeIconUI
    {
        public RectTransform Rect;
        public Image Bg;
        public Text Label;
        public Text Level;
    }

    private readonly System.Collections.Generic.List<UpgradeIconUI> _weaponIconUI = new System.Collections.Generic.List<UpgradeIconUI>();
    private readonly System.Collections.Generic.List<UpgradeIconUI> _statIconUI = new System.Collections.Generic.List<UpgradeIconUI>();

    private void Awake()
    {
        if (useUGUI)
        {
            BuildUGUI();
        }
    }

    private void Update()
    {
        if (!useUGUI)
        {
            return;
        }

        var session = GameSession.Instance;
        if (session == null || !session.IsGameplayActive)
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.gameObject.SetActive(false);
            }
            return;
        }

        if (_canvasRoot != null && !_canvasRoot.gameObject.activeSelf)
        {
            _canvasRoot.gameObject.SetActive(true);
        }

        if (!_uiReady)
        {
            BuildUGUI();
        }

        UpdateUGUI(session);
    }

    private void OnGUI()
    {
        if (useUGUI)
        {
            return;
        }

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

    private void BuildUGUI()
    {
        if (_uiReady)
        {
            return;
        }

        var fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGo = new GameObject("HUDCanvas");
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1000;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        _canvasRoot = canvasGo.GetComponent<RectTransform>();

        var barGo = new GameObject("XpBar");
        barGo.transform.SetParent(canvasGo.transform, false);
        var barRect = barGo.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(-margin * 2f, xpBarHeight);
        barRect.anchoredPosition = new Vector2(0f, -margin);
        _xpBarBg = barGo.AddComponent<Image>();
        _xpBarBg.color = new Color(0f, 0f, 0f, 0.55f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barGo.transform, false);
        _xpFillRect = fillGo.AddComponent<RectTransform>();
        _xpFillRect.anchorMin = new Vector2(0f, 0f);
        _xpFillRect.anchorMax = new Vector2(0f, 1f);
        _xpFillRect.pivot = new Vector2(0f, 0.5f);
        _xpFillRect.offsetMin = Vector2.zero;
        _xpFillRect.offsetMax = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 1f, 0.95f);

        _levelText = CreateText(barGo.transform, "LevelText", fontToUse, labelFontSize, TextAnchor.MiddleLeft, Color.white);
        var levelRect = _levelText.rectTransform;
        levelRect.anchorMin = new Vector2(0f, 0f);
        levelRect.anchorMax = new Vector2(0f, 1f);
        levelRect.pivot = new Vector2(0f, 0.5f);
        levelRect.anchoredPosition = new Vector2(6f, 0f);
        levelRect.sizeDelta = new Vector2(200f, xpBarHeight + 4f);

        _timeText = CreateText(canvasGo.transform, "TimeText", fontToUse, labelFontSize, TextAnchor.MiddleCenter, Color.white);
        var timeRect = _timeText.rectTransform;
        timeRect.anchorMin = new Vector2(0.5f, 1f);
        timeRect.anchorMax = new Vector2(0.5f, 1f);
        timeRect.pivot = new Vector2(0.5f, 1f);
        timeRect.anchoredPosition = new Vector2(0f, -(margin + xpBarHeight + 4f));
        timeRect.sizeDelta = new Vector2(200f, 24f);

        _infoText = CreateText(canvasGo.transform, "InfoText", fontToUse, smallFontSize, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.9f));
        var infoRect = _infoText.rectTransform;
        infoRect.anchorMin = new Vector2(1f, 1f);
        infoRect.anchorMax = new Vector2(1f, 1f);
        infoRect.pivot = new Vector2(1f, 1f);
        infoRect.anchoredPosition = new Vector2(-margin, -(margin + xpBarHeight + 4f));
        infoRect.sizeDelta = new Vector2(260f, 22f);

        var iconsGo = new GameObject("UpgradeIcons");
        iconsGo.transform.SetParent(canvasGo.transform, false);
        _iconRoot = iconsGo.AddComponent<RectTransform>();
        _iconRoot.anchorMin = new Vector2(0f, 1f);
        _iconRoot.anchorMax = new Vector2(0f, 1f);
        _iconRoot.pivot = new Vector2(0f, 1f);
        _iconRoot.anchoredPosition = new Vector2(margin + iconStartOffsetX, -(margin + xpBarHeight + 28f));
        _iconRoot.sizeDelta = new Vector2(0f, 0f);

        _uiReady = true;
    }

    private void UpdateUGUI(GameSession session)
    {
        if (_xpFillRect == null)
        {
            return;
        }

        if (_timeText != null)
        {
            _timeText.fontSize = labelFontSize;
        }

        if (_infoText != null)
        {
            _infoText.fontSize = smallFontSize;
        }

        int level = session.PlayerExperience != null ? session.PlayerExperience.Level : 1;
        float xp = session.PlayerExperience != null ? session.PlayerExperience.CurrentXp : 0f;
        float xpNext = session.PlayerExperience != null ? session.PlayerExperience.XpToNext : 0f;
        float ratio = xpNext <= 0f ? 0f : Mathf.Clamp01(xp / xpNext);

        _xpFillRect.anchorMax = new Vector2(ratio, 1f);
        _levelText.text = $"레벨 {level}";

        int totalSeconds = Mathf.FloorToInt(session.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        _timeText.text = $"{minutes:00}:{seconds:00}";

        _infoText.text = $"처치 {session.KillCount}  코인 {session.CoinCount}";

        UpdateUpgradeIcons(session);
    }

    private void UpdateUpgradeIcons(GameSession session)
    {
        session.GetUpgradeIconData(_upgradeIcons);

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

        int perRow = Mathf.Max(1, Mathf.FloorToInt((Screen.width - margin * 2f - iconStartOffsetX) / (iconSize + iconGap)));
        int weaponRows = Mathf.Max(1, Mathf.CeilToInt(_weaponIcons.Count / (float)perRow));

        EnsureIconList(_weaponIconUI, _weaponIcons.Count);
        EnsureIconList(_statIconUI, _statIcons.Count);

        LayoutIcons(_weaponIconUI, _weaponIcons, perRow, 0f);
        LayoutIcons(_statIconUI, _statIcons, perRow, weaponRows * (iconSize + iconGap));
    }

    private void EnsureIconList(System.Collections.Generic.List<UpgradeIconUI> list, int count)
    {
        while (list.Count < count)
        {
            list.Add(CreateIcon(_iconRoot));
        }

        for (int i = 0; i < list.Count; i++)
        {
            list[i].Rect.gameObject.SetActive(i < count);
        }
    }

    private void LayoutIcons(System.Collections.Generic.List<UpgradeIconUI> uiList, System.Collections.Generic.List<GameSession.UpgradeIconData> dataList, int perRow, float yOffset)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            float x = col * (iconSize + iconGap);
            float y = -(row * (iconSize + iconGap) + yOffset);

            var ui = uiList[i];
            ui.Rect.anchoredPosition = new Vector2(x, y);

            Color color;
            string label;
            GetIconStyle(dataList[i], out color, out label);
            ui.Bg.color = color;
            ui.Label.text = label;
            ui.Level.text = $"Lv{dataList[i].Level}";
        }
    }

    private UpgradeIconUI CreateIcon(Transform parent)
    {
        var go = new GameObject("Icon");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(iconSize, iconSize);

        var bg = go.AddComponent<Image>();

        var fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var label = CreateText(go.transform, "Label", fontToUse, iconFontSize, TextAnchor.MiddleCenter, Color.white);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        var level = CreateText(go.transform, "Level", fontToUse, smallFontSize, TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.9f));
        var levelRect = level.rectTransform;
        levelRect.anchorMin = new Vector2(1f, 0f);
        levelRect.anchorMax = new Vector2(1f, 0f);
        levelRect.pivot = new Vector2(1f, 0f);
        levelRect.anchoredPosition = new Vector2(-2f, 2f);
        levelRect.sizeDelta = new Vector2(iconSize + 12f, smallFontSize + 6f);
        level.horizontalOverflow = HorizontalWrapMode.Overflow;
        level.verticalOverflow = VerticalWrapMode.Overflow;

        return new UpgradeIconUI
        {
            Rect = rect,
            Bg = bg,
            Label = label,
            Level = level
        };
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

    private void EnsureStyles()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.normal.textColor = Color.white;
        }

        if (_smallStyle == null)
        {
            _smallStyle = new GUIStyle(GUI.skin.label);
            _smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
        }

        if (_iconStyle == null)
        {
            _iconStyle = new GUIStyle(GUI.skin.label);
            _iconStyle.alignment = TextAnchor.MiddleCenter;
            _iconStyle.normal.textColor = Color.white;
        }
        
        _labelStyle.fontSize = labelFontSize;
        _smallStyle.fontSize = smallFontSize;
        _iconStyle.fontSize = iconFontSize;

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
        float y = margin + xpBarHeight + 2f;
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

        float startX = margin + iconStartOffsetX;
        float startY = margin + xpBarHeight + 28f;
        int perRow = Mathf.Max(1, Mathf.FloorToInt((Screen.width - margin * 2f - iconStartOffsetX) / (iconSize + iconGap)));
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

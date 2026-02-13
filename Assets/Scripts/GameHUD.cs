using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    private bool useUGUI = true;

    [SerializeField]
    private GameConfig gameConfig;

    private Vector2 referenceResolution = new Vector2(1280f, 720f);
    private Font uiFont;
    private float margin = 12f;
    private float xpBarHeight = 18f;
    private float iconSize = 56f;
    private float iconGap = 6f;
    private float iconStartOffsetX = 20f;
    private int labelFontSize = 24;
    private int smallFontSize = 20;
    private int iconFontSize = 11;
    private int iconLevelFontSize = 12;
    private readonly Color _iconSlotFillColor = new Color(0.08f, 0.1f, 0.14f, 0.92f);
    private readonly Color _iconSlotEmptyFillColor = new Color(0.08f, 0.1f, 0.14f, 0.28f);
    private readonly Color _iconSlotBorderColor = new Color(1f, 1f, 1f, 0.25f);

    private GUIStyle _labelStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _iconStyle;
    private GUIStyle _iconLevelStyle;
    private Texture2D _solidTex;
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _upgradeIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _weaponIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.List<GameSession.UpgradeIconData> _statIcons = new System.Collections.Generic.List<GameSession.UpgradeIconData>();
    private readonly System.Collections.Generic.Dictionary<string, int> _weaponIconOrder = new System.Collections.Generic.Dictionary<string, int>();
    private readonly System.Collections.Generic.Dictionary<string, int> _statIconOrder = new System.Collections.Generic.Dictionary<string, int>();
    private int _nextWeaponIconOrder;
    private int _nextStatIconOrder;

    private Canvas _canvas;
    private CanvasScaler _canvasScaler;
    private RectTransform _canvasRoot;
    private Image _xpBarBg;
    private RectTransform _xpFillRect;
    private Text _levelText;
    private Text _timeText;
    private Text _infoText;
    private Button _pauseButton;
    private RectTransform _iconRoot;
    private bool _uiReady;
    private bool _settingsApplied;

    private class UpgradeIconUI
    {
        public RectTransform Rect;
        public Image Bg;
        public Image Icon;
        public Text Label;
        public Text Level;
    }

    private readonly System.Collections.Generic.List<UpgradeIconUI> _weaponIconUI = new System.Collections.Generic.List<UpgradeIconUI>();
    private readonly System.Collections.Generic.List<UpgradeIconUI> _statIconUI = new System.Collections.Generic.List<UpgradeIconUI>();

    private void Awake()
    {
        ApplySettings();
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

        if (!_settingsApplied)
        {
            ApplySettings();
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

    private void ApplySettings()
    {
        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.hud;

        referenceResolution = settings.referenceResolution;
        uiFont = settings.uiFont;
        margin = settings.margin;
        xpBarHeight = settings.xpBarHeight;
        iconSize = settings.iconSize;
        iconGap = settings.iconGap;
        iconStartOffsetX = settings.iconStartOffsetX;
        labelFontSize = settings.labelFontSize;
        smallFontSize = settings.smallFontSize;
        iconFontSize = settings.iconFontSize;
        iconLevelFontSize = settings.iconLevelFontSize;
        _settingsApplied = true;
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
        _canvasScaler = canvasGo.AddComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = referenceResolution;
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        _canvasScaler.matchWidthOrHeight = 0.5f;
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

        _pauseButton = CreateButton(canvasGo.transform, "PauseButton", fontToUse, "일시정지", new Vector2(92f, 28f), new Color(0.2f, 0.2f, 0.2f, 0.95f));
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);

        var iconsGo = new GameObject("UpgradeIcons");
        iconsGo.transform.SetParent(canvasGo.transform, false);
        _iconRoot = iconsGo.AddComponent<RectTransform>();
        _iconRoot.anchorMin = new Vector2(0f, 1f);
        _iconRoot.anchorMax = new Vector2(0f, 1f);
        _iconRoot.pivot = new Vector2(0f, 1f);
        _iconRoot.anchoredPosition = new Vector2(margin + iconStartOffsetX, -(margin + xpBarHeight + 28f));
        _iconRoot.sizeDelta = new Vector2(0f, 0f);

        ApplyUGUILayout();

        _uiReady = true;
    }

    private void UpdateUGUI(GameSession session)
    {
        if (_xpFillRect == null)
        {
            return;
        }

        ApplyUGUILayout();

        if (_levelText != null)
        {
            _levelText.fontSize = labelFontSize;
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

        if (_infoText != null)
        {
            var infoRect = _infoText.rectTransform;
            float minWidth = 260f;
            float preferred = _infoText.preferredWidth + 12f;
            infoRect.sizeDelta = new Vector2(Mathf.Max(minWidth, preferred), smallFontSize + 6f);
        }

        if (_pauseButton != null)
        {
            var menu = session.GetComponent<InGameMenu>();
            _pauseButton.interactable = menu != null;
        }

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

        SortIconsByFirstSeen(_weaponIcons, _weaponIconOrder, ref _nextWeaponIconOrder);
        SortIconsByFirstSeen(_statIcons, _statIconOrder, ref _nextStatIconOrder);

        int playerLevel = session.PlayerExperience != null ? Mathf.Max(1, session.PlayerExperience.Level) : 1;
        int weaponUnlockedSlots = Mathf.Max(1, session.WeaponSlotLimit);
        int weaponSlots = Mathf.Max(_weaponIcons.Count, session.WeaponSlotCapacity);
        int statSlots = session.StatSlotLimit > 0
            ? Mathf.Max(_statIcons.Count, session.StatSlotLimit)
            : _statIcons.Count;

        float layoutWidth = GetLayoutWidth();
        int perRow = Mathf.Max(1, Mathf.FloorToInt((layoutWidth - margin * 2f - iconStartOffsetX) / (iconSize + iconGap)));
        int weaponRows = Mathf.Max(1, Mathf.CeilToInt(weaponSlots / (float)perRow));

        EnsureIconList(_weaponIconUI, weaponSlots);
        EnsureIconList(_statIconUI, statSlots);

        LayoutIcons(_weaponIconUI, _weaponIcons, perRow, 0f, true, weaponUnlockedSlots, playerLevel);
        LayoutIcons(_statIconUI, _statIcons, perRow, weaponRows * (iconSize + iconGap), false, 0, playerLevel);
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

    private static void SortIconsByFirstSeen(
        System.Collections.Generic.List<GameSession.UpgradeIconData> icons,
        System.Collections.Generic.Dictionary<string, int> orderMap,
        ref int nextOrder)
    {
        if (icons == null)
        {
            return;
        }

        for (int i = 0; i < icons.Count; i++)
        {
            string orderKey = BuildIconOrderKey(icons[i]);
            if (!orderMap.ContainsKey(orderKey))
            {
                orderMap[orderKey] = nextOrder;
                nextOrder += 1;
            }
        }

        icons.Sort((a, b) =>
        {
            string ak = BuildIconOrderKey(a);
            string bk = BuildIconOrderKey(b);
            int ao = orderMap.TryGetValue(ak, out var av) ? av : int.MaxValue;
            int bo = orderMap.TryGetValue(bk, out var bv) ? bv : int.MaxValue;
            if (ao != bo)
            {
                return ao.CompareTo(bo);
            }

            return string.Compare(ak, bk, System.StringComparison.Ordinal);
        });
    }

    private static string BuildIconOrderKey(GameSession.UpgradeIconData data)
    {
        string key = string.IsNullOrWhiteSpace(data.Key) ? "unknown" : data.Key.Trim();
        return $"{(data.IsWeapon ? "W" : "S")}:{key}";
    }

    private void LayoutIcons(
        System.Collections.Generic.List<UpgradeIconUI> uiList,
        System.Collections.Generic.List<GameSession.UpgradeIconData> dataList,
        int perRow,
        float yOffset,
        bool weaponRow,
        int unlockedWeaponSlots,
        int playerLevel)
    {
        int dataCount = dataList != null ? dataList.Count : 0;
        for (int i = 0; i < uiList.Count; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            float x = col * (iconSize + iconGap);
            float y = -(row * (iconSize + iconGap) + yOffset);

            var ui = uiList[i];
            ui.Rect.sizeDelta = new Vector2(iconSize, iconSize);
            ui.Rect.anchoredPosition = new Vector2(x, y);

            bool slotLocked = weaponRow && i >= unlockedWeaponSlots;
            bool hasData = i < dataCount;
            if (slotLocked)
            {
                ui.Bg.color = _iconSlotEmptyFillColor;
                if (ui.Icon != null)
                {
                    ui.Icon.enabled = false;
                }

                int unlockLevel = GetWeaponSlotUnlockLevel(i);
                ui.Label.text = unlockLevel > playerLevel ? $"Lv{unlockLevel}" : string.Empty;
                ui.Level.text = string.Empty;
                ui.Label.fontSize = iconFontSize;
                continue;
            }

            if (!hasData)
            {
                ui.Bg.color = _iconSlotEmptyFillColor;
                if (ui.Icon != null)
                {
                    ui.Icon.enabled = false;
                }

                ui.Label.text = string.Empty;
                ui.Level.text = string.Empty;
                continue;
            }

            Color color;
            string label;
            GetIconStyle(dataList[i], out color, out label);
            var iconSprite = UpgradeIconCatalog.ResolveSprite(dataList[i].Key, dataList[i].IsWeapon);

            ui.Bg.color = _iconSlotFillColor;
            if (ui.Icon != null)
            {
                ui.Icon.sprite = iconSprite;
                ui.Icon.enabled = iconSprite != null;
                ui.Icon.color = Color.white;
            }

            ui.Label.text = iconSprite == null ? label : string.Empty;
            ui.Level.text = $"Lv{dataList[i].Level}";
            ui.Label.fontSize = iconFontSize;
            ui.Level.fontSize = iconLevelFontSize;
            ui.Level.rectTransform.sizeDelta = new Vector2(iconSize + 12f, iconLevelFontSize + 6f);
        }
    }

    private static int GetWeaponSlotUnlockLevel(int slotIndex)
    {
        if (slotIndex <= 0)
        {
            return 1;
        }

        if (slotIndex == 1)
        {
            return 10;
        }

        if (slotIndex == 2)
        {
            return 20;
        }

        return 20 + (slotIndex - 2) * 10;
    }

    private UpgradeIconUI CreateIcon(Transform parent)
    {
        var go = new GameObject("Icon");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(iconSize, iconSize);

        var bg = go.AddComponent<Image>();
        bg.color = _iconSlotEmptyFillColor;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = _iconSlotBorderColor;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0f);
        iconRect.anchorMax = new Vector2(1f, 1f);
        iconRect.offsetMin = new Vector2(5f, 5f);
        iconRect.offsetMax = new Vector2(-5f, -5f);
        var icon = iconGo.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var label = CreateText(go.transform, "Label", fontToUse, iconFontSize, TextAnchor.MiddleCenter, Color.white);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        var level = CreateText(go.transform, "Level", fontToUse, iconLevelFontSize, TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.9f));
        var levelRect = level.rectTransform;
        levelRect.anchorMin = new Vector2(1f, 0f);
        levelRect.anchorMax = new Vector2(1f, 0f);
        levelRect.pivot = new Vector2(1f, 0f);
        levelRect.anchoredPosition = new Vector2(-2f, 2f);
        levelRect.sizeDelta = new Vector2(iconSize + 12f, iconLevelFontSize + 6f);
        level.horizontalOverflow = HorizontalWrapMode.Overflow;
        level.verticalOverflow = VerticalWrapMode.Overflow;

        return new UpgradeIconUI
        {
            Rect = rect,
            Bg = bg,
            Icon = icon,
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

    private static Button CreateButton(Transform parent, string name, Font font, string label, Vector2 size, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        var image = go.AddComponent<Image>();
        image.color = bgColor;

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var labelText = CreateText(go.transform, "Label", font, 14, TextAnchor.MiddleCenter, Color.white);
        var labelRect = labelText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        labelText.text = label;
        return button;
    }

    private void OnPauseButtonClicked()
    {
        var session = GameSession.Instance;
        if (session == null)
        {
            return;
        }

        var menu = session.GetComponent<InGameMenu>();
        if (menu != null)
        {
            menu.ToggleFromHud();
        }
    }

    private void ApplyUGUILayout()
    {
        if (_xpBarBg == null)
        {
            return;
        }

        var barRect = _xpBarBg.rectTransform;
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(-margin * 2f, xpBarHeight);
        barRect.anchoredPosition = new Vector2(0f, -margin);

        if (_levelText != null)
        {
            var levelRect = _levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0f, 0f);
            levelRect.anchorMax = new Vector2(0f, 1f);
            levelRect.pivot = new Vector2(0f, 0.5f);
            levelRect.anchoredPosition = new Vector2(6f, 0f);
            levelRect.sizeDelta = new Vector2(200f, xpBarHeight + 4f);
        }

        if (_timeText != null)
        {
            var timeRect = _timeText.rectTransform;
            timeRect.anchorMin = new Vector2(0.5f, 1f);
            timeRect.anchorMax = new Vector2(0.5f, 1f);
            timeRect.pivot = new Vector2(0.5f, 1f);
            timeRect.anchoredPosition = new Vector2(0f, -(margin + xpBarHeight + 4f));
            timeRect.sizeDelta = new Vector2(200f, labelFontSize + 6f);
        }

        if (_infoText != null)
        {
            var infoRect = _infoText.rectTransform;
            infoRect.anchorMin = new Vector2(1f, 1f);
            infoRect.anchorMax = new Vector2(1f, 1f);
            infoRect.pivot = new Vector2(1f, 1f);
            infoRect.anchoredPosition = new Vector2(-margin, -(margin + xpBarHeight + 4f));
            infoRect.sizeDelta = new Vector2(260f, smallFontSize + 6f);
        }

        if (_pauseButton != null)
        {
            var pauseRect = _pauseButton.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1f, 1f);
            pauseRect.anchorMax = new Vector2(1f, 1f);
            pauseRect.pivot = new Vector2(1f, 1f);
            float baseY = -(margin + xpBarHeight + 4f);
            float offsetY = smallFontSize + 10f;
            if (_infoText != null)
            {
                offsetY = _infoText.rectTransform.sizeDelta.y + 8f;
            }

            pauseRect.anchoredPosition = new Vector2(-margin, baseY - offsetY);
            pauseRect.sizeDelta = new Vector2(92f, 28f);
        }

        if (_iconRoot != null)
        {
            _iconRoot.anchoredPosition = new Vector2(margin + iconStartOffsetX, -(margin + xpBarHeight + 28f));
        }
    }

    private float GetLayoutWidth()
    {
        if (_canvasRoot != null)
        {
            float width = _canvasRoot.rect.width;
            if (width > 0f)
            {
                return width;
            }
        }

        return Screen.width;
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

        if (_iconLevelStyle == null)
        {
            _iconLevelStyle = new GUIStyle(GUI.skin.label);
            _iconLevelStyle.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
        }
        
        _labelStyle.fontSize = labelFontSize;
        _smallStyle.fontSize = smallFontSize;
        _iconStyle.fontSize = iconFontSize;
        _iconLevelStyle.fontSize = iconLevelFontSize;

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

        SortIconsByFirstSeen(_weaponIcons, _weaponIconOrder, ref _nextWeaponIconOrder);
        SortIconsByFirstSeen(_statIcons, _statIconOrder, ref _nextStatIconOrder);

        int playerLevel = session.PlayerExperience != null ? Mathf.Max(1, session.PlayerExperience.Level) : 1;
        int weaponUnlockedSlots = Mathf.Max(1, session.WeaponSlotLimit);
        int weaponSlots = Mathf.Max(_weaponIcons.Count, session.WeaponSlotCapacity);
        int statSlots = session.StatSlotLimit > 0
            ? Mathf.Max(_statIcons.Count, session.StatSlotLimit)
            : _statIcons.Count;

        float startX = margin + iconStartOffsetX;
        float startY = margin + xpBarHeight + 28f;
        int perRow = Mathf.Max(1, Mathf.FloorToInt((Screen.width - margin * 2f - iconStartOffsetX) / (iconSize + iconGap)));
        int weaponRows = Mathf.Max(1, Mathf.CeilToInt(weaponSlots / (float)perRow));

        DrawIconRow(_weaponIcons, startX, startY, perRow, weaponSlots, true, weaponUnlockedSlots, playerLevel);
        DrawIconRow(_statIcons, startX, startY + weaponRows * (iconSize + iconGap), perRow, statSlots, false, 0, playerLevel);
    }

    private void DrawIconRow(
        System.Collections.Generic.List<GameSession.UpgradeIconData> icons,
        float startX,
        float startY,
        int perRow,
        int slotCount,
        bool weaponRow,
        int unlockedWeaponSlots,
        int playerLevel)
    {
        if (slotCount <= 0)
        {
            return;
        }

        int iconCount = icons != null ? icons.Count : 0;
        for (int i = 0; i < slotCount; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            float x = startX + col * (iconSize + iconGap);
            float y = startY + row * (iconSize + iconGap);
            Rect rect = new Rect(x, y, iconSize, iconSize);

            bool slotLocked = weaponRow && i >= unlockedWeaponSlots;
            bool hasData = i < iconCount;
            var prev = GUI.color;
            GUI.color = hasData && !slotLocked ? _iconSlotFillColor : _iconSlotEmptyFillColor;
            GUI.DrawTexture(rect, _solidTex);
            GUI.color = _iconSlotBorderColor;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1f), _solidTex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), _solidTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1f, rect.height), _solidTex);
            GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), _solidTex);
            GUI.color = prev;

            if (slotLocked)
            {
                int unlockLevel = GetWeaponSlotUnlockLevel(i);
                if (unlockLevel > playerLevel)
                {
                    GUI.Label(rect, $"Lv{unlockLevel}", _iconStyle);
                }
                continue;
            }

            if (!hasData)
            {
                continue;
            }

            Color color;
            string label;
            GetIconStyle(icons[i], out color, out label);
            var iconSprite = UpgradeIconCatalog.ResolveSprite(icons[i].Key, icons[i].IsWeapon);

            if (iconSprite != null)
            {
                DrawSprite(rect, iconSprite, Color.white);
            }

            if (iconSprite == null)
            {
                GUI.Label(rect, label, _iconStyle);
            }

            string levelText = $"Lv{icons[i].Level}";
            var levelSize = _iconLevelStyle.CalcSize(new GUIContent(levelText));
            GUI.Label(new Rect(rect.xMax - levelSize.x - 2f, rect.yMax - levelSize.y - 1f, levelSize.x, levelSize.y), levelText, _iconLevelStyle);
        }
    }

    private static void DrawSprite(Rect rect, Sprite sprite, Color tint)
    {
        if (sprite == null || sprite.texture == null)
        {
            return;
        }

        var texture = sprite.texture;
        Rect tr = sprite.textureRect;
        Rect uv = new Rect(
            tr.x / texture.width,
            tr.y / texture.height,
            tr.width / texture.width,
            tr.height / texture.height);

        var prev = GUI.color;
        GUI.color = tint;
        GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
        GUI.color = prev;
    }

    private static bool ContainsAny(string source, params string[] terms)
    {
        if (string.IsNullOrEmpty(source) || terms == null)
        {
            return false;
        }

        for (int i = 0; i < terms.Length; i++)
        {
            string t = terms[i];
            if (string.IsNullOrWhiteSpace(t))
            {
                continue;
            }

            if (source.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private void GetIconStyle(GameSession.UpgradeIconData data, out Color color, out string label)
    {
        string key = data.Key ?? string.Empty;
        string keyNoSpace = key.Replace(" ", string.Empty);

        if (data.IsWeapon)
        {
            label = keyNoSpace;
            if (label.Length > 4)
            {
                label = label.Substring(0, 4);
            }

            color = new Color(0.85f, 0.55f, 0.2f, 0.95f);

            if (ContainsAny(key, "SingleShot", "총", "single"))
            {
                color = new Color(0.8f, 0.7f, 0.2f, 0.95f);
                label = "싱글샷";
            }
            else if (ContainsAny(key, "MultiShot", "부메랑", "boom"))
            {
                color = new Color(0.2f, 0.9f, 0.5f, 0.95f);
                label = "멀티샷";
            }
            else if (ContainsAny(key, "PiercingShot", "노바", "pierce"))
            {
                color = new Color(0.5f, 0.6f, 0.95f, 0.95f);
                label = "관통샷";
            }
            else if (ContainsAny(key, "Aura", "오라", "shotgun"))
            {
                color = new Color(0.9f, 0.6f, 0.3f, 0.95f);
                label = "오라";
            }
            else if (ContainsAny(key, "HomingShot", "호밍", "레이저", "homing"))
            {
                color = new Color(0.7f, 0.4f, 1f, 0.95f);
                label = "호밍";
            }
            else if (ContainsAny(key, "Grenade", "수류탄", "체인", "grenade"))
            {
                color = new Color(0.3f, 0.7f, 1f, 0.95f);
                label = "수류탄";
            }
            else if (ContainsAny(key, "Melee", "근접", "번개", "melee"))
            {
                color = new Color(1f, 0.9f, 0.2f, 0.95f);
                label = "근접";
            }
        }
        else
        {
            label = keyNoSpace;
            if (label.Length > 4)
            {
                label = label.Substring(0, 4);
            }
            color = new Color(0.2f, 0.6f, 0.3f, 0.95f);

            if (key.Contains("공격력"))
            {
                label = "공격력";
                color = new Color(0.9f, 0.3f, 0.3f, 0.95f);
            }
            else if (key.Contains("공격속도"))
            {
                label = "공격속도";
                color = new Color(0.9f, 0.6f, 0.2f, 0.95f);
            }
            else if (key.Contains("이동속도"))
            {
                label = "이동속도";
                color = new Color(0.3f, 0.8f, 0.4f, 0.95f);
            }
            else if (key.Contains("체력강화"))
            {
                label = "체력강화";
                color = new Color(0.3f, 0.9f, 0.3f, 0.95f);
            }
            else if (key.Contains("사거리"))
            {
                label = "사거리";
                color = new Color(0.4f, 0.7f, 1f, 0.95f);
            }
            else if (key.Contains("경험치"))
            {
                label = "경험치";
                color = new Color(0.2f, 0.7f, 1f, 0.95f);
            }
            else if (key.Contains("자석"))
            {
                label = "자석범위";
                color = new Color(0.5f, 0.85f, 1f, 0.95f);
            }
            else if (key.Contains("투사체수"))
            {
                label = "투사체수";
                color = new Color(0.85f, 0.85f, 0.3f, 0.95f);
            }
            else if (key.Contains("공격범위") || key.Contains("투사체크기") || key.Contains("크기"))
            {
                label = "공격범위";
                color = new Color(0.7f, 0.75f, 0.8f, 0.95f);
            }
            else if (key.Contains("관통"))
            {
                label = "관통력";
                color = new Color(0.9f, 0.55f, 0.2f, 0.95f);
            }
        }
    }
}

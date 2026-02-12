using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InGameMenu : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _panel;
    private bool _isOpen;

    private void Awake()
    {
        BuildUI();
        SetOpen(false);
    }

    private void Update()
    {
        if (WasEscapePressedThisFrame())
        {
            ToggleFromHud();
        }

        if (_isOpen && !CanOpen())
        {
            SetOpen(false);
        }
    }

    private static bool WasEscapePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private bool CanOpen()
    {
        var session = GameSession.Instance;
        return session != null && session.IsGameplayActive && !session.IsGameOver;
    }

    public void ToggleFromHud()
    {
        if (_isOpen)
        {
            SetOpen(false);
            return;
        }

        if (CanOpen())
        {
            SetOpen(true);
        }
    }

    private void BuildUI()
    {
        if (_canvas != null)
        {
            return;
        }

        var canvasGo = new GameObject("InGameMenuCanvas");
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 3000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        var root = canvasGo.GetComponent<RectTransform>();

        _panel = CreatePanel(root, "Panel", new Vector2(340f, 220f), new Color(0f, 0f, 0f, 0.75f));

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var title = CreateText(_panel, "Title", "일시정지", font, 22, TextAnchor.MiddleCenter);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);
        titleRect.sizeDelta = new Vector2(220f, 30f);

        var resume = CreateButton(_panel, "ResumeButton", "계속", font, new Vector2(0f, -62f));
        resume.onClick.AddListener(() => SetOpen(false));

        var main = CreateButton(_panel, "MainButton", "메인 화면", font, new Vector2(0f, -112f));
        main.onClick.AddListener(ReturnToMainMenu);

        var quit = CreateButton(_panel, "QuitButton", "게임 종료", font, new Vector2(0f, -162f));
        quit.onClick.AddListener(QuitGame);
    }

    private void SetOpen(bool open)
    {
        _isOpen = open;
        if (_canvas != null)
        {
            _canvas.gameObject.SetActive(open);
        }

        Time.timeScale = open ? 0f : 1f;
    }

    private void ReturnToMainMenu()
    {
        SetOpen(false);
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("SampleScene");
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = color;
        return rect;
    }

    private static Text CreateText(Transform parent, string name, string value, Font font, int fontSize, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Font font, Vector2 anchoredPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 38f);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var text = CreateText(go.transform, "Label", label, font, 16, TextAnchor.MiddleCenter);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }
}


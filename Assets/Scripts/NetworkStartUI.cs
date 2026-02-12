using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkStartUI : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    [Header("Buttons")]
    [SerializeField]
    private string hostButtonText = "Start Host";

    [SerializeField]
    private string clientButtonText = "Start Client";

    [SerializeField]
    private string localButtonText = "Start Local";

    [SerializeField]
    private Vector2 buttonSize = new Vector2(200f, 50f);

    [SerializeField]
    private float buttonSpacing = 10f;

    [Header("Connection")]
    [SerializeField]
    private string address = "127.0.0.1";

    [SerializeField]
    private ushort port = 7777;

    [Header("Local")]
    [SerializeField]
    private bool useImGuiFallback = false;

    [SerializeField]
    private Vector3 localSpawnPosition = Vector3.zero;

    private const string BuiltinFontPath = "LegacyRuntime.ttf";

    private Canvas _canvas;
    private bool _settingsApplied;

    private void Start()
    {
        ApplySettings();
        EnsureEventSystem();
        CreateButtons();
    }

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.networkUi;

        hostButtonText = settings.hostButtonText;
        clientButtonText = settings.clientButtonText;
        localButtonText = settings.localButtonText;
        buttonSize = settings.buttonSize;
        buttonSpacing = settings.buttonSpacing;
        address = settings.address;
        port = settings.port;
        useImGuiFallback = settings.useImGuiFallback;
        localSpawnPosition = settings.localSpawnPosition;

        _settingsApplied = true;
    }

    private void CreateButtons()
    {
        var existingCanvas = GameObject.Find("NetworkStartCanvas");
        if (existingCanvas != null)
        {
            _canvas = existingCanvas.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = existingCanvas.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                existingCanvas.AddComponent<CanvasScaler>();
                existingCanvas.AddComponent<GraphicRaycaster>();
            }
        }
        else
        {
            var canvasGo = new GameObject("NetworkStartCanvas");
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        float offset = buttonSize.y + buttonSpacing;
        CreateButton(_canvas.transform, hostButtonText, new Vector2(0f, offset), OnHostClicked);
        CreateButton(_canvas.transform, clientButtonText, new Vector2(0f, 0f), OnClientClicked);
        CreateButton(_canvas.transform, localButtonText, new Vector2(0f, -offset), OnLocalClicked);
    }

    private void CreateButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject(text);
        buttonGo.transform.SetParent(parent, false);

        var image = buttonGo.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var button = buttonGo.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var rect = buttonGo.GetComponent<RectTransform>();
        rect.sizeDelta = buttonSize;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);

        var label = textGo.AddComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>(BuiltinFontPath);

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void OnHostClicked()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        if (!NetworkManager.Singleton.IsListening)
        {
            ConfigureTransport();
            RuntimeNetworkPrefabs.EnsureRegistered();
            bool ok = NetworkManager.Singleton.StartHost();
            if (!ok)
            {
                Debug.LogError($"StartHost failed. Port {port} may be in use.");
            }
        }
    }

    private void OnClientClicked()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        if (!NetworkManager.Singleton.IsListening)
        {
            ConfigureTransport();
            RuntimeNetworkPrefabs.EnsureRegistered();
            bool ok = NetworkManager.Singleton.StartClient();
            if (!ok)
            {
                Debug.LogError($"StartClient failed. Check address {address}:{port}.");
            }
        }
    }

    private void OnLocalClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already running in network mode.");
            return;
        }

        var session = FindObjectOfType<GameSession>();
        if (session != null)
        {
            session.BeginLocalSession();
            return;
        }

        if (GameObject.Find("LocalPlayer") != null)
        {
            return;
        }

        var go = new GameObject("LocalPlayer");
        go.transform.position = localSpawnPosition;
        go.AddComponent<PlayerController>();
    }

    private void ConfigureTransport()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport not found on NetworkManager.");
            return;
        }

        transport.SetConnectionData(address, port);
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystem = eventSystemGo.AddComponent<EventSystem>();
        }

        var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputSystemModule == null)
        {
            var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Object.Destroy(legacyModule);
            }

            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void OnGUI()
    {
        if (!useImGuiFallback)
        {
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            return;
        }

        float totalHeight = buttonSize.y * 3f + buttonSpacing * 2f;
        float startY = (Screen.height - totalHeight) * 0.5f;
        float startX = (Screen.width - buttonSize.x) * 0.5f;

        var hostRect = new Rect(startX, startY, buttonSize.x, buttonSize.y);
        var clientRect = new Rect(startX, startY + buttonSize.y + buttonSpacing, buttonSize.x, buttonSize.y);
        var localRect = new Rect(startX, startY + (buttonSize.y + buttonSpacing) * 2f, buttonSize.x, buttonSize.y);

        if (GUI.Button(hostRect, hostButtonText))
        {
            OnHostClicked();
        }

        if (GUI.Button(clientRect, clientButtonText))
        {
            OnClientClicked();
        }

        if (GUI.Button(localRect, localButtonText))
        {
            OnLocalClicked();
        }
    }
}

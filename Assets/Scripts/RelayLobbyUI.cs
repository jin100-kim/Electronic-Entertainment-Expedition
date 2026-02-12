using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class RelayLobbyUI : MonoBehaviour
{
    [Header("Lobby")]
    [SerializeField]
    private string lobbyName = "MyLobby";

    [SerializeField]
    private int maxPlayers = 4;

    [SerializeField]
    private bool lobbyPrivate = true;

    [Header("Relay")]
    [SerializeField]
    private string connectionType = "dtls";

    [Header("UI")]
    [SerializeField]
    private bool useUGUI = true;

    [SerializeField]
    private bool showImGui = false;

    [SerializeField]
    private float lobbyPollInterval = 3f;

    private const string JoinCodeKey = "join_code";
    private const string GameStartedKey = "game_started";
    private const string SelectedMapKey = "selected_map";
    private const string CoinKey = "CoinCount";
    private const string BuiltinFontPath = "LegacyRuntime.ttf";
    private const string DefaultMapScene = "ForestOpenWorld";

    private enum ScreenState
    {
        Main,
        Join,
        Lobby,
        Upgrade,
        Hidden
    }

    private ScreenState _screenState = ScreenState.Main;
    private string _status = "준비됨";
    private string _lobbyCodeInput = string.Empty;
    private string _joinCode = string.Empty;
    private string _lobbyCode = string.Empty;
    private string _selectedMapScene = DefaultMapScene;
    private Lobby _currentLobby;
    private float _nextHeartbeatTime;
    private float _nextPollTime;
    private bool _gameStartTriggered;
    private bool _servicesReady;
    private bool _uiBuilt;

    private Canvas _canvas;
    private RectTransform _menuBackdrop;
    private RectTransform _mainPanel;
    private RectTransform _joinPanel;
    private RectTransform _lobbyPanel;
    private RectTransform _upgradePanel;

    private Text _statusText;
    private InputField _joinCodeInputField;
    private Text _lobbyCodeText;
    private Text _joinCodeText;
    private Text _mapInfoText;
    private Text _playersText;
    private Text _lobbyStatusText;
    private Button _startButton;
    private Text _coinText;
    private Text _upgradeLevelText;

    private readonly List<MapChoiceEntry> _mapChoices = new List<MapChoiceEntry>();

    private async void Awake()
    {
        if (useUGUI)
        {
            showImGui = false;
        }

        DisableLegacyNetworkStartUI();
        LoadMapChoices();

        if (NetworkManager.Singleton != null)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
        }

        await EnsureServices();
    }

    private void Start()
    {
        if (useUGUI)
        {
            BuildUGUI();
            SetScreen(ScreenState.Main);
        }
    }

    private void Update()
    {
        if (_currentLobby != null && AuthenticationService.Instance.IsSignedIn)
        {
            if (Time.unscaledTime >= _nextHeartbeatTime && IsHost())
            {
                _nextHeartbeatTime = Time.unscaledTime + 15f;
                _ = SendHeartbeat();
            }

            if (Time.unscaledTime >= _nextPollTime)
            {
                _nextPollTime = Time.unscaledTime + lobbyPollInterval;
                _ = RefreshLobby();
            }
        }

        if (useUGUI && _uiBuilt)
        {
            UpdateUGUI();
        }
    }

    private void LoadMapChoices()
    {
        _mapChoices.Clear();
        var config = GameConfig.LoadOrCreate();
        if (config != null && config.game != null && config.game.mapChoices != null && config.game.mapChoices.Length > 0)
        {
            for (int i = 0; i < config.game.mapChoices.Length; i++)
            {
                _mapChoices.Add(config.game.mapChoices[i]);
            }
        }

        if (_mapChoices.Count == 0)
        {
            _mapChoices.Add(new MapChoiceEntry { theme = MapTheme.Forest, displayName = "숲", sceneName = "ForestOpenWorld" });
            _mapChoices.Add(new MapChoiceEntry { theme = MapTheme.Desert, displayName = "사막", sceneName = "DesertOpenWorld" });
            _mapChoices.Add(new MapChoiceEntry { theme = MapTheme.Snow, displayName = "설원", sceneName = "SnowOpenWorld" });
        }

        if (string.IsNullOrWhiteSpace(_selectedMapScene))
        {
            _selectedMapScene = _mapChoices[0].sceneName;
        }
    }

    private async Task EnsureServices()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            _servicesReady = true;
            SetStatus("온라인 서비스 연결됨");
        }
        catch (Exception e)
        {
            _servicesReady = false;
            SetStatus($"서비스 연결 실패: {e.Message}");
        }
    }

    private void BuildUGUI()
    {
        if (_uiBuilt)
        {
            return;
        }

        EnsureEventSystem();

        var canvasGo = new GameObject("MainMenuCanvas");
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 2200;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var font = Resources.GetBuiltinResource<Font>(BuiltinFontPath);
        var root = canvasGo.GetComponent<RectTransform>();
        _menuBackdrop = CreateFullscreenPanel(root, "MenuBackdrop", new Color(0f, 0f, 0f, 0.72f));

        _mainPanel = CreatePanel(_menuBackdrop, "MainPanel", new Vector2(500f, 440f), new Color(0f, 0f, 0f, 0.88f));
        CreateTitle(_mainPanel, font, "전자오락원정대");
        CreateMenuButtons(font);
        _statusText = CreateText(_mainPanel, "Status", font, 13, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.9f));
        var statusRect = _statusText.rectTransform;
        statusRect.anchorMin = new Vector2(0.5f, 0f);
        statusRect.anchorMax = new Vector2(0.5f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.anchoredPosition = new Vector2(0f, 12f);
        statusRect.sizeDelta = new Vector2(420f, 28f);

        _joinPanel = CreatePanel(_menuBackdrop, "JoinPanel", new Vector2(460f, 260f), new Color(0f, 0f, 0f, 0.88f));
        var joinTitle = CreateText(_joinPanel, "JoinTitle", font, 20, TextAnchor.MiddleCenter, Color.white);
        joinTitle.text = "멀티 플레이 참여";
        joinTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        joinTitle.verticalOverflow = VerticalWrapMode.Truncate;
        var joinTitleRect = joinTitle.rectTransform;
        joinTitleRect.anchorMin = new Vector2(0.5f, 1f);
        joinTitleRect.anchorMax = new Vector2(0.5f, 1f);
        joinTitleRect.pivot = new Vector2(0.5f, 1f);
        joinTitleRect.anchoredPosition = new Vector2(0f, -24f);
        joinTitleRect.sizeDelta = new Vector2(360f, 32f);
        var inputBg = CreatePanel(_joinPanel, "InputBg", new Vector2(320f, 40f), new Color(0.18f, 0.18f, 0.18f, 0.95f));
        inputBg.anchorMin = new Vector2(0.5f, 1f);
        inputBg.anchorMax = new Vector2(0.5f, 1f);
        inputBg.pivot = new Vector2(0.5f, 1f);
        inputBg.anchoredPosition = new Vector2(0f, -74f);
        _joinCodeInputField = CreateInput(inputBg, font);
        CreateButton(_joinPanel, "JoinSubmit", font, "참여", new Vector2(0f, -128f), JoinLobbyByCodeAndClient);
        CreateButton(_joinPanel, "JoinBack", font, "뒤로", new Vector2(0f, -178f), () => SetScreen(ScreenState.Main));

        _lobbyPanel = CreatePanel(_menuBackdrop, "LobbyPanel", new Vector2(620f, 420f), new Color(0f, 0f, 0f, 0.88f));
        var lobbyTitle = CreateText(_lobbyPanel, "LobbyTitle", font, 20, TextAnchor.MiddleCenter, Color.white);
        lobbyTitle.text = "로비";
        var lobbyTitleRect = lobbyTitle.rectTransform;
        lobbyTitleRect.anchorMin = new Vector2(0.5f, 1f);
        lobbyTitleRect.anchorMax = new Vector2(0.5f, 1f);
        lobbyTitleRect.pivot = new Vector2(0.5f, 1f);
        lobbyTitleRect.anchoredPosition = new Vector2(0f, -20f);
        lobbyTitleRect.sizeDelta = new Vector2(240f, 32f);

        _lobbyCodeText = CreateText(_lobbyPanel, "LobbyCode", font, 15, TextAnchor.MiddleLeft, Color.white);
        var lobbyCodeRect = _lobbyCodeText.rectTransform;
        lobbyCodeRect.anchorMin = new Vector2(0f, 1f);
        lobbyCodeRect.anchorMax = new Vector2(0f, 1f);
        lobbyCodeRect.pivot = new Vector2(0f, 1f);
        lobbyCodeRect.anchoredPosition = new Vector2(18f, -58f);
        lobbyCodeRect.sizeDelta = new Vector2(360f, 24f);

        _joinCodeText = CreateText(_lobbyPanel, "JoinCode", font, 14, TextAnchor.MiddleLeft, Color.white);
        var joinCodeRect = _joinCodeText.rectTransform;
        joinCodeRect.anchorMin = new Vector2(0f, 1f);
        joinCodeRect.anchorMax = new Vector2(0f, 1f);
        joinCodeRect.pivot = new Vector2(0f, 1f);
        joinCodeRect.anchoredPosition = new Vector2(18f, -84f);
        joinCodeRect.sizeDelta = new Vector2(360f, 24f);

        _mapInfoText = CreateText(_lobbyPanel, "MapInfo", font, 14, TextAnchor.MiddleLeft, Color.white);
        var mapInfoRect = _mapInfoText.rectTransform;
        mapInfoRect.anchorMin = new Vector2(0f, 1f);
        mapInfoRect.anchorMax = new Vector2(0f, 1f);
        mapInfoRect.pivot = new Vector2(0f, 1f);
        mapInfoRect.anchoredPosition = new Vector2(18f, -110f);
        mapInfoRect.sizeDelta = new Vector2(360f, 24f);

        _playersText = CreateText(_lobbyPanel, "Players", font, 14, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.92f));
        var playersRect = _playersText.rectTransform;
        playersRect.anchorMin = new Vector2(0f, 1f);
        playersRect.anchorMax = new Vector2(0f, 1f);
        playersRect.pivot = new Vector2(0f, 1f);
        playersRect.anchoredPosition = new Vector2(18f, -138f);
        playersRect.sizeDelta = new Vector2(360f, 180f);

        _lobbyStatusText = CreateText(_lobbyPanel, "LobbyStatus", font, 13, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.88f));
        var lobbyStatusRect = _lobbyStatusText.rectTransform;
        lobbyStatusRect.anchorMin = new Vector2(0f, 0f);
        lobbyStatusRect.anchorMax = new Vector2(0f, 0f);
        lobbyStatusRect.pivot = new Vector2(0f, 0f);
        lobbyStatusRect.anchoredPosition = new Vector2(18f, 14f);
        lobbyStatusRect.sizeDelta = new Vector2(580f, 24f);

        BuildMapButtons(font);
        _startButton = CreateButton(_lobbyPanel, "StartButton", font, "게임 시작", new Vector2(148f, -280f), StartGameAsHost);
        CreateButton(_lobbyPanel, "LeaveButton", font, "로비 나가기", new Vector2(148f, -330f), LeaveLobbyAndReturnMain);

        _upgradePanel = CreatePanel(_menuBackdrop, "UpgradePanel", new Vector2(460f, 300f), new Color(0f, 0f, 0f, 0.88f));
        var upgradeTitle = CreateText(_upgradePanel, "UpgradeTitle", font, 20, TextAnchor.MiddleCenter, Color.white);
        upgradeTitle.text = "업그레이드";
        upgradeTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        upgradeTitle.verticalOverflow = VerticalWrapMode.Truncate;
        var upgradeTitleRect = upgradeTitle.rectTransform;
        upgradeTitleRect.anchorMin = new Vector2(0.5f, 1f);
        upgradeTitleRect.anchorMax = new Vector2(0.5f, 1f);
        upgradeTitleRect.pivot = new Vector2(0.5f, 1f);
        upgradeTitleRect.anchoredPosition = new Vector2(0f, -24f);
        upgradeTitleRect.sizeDelta = new Vector2(360f, 32f);

        _coinText = CreateText(_upgradePanel, "CoinText", font, 16, TextAnchor.MiddleCenter, Color.white);
        _coinText.horizontalOverflow = HorizontalWrapMode.Overflow;
        _coinText.verticalOverflow = VerticalWrapMode.Truncate;
        var coinRect = _coinText.rectTransform;
        coinRect.anchorMin = new Vector2(0.5f, 1f);
        coinRect.anchorMax = new Vector2(0.5f, 1f);
        coinRect.pivot = new Vector2(0.5f, 1f);
        coinRect.anchoredPosition = new Vector2(0f, -74f);
        coinRect.sizeDelta = new Vector2(360f, 26f);

        _upgradeLevelText = CreateText(_upgradePanel, "UpgradeLevelText", font, 14, TextAnchor.MiddleCenter, Color.white);
        _upgradeLevelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        _upgradeLevelText.verticalOverflow = VerticalWrapMode.Truncate;
        var levelRect = _upgradeLevelText.rectTransform;
        levelRect.anchorMin = new Vector2(0.5f, 1f);
        levelRect.anchorMax = new Vector2(0.5f, 1f);
        levelRect.pivot = new Vector2(0.5f, 1f);
        levelRect.anchoredPosition = new Vector2(0f, -106f);
        levelRect.sizeDelta = new Vector2(360f, 24f);

        CreateButton(_upgradePanel, "UpgradeBuyButton", font, "공격 영구강화 구매", new Vector2(0f, -144f), BuyPermanentUpgrade);
        CreateButton(_upgradePanel, "UpgradeBackButton", font, "뒤로", new Vector2(0f, -194f), () => SetScreen(ScreenState.Main));

        _uiBuilt = true;
    }

    private void CreateMenuButtons(Font font)
    {
        CreateButton(_mainPanel, "SingleButton", font, "싱글 플레이 시작", new Vector2(0f, -90f), StartLocalGame);
        CreateButton(_mainPanel, "CreateRoomButton", font, "멀티 방 생성", new Vector2(0f, -140f), CreateLobbyAndHost);
        CreateButton(_mainPanel, "JoinRoomButton", font, "멀티 플레이 참여", new Vector2(0f, -190f), () => SetScreen(ScreenState.Join));
        CreateButton(_mainPanel, "UpgradeButton", font, "업그레이드", new Vector2(0f, -240f), () => SetScreen(ScreenState.Upgrade));
        CreateButton(_mainPanel, "QuitButton", font, "게임 종료", new Vector2(0f, -290f), QuitGame);
    }

    private void BuildMapButtons(Font font)
    {
        for (int i = 0; i < _mapChoices.Count; i++)
        {
            var choice = _mapChoices[i];
            float x = 148f;
            float y = -74f - i * 46f;
            string label = GetMapLabel(choice);
            CreateButton(_lobbyPanel, $"MapButton_{i}", font, label, new Vector2(x, y), () => SelectMap(choice.sceneName));
        }
    }

    private void SetScreen(ScreenState state)
    {
        _screenState = state;
        if (!_uiBuilt)
        {
            return;
        }

        if (_menuBackdrop != null)
        {
            _menuBackdrop.gameObject.SetActive(state != ScreenState.Hidden);
        }

        _mainPanel.gameObject.SetActive(state == ScreenState.Main);
        _joinPanel.gameObject.SetActive(state == ScreenState.Join);
        _lobbyPanel.gameObject.SetActive(state == ScreenState.Lobby);
        _upgradePanel.gameObject.SetActive(state == ScreenState.Upgrade);
    }

    private void SetStatus(string value)
    {
        _status = value;
        if (_statusText != null)
        {
            _statusText.text = value;
        }

        if (_lobbyStatusText != null)
        {
            _lobbyStatusText.text = value;
        }
    }

    private void UpdateUGUI()
    {
        if (_coinText != null)
        {
            _coinText.text = $"보유 코인: {PlayerPrefs.GetInt(CoinKey, 0)}";
        }

        if (_upgradeLevelText != null)
        {
            int level = PlayerPrefs.GetInt("Meta_AttackLevel", 0);
            int cost = GetUpgradeCost(level);
            _upgradeLevelText.text = $"공격 영구강화 Lv.{level} (비용 {cost})";
        }

        if (_screenState == ScreenState.Join && _joinCodeInputField != null)
        {
            _lobbyCodeInput = _joinCodeInputField.text;
        }

        if (_screenState == ScreenState.Lobby)
        {
            UpdateLobbyTexts();
        }
    }

    private void UpdateLobbyTexts()
    {
        if (_lobbyCodeText != null)
        {
            _lobbyCodeText.text = $"로비 코드: {_lobbyCode}";
        }

        if (_joinCodeText != null)
        {
            _joinCodeText.text = $"릴레이 코드: {_joinCode}";
        }

        if (_mapInfoText != null)
        {
            _mapInfoText.text = $"선택 맵: {GetMapNameByScene(_selectedMapScene)}";
        }

        if (_playersText != null)
        {
            _playersText.text = BuildPlayersText();
        }

        if (_startButton != null)
        {
            _startButton.gameObject.SetActive(IsHost());
        }
    }

    private string BuildPlayersText()
    {
        if (_currentLobby == null)
        {
            return "플레이어 목록 없음";
        }

        var lines = new List<string>();
        var players = _currentLobby.Players ?? new List<Player>();
        lines.Add($"플레이어 {players.Count}/{_currentLobby.MaxPlayers}");
        for (int i = 0; i < players.Count; i++)
        {
            string id = players[i].Id;
            string shortId = id.Length > 6 ? id.Substring(0, 6) : id;
            string mine = id == AuthenticationService.Instance.PlayerId ? " (나)" : string.Empty;
            lines.Add($"- {shortId}{mine}");
        }
        return string.Join("\n", lines);
    }

    private bool IsHost()
    {
        return _currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task SendHeartbeat()
    {
        if (_currentLobby == null)
        {
            return;
        }

        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }
        catch
        {
            // Ignore heartbeat errors.
        }
    }

    private async Task RefreshLobby()
    {
        if (_currentLobby == null)
        {
            return;
        }

        try
        {
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            ReadSelectedMapFromLobby(_currentLobby);

            if (!_gameStartTriggered && TryReadGameStarted(_currentLobby))
            {
                _gameStartTriggered = true;
                ApplySelectedMapToGameSession();
                GameSession.StartNetworkGame();
                HideAllMenus();
            }
        }
        catch (Exception e)
        {
            SetStatus($"로비 갱신 실패: {e.Message}");
        }
    }

    private static bool TryReadGameStarted(Lobby lobby)
    {
        if (lobby == null || lobby.Data == null)
        {
            return false;
        }

        if (!lobby.Data.TryGetValue(GameStartedKey, out var data))
        {
            return false;
        }

        return string.Equals(data.Value, "1", StringComparison.OrdinalIgnoreCase);
    }

    private async void CreateLobbyAndHost()
    {
        if (!_servicesReady)
        {
            SetStatus("온라인 서비스가 준비되지 않았습니다.");
            return;
        }

        SetStatus("방 생성 중...");

        try
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var data = new Dictionary<string, DataObject>
            {
                { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, _joinCode) },
                { GameStartedKey, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { SelectedMapKey, new DataObject(DataObject.VisibilityOptions.Member, _selectedMapScene) }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = lobbyPrivate,
                Data = data
            });

            _lobbyCode = _currentLobby.LobbyCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool started = NetworkManager.Singleton.StartHost();
            if (!started)
            {
                SetStatus("호스트 시작 실패");
                return;
            }

            _gameStartTriggered = false;
            SetStatus("방 생성 완료");
            SetScreen(ScreenState.Lobby);
        }
        catch (Exception e)
        {
            SetStatus($"방 생성 실패: {e.Message}");
        }
    }

    private async void JoinLobbyByCodeAndClient()
    {
        if (!_servicesReady)
        {
            SetStatus("온라인 서비스가 준비되지 않았습니다.");
            return;
        }

        string code = _joinCodeInputField != null ? _joinCodeInputField.text : _lobbyCodeInput;
        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus("참여 코드가 필요합니다.");
            return;
        }

        SetStatus("방 참여 중...");

        try
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code.Trim());
            _lobbyCode = _currentLobby.LobbyCode;
            ReadSelectedMapFromLobby(_currentLobby);

            if (_currentLobby == null || !_currentLobby.Data.ContainsKey(JoinCodeKey))
            {
                SetStatus("릴레이 코드가 없습니다.");
                return;
            }

            string joinCode = _currentLobby.Data[JoinCodeKey].Value;
            _joinCode = joinCode;
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool started = NetworkManager.Singleton.StartClient();
            if (!started)
            {
                SetStatus("클라이언트 시작 실패");
                return;
            }

            _gameStartTriggered = false;
            SetStatus("방 참여 완료");
            SetScreen(ScreenState.Lobby);
        }
        catch (Exception e)
        {
            SetStatus($"방 참여 실패: {e.Message}");
        }
    }

    private async void StartGameAsHost()
    {
        if (_currentLobby == null)
        {
            SetStatus("로비가 없습니다.");
            return;
        }

        if (!IsHost())
        {
            SetStatus("호스트만 시작할 수 있습니다.");
            return;
        }

        try
        {
            await UpdateMapInLobby();
            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { GameStartedKey, new DataObject(DataObject.VisibilityOptions.Member, "1") },
                    { SelectedMapKey, new DataObject(DataObject.VisibilityOptions.Member, _selectedMapScene) }
                }
            });

            ApplySelectedMapToGameSession();
            _gameStartTriggered = true;
            GameSession.StartNetworkGame();
            HideAllMenus();
        }
        catch (Exception e)
        {
            SetStatus($"게임 시작 실패: {e.Message}");
        }
    }

    private void SelectMap(string sceneName)
    {
        if (!IsHost())
        {
            return;
        }

        _selectedMapScene = sceneName;
        _ = UpdateMapInLobby();
        ApplySelectedMapToGameSession();
    }

    private async Task UpdateMapInLobby()
    {
        if (_currentLobby == null || !IsHost())
        {
            return;
        }

        try
        {
            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { SelectedMapKey, new DataObject(DataObject.VisibilityOptions.Member, _selectedMapScene) }
                }
            });
        }
        catch (Exception e)
        {
            SetStatus($"맵 동기화 실패: {e.Message}");
        }
    }

    private void ApplySelectedMapToGameSession()
    {
        var session = FindObjectOfType<GameSession>();
        if (session != null)
        {
            session.TryPreselectMapBySceneName(_selectedMapScene);
        }
    }

    private void ReadSelectedMapFromLobby(Lobby lobby)
    {
        if (lobby == null || lobby.Data == null)
        {
            return;
        }

        if (lobby.Data.TryGetValue(SelectedMapKey, out var mapData) && !string.IsNullOrWhiteSpace(mapData.Value))
        {
            _selectedMapScene = mapData.Value;
        }
    }

    private async void LeaveLobbyAndReturnMain()
    {
        await LeaveLobby();
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        _gameStartTriggered = false;
        _currentLobby = null;
        _joinCode = string.Empty;
        _lobbyCode = string.Empty;
        SetStatus("메인 화면");
        SetScreen(ScreenState.Main);
    }

    private async Task LeaveLobby()
    {
        if (_currentLobby == null || !AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        try
        {
            if (IsHost())
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch
        {
            // Ignore leave errors.
        }
    }

    private void StartLocalGame()
    {
        _gameStartTriggered = true;
        var session = FindObjectOfType<GameSession>();
        if (session != null)
        {
            session.BeginLocalSession();
        }
        HideAllMenus();
    }

    private void HideAllMenus()
    {
        SetScreen(ScreenState.Hidden);
        if (_canvas != null)
        {
            _canvas.gameObject.SetActive(false);
        }
    }

    private void BuyPermanentUpgrade()
    {
        int level = PlayerPrefs.GetInt("Meta_AttackLevel", 0);
        int cost = GetUpgradeCost(level);
        int coins = PlayerPrefs.GetInt(CoinKey, 0);
        if (coins < cost)
        {
            SetStatus("코인이 부족합니다.");
            return;
        }

        coins -= cost;
        level += 1;
        PlayerPrefs.SetInt(CoinKey, coins);
        PlayerPrefs.SetInt("Meta_AttackLevel", level);
        PlayerPrefs.Save();
        SetStatus($"강화 완료 Lv.{level}");
    }

    private static int GetUpgradeCost(int level)
    {
        return 50 + level * 30;
    }

    private string GetMapNameByScene(string sceneName)
    {
        for (int i = 0; i < _mapChoices.Count; i++)
        {
            var choice = _mapChoices[i];
            if (string.Equals(choice.sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return GetMapLabel(choice);
            }
        }

        return sceneName;
    }

    private static string GetMapLabel(MapChoiceEntry choice)
    {
        string name = string.IsNullOrWhiteSpace(choice.displayName) ? choice.theme.ToString() : choice.displayName;
        string diff = choice.difficulty != null && !string.IsNullOrWhiteSpace(choice.difficulty.difficultyName)
            ? choice.difficulty.difficultyName
            : "Normal";
        return $"{name} ({diff})";
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = bgColor;
        return rect;
    }

    private static RectTransform CreateFullscreenPanel(Transform parent, string name, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = go.AddComponent<Image>();
        image.color = bgColor;
        return rect;
    }

    private static void CreateTitle(RectTransform parent, Font font, string value)
    {
        var text = CreateText(parent, "Title", font, 26, TextAnchor.MiddleCenter, Color.white);
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -30f);
        rect.sizeDelta = new Vector2(360f, 40f);
        text.text = value;
    }

    private static Text CreateText(Transform parent, string name, Font font, int size, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, Font font, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(280f, 40f);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateText(go.transform, "Label", font, 15, TextAnchor.MiddleCenter, Color.white);
        text.text = label;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }

    private static InputField CreateInput(RectTransform parent, Font font)
    {
        var inputGo = new GameObject("Input");
        inputGo.transform.SetParent(parent, false);
        var inputRect = inputGo.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = Vector2.one;
        inputRect.offsetMin = new Vector2(8f, 8f);
        inputRect.offsetMax = new Vector2(-8f, -8f);

        var text = CreateText(inputGo.transform, "Text", font, 16, TextAnchor.MiddleLeft, Color.white);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(8f, 0f);
        text.rectTransform.offsetMax = new Vector2(-8f, 0f);

        var placeholder = CreateText(inputGo.transform, "Placeholder", font, 16, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.4f));
        placeholder.text = "로비 코드 입력";
        placeholder.rectTransform.anchorMin = Vector2.zero;
        placeholder.rectTransform.anchorMax = Vector2.one;
        placeholder.rectTransform.offsetMin = new Vector2(8f, 0f);
        placeholder.rectTransform.offsetMax = new Vector2(-8f, 0f);

        var input = inputGo.AddComponent<InputField>();
        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            var go = new GameObject("EventSystem");
            eventSystem = go.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private static void DisableLegacyNetworkStartUI()
    {
        var legacy = FindObjectOfType<NetworkStartUI>();
        if (legacy != null)
        {
            legacy.gameObject.SetActive(false);
        }

        var canvas = GameObject.Find("NetworkStartCanvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (!showImGui || useUGUI)
        {
            return;
        }

        const float width = 420f;
        const float height = 240f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;
        GUI.Box(new Rect(x, y, width, height), "Relay Menu");
        GUI.Label(new Rect(x + 16f, y + 30f, width - 32f, 24f), _status);
        if (GUI.Button(new Rect(x + 16f, y + 64f, width - 32f, 32f), "싱글 플레이 시작"))
        {
            StartLocalGame();
        }
        if (GUI.Button(new Rect(x + 16f, y + 102f, width - 32f, 32f), "멀티 방 생성"))
        {
            CreateLobbyAndHost();
        }
        _lobbyCodeInput = GUI.TextField(new Rect(x + 16f, y + 142f, width - 32f, 24f), _lobbyCodeInput);
        if (GUI.Button(new Rect(x + 16f, y + 172f, width - 32f, 32f), "멀티 참여"))
        {
            JoinLobbyByCodeAndClient();
        }
    }
}


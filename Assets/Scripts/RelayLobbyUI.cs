using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

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
    private bool showImGui = true;

    [SerializeField]
    private float lobbyPollInterval = 3f;

    private string _lobbyCodeInput = "";
    private string _status = "Idle";
    private string _joinCode = "";
    private string _lobbyCode = "";
    private Lobby _currentLobby;
    private float _nextHeartbeatTime;
    private float _nextPollTime;
    private bool _gameStartTriggered;

    private const string JoinCodeKey = "join_code";
    private const string GameStartedKey = "game_started";

    private async void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
        }
        await EnsureServices();
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
        }
        catch (Exception e)
        {
            _status = $"UGS init failed: {e.Message}";
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
    }

    private bool IsHost()
    {
        return _currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }
        catch
        {
            // Ignore heartbeat failures in this minimal setup.
        }
    }

    private async Task RefreshLobby()
    {
        try
        {
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            if (!_gameStartTriggered && TryReadGameStarted(_currentLobby))
            {
                _gameStartTriggered = true;
                GameSession.StartNetworkGame();
                DisableSelfUI();
            }
        }
        catch
        {
            // Ignore refresh failures in this minimal setup.
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
        _status = "Creating lobby...";
        await EnsureServices();

        try
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var data = new Dictionary<string, DataObject>
            {
                { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, _joinCode) },
                { GameStartedKey, new DataObject(DataObject.VisibilityOptions.Member, "0") }
            };

            var options = new CreateLobbyOptions
            {
                IsPrivate = lobbyPrivate,
                Data = data
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            _lobbyCode = _currentLobby.LobbyCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool started = NetworkManager.Singleton.StartHost();
            _status = started ? $"Host started. Lobby Code: {_lobbyCode}" : "Host start failed.";
        }
        catch (Exception e)
        {
            _status = $"Create failed: {e.Message}";
        }
    }

    private async void JoinLobbyByCodeAndClient()
    {
        _status = "Joining lobby...";
        await EnsureServices();

        if (string.IsNullOrWhiteSpace(_lobbyCodeInput))
        {
            _status = "Enter lobby code.";
            return;
        }

        try
        {
            RuntimeNetworkPrefabs.EnsureRegistered();
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_lobbyCodeInput.Trim());
            _lobbyCode = _currentLobby.LobbyCode;

            if (_currentLobby == null || !_currentLobby.Data.ContainsKey(JoinCodeKey))
            {
                _status = "Lobby join code not found.";
                return;
            }

            string joinCode = _currentLobby.Data[JoinCodeKey].Value;
            _joinCode = joinCode;
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool started = NetworkManager.Singleton.StartClient();
            _status = started ? "Client started. Waiting for host..." : "Client start failed.";
        }
        catch (Exception e)
        {
            _status = $"Join failed: {e.Message}";
        }
    }

    private async void StartGameAsHost()
    {
        if (_currentLobby == null)
        {
            return;
        }

        if (!IsHost())
        {
            _status = "Only host can start the game.";
            return;
        }

        try
        {
            var data = new Dictionary<string, DataObject>
            {
                { GameStartedKey, new DataObject(DataObject.VisibilityOptions.Member, "1") }
            };

            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = data
            });

            _gameStartTriggered = true;
            GameSession.StartNetworkGame();
            DisableSelfUI();
        }
        catch (Exception e)
        {
            _status = $"Start game failed: {e.Message}";
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
        DisableSelfUI();
    }

    private void DisableSelfUI()
    {
        showImGui = false;
        var canvas = GameObject.Find("NetworkStartCanvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (!showImGui)
        {
            return;
        }

        const float w = 500f;
        const float h = 330f;
        float x = 20f;
        float y = 20f;

        GUI.Box(new Rect(x, y, w, h), "Relay + Lobby");

        GUI.Label(new Rect(x + 12f, y + 28f, w - 24f, 20f), $"Status: {_status}");
        GUI.Label(new Rect(x + 12f, y + 52f, w - 24f, 20f), $"Lobby Code: {_lobbyCode}");
        GUI.Label(new Rect(x + 12f, y + 72f, w - 24f, 20f), $"Relay Join Code: {_joinCode}");

        GUI.Label(new Rect(x + 12f, y + 100f, 120f, 20f), "Lobby Code:");
        _lobbyCodeInput = GUI.TextField(new Rect(x + 130f, y + 98f, 160f, 22f), _lobbyCodeInput);

        if (GUI.Button(new Rect(x + 12f, y + 132f, 200f, 28f), "Create Lobby + Host"))
        {
            CreateLobbyAndHost();
        }

        if (GUI.Button(new Rect(x + 230f, y + 132f, 200f, 28f), "Join Lobby + Client"))
        {
            JoinLobbyByCodeAndClient();
        }

        if (GUI.Button(new Rect(x + 12f, y + 166f, 200f, 28f), "Start Game (Host)"))
        {
            StartGameAsHost();
        }

        if (GUI.Button(new Rect(x + 230f, y + 166f, 200f, 28f), "Start Local"))
        {
            StartLocalGame();
        }

        GUI.Label(new Rect(x + 12f, y + 202f, w - 24f, 20f), "Host shares Lobby Code. Client enters Lobby Code.");

        DrawPlayerList(new Rect(x + 12f, y + 228f, w - 24f, 90f));
    }

    private void DrawPlayerList(Rect rect)
    {
        GUI.Box(rect, "Players");
        if (_currentLobby == null)
        {
            GUI.Label(new Rect(rect.x + 8f, rect.y + 20f, rect.width - 16f, 20f), "No lobby joined.");
            return;
        }

        float y = rect.y + 20f;
        foreach (var p in _currentLobby.Players ?? new List<Player>())
        {
            string name = p.Id == AuthenticationService.Instance.PlayerId ? "(You)" : "";
            GUI.Label(new Rect(rect.x + 8f, y, rect.width - 16f, 20f), $"Player {p.Id.Substring(0, 6)} {name}");
            y += 18f;
        }
    }
}

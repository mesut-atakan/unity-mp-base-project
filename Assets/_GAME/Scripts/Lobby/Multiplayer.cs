using Unity.Services.Lobbies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Aventra.Game
{
    [System.Serializable]
    public enum EncryptionType
    {
        [Tooltip("Datagram Transport Layer Security")]
        DTLS,
        [Tooltip("Web Socket Secure")]
        WSS
    }
    // Note: Also Udp and Ws are possible choices

    /// <summary>
    /// Oyuncunun adi
    /// Kimlik Dogrulamasi
    /// Lobi ve Rolley ilgili her seyi yonetecek bir singelton olacak.
    /// </summary>
    public class Multiplayer : MonoBehaviour
    {
        public const string DISPLAY_NAME = "displayName";
        private const float LOBBY_HEARTBEAT_INTERVAL = 20.0f;
        private const float LOBBY_POLL_INTERVAL = 65.0f;
        private const string KEY_JOIN_CODE = "RelayJoinCode";
        private const string DTLS_ENCRYPTION = "dtls"; // Datagram Transport Layer Security
        private const string WSS_ENCRYPTION = "wss"; // Web Socket Secure, use for WebGL builds

        public event Action<IReadOnlyList<Player>> OnLobbyPlayersChanged;

        //[SerializeField] private string lobbyName = "Lobby";
        //[SerializeField] private int maxPlayer = 4;
        [SerializeField] private EncryptionType encryption = EncryptionType.DTLS;

        public static Multiplayer Instance { get; private set; }
        public string PlayerID { get; private set; }
        public string PlayerName { get; private set; }

        public Lobby CurrentLobby { get; private set; }
        public bool IsEnteredLobby => CurrentLobby != null;


        private float _heartbeatTimer;
        private float _pollTimer;
        private bool _signedInHooked;
        private HashSet<string> _lastPlayerIds = new HashSet<string>();
        private ILobbyEvents _subscription;

        public bool IsLobbyHost =>
            CurrentLobby != null &&
            AuthenticationService.Instance.IsSignedIn &&
            CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            LobbyHearbeatTimer();
        }

        public async Task<bool> CreateUser(string userName)
        {
            return await Authenticate(userName);
        }

        public async Task<bool> CreateLobby(string lobbyName, int maxPlayer)
        {
            try
            {
                Allocation allocation = await AllocateRelay(maxPlayer);
                string joinCode = await GetRelayJoinCode(allocation);
                Player me = AddLobbyPlayer();
                CreateLobbyOptions options = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Player = me
                };

                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
                Debug.Log($"Created lobby: {CurrentLobby.Name}\twith code: { CurrentLobby.LobbyCode}", gameObject);

                ResetTimer();

                await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
                    }
                });

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                ConfigureTransportForHost(allocation, transport);
                NetworkManager.Singleton.StartHost();
                RaisePlayersChangedIfNeeded(CurrentLobby.Players);
                await SubscribeLobbyEventAsync();
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to create lobby: {e.Message}", gameObject);
                return false;
            }
        }

        public async Task<bool> JoinLobby(string joinCode)
        {
            try
            {
                Player me = AddLobbyPlayer();
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, new JoinLobbyByCodeOptions { 
                    Player = me});
                ResetTimer();

                string relayJoinCode = CurrentLobby.Data[KEY_JOIN_CODE].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                ConfigureTransportForClient(joinAllocation, unityTransport);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Joined lobby: {CurrentLobby.Name}\tWith Code: {CurrentLobby.LobbyCode}", gameObject);
                RaisePlayersChangedIfNeeded(CurrentLobby.Players);
                await SubscribeLobbyEventAsync();
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to allocate relay {e.Message}", gameObject);
                return false;
            }
        }

        public async Task<bool> QuickJoinLobby()
        {
            try
            {
                Player me = AddLobbyPlayer();
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(
                    new QuickJoinLobbyOptions { Player = me});
                ResetTimer();

                string relayJoinCode = CurrentLobby.Data[KEY_JOIN_CODE].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                ConfigureTransportForClient(joinAllocation, unityTransport);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Joined lobby: {CurrentLobby.Name}\tWith Code: {CurrentLobby.LobbyCode}", gameObject);
                RaisePlayersChangedIfNeeded(CurrentLobby.Players);
                await SubscribeLobbyEventAsync();
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to allocate relay: {e.Message}", gameObject);
                return false;
            }
        }

        public string GetJoinCode()
        {
            return CurrentLobby.LobbyCode;
        }

        private void RaisePlayersChangedIfNeeded(IReadOnlyList<Player> players)
        {
            var current = new HashSet<string>(players.Select(p => p.Id));
            if (!_lastPlayerIds.SetEquals(current))
            {
                _lastPlayerIds = current;
                OnLobbyPlayersChanged?.Invoke(players);
            }
        }

        private async void OnLobbyChangedPush()
        {
            // Push geldiðinde snapshot’ý çek, sonra UI event’ini tetikle
            try
            {
                var lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                CurrentLobby = lobby;
                RaisePlayersChangedIfNeeded(CurrentLobby.Players);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Push update failed: {e.Message}");
            }
        }

        private async Task SubscribeLobbyEventAsync()
        {
            var callbacks = new LobbyEventCallbacks();

            // Oyuncu girdi/çýktý
            callbacks.PlayerJoined += _ => OnLobbyChangedPush();
            callbacks.PlayerLeft += _ => OnLobbyChangedPush();

            // (Ýsteðe baðlý) veri/isim deðiþimleri vs. varsa onlarý da tek yerden yakala
            callbacks.DataChanged += _ => OnLobbyChangedPush();
            callbacks.LobbyChanged += _ => OnLobbyChangedPush();

            // Not: Bazý sürümlerde bazý callback'ler olmayabilir;
            // olanlarý eklemen yeterli.

            _subscription = await LobbyService.Instance
                .SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
        }

        private async Task<Allocation> AllocateRelay(int maxPlayer)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer - 1); // maxPlayer '-1' oldugundan emin ol cunku ana bilgisayari saymaz!
                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to allocate relay: {e.Message}", gameObject);
                return default;
            }
        }

        private async Task<bool> Authenticate(string playerName)
        {
            try
            {
                // 1) Initialize (profil istersen kalabilir ama isim deðildir)
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    var options = new InitializationOptions()
                        .SetProfile(playerName); // farklý cihaz/profil simülasyonu için faydalý, ama isim deðil
                    await UnityServices.InitializeAsync(options);
                }

                // 2) Event'i 1 kez baðla
                if (!_signedInHooked)
                {
                    AuthenticationService.Instance.SignedIn += () =>
                        Debug.Log($"Signed in. ID: {AuthenticationService.Instance.PlayerId} | Name(now): {AuthenticationService.Instance.PlayerName}");
                    _signedInHooked = true;
                }

                // 3) Giriþ yap
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                // 4) Görünen ismi ayarla (esas kýsým)
                if (AuthenticationService.Instance.PlayerName != playerName)
                {
                    try
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                    }
                    catch (AuthenticationException ex)
                    {
                        Debug.LogWarning($"Player name set failed (will continue): {ex.Message}");
                    }
                }

                // 5) Yerel alanlarý güncelle
                PlayerID = AuthenticationService.Instance.PlayerId;
                PlayerName = AuthenticationService.Instance.PlayerName; // artýk playerName olmalý

                Debug.Log($"Player ready -> {PlayerName} ({PlayerID})");
                return true;
            }
            catch (AuthenticationException e)
            {
                Debug.LogError($"Failed SignIn: {e.Message}", gameObject);
                return false;
            }
        }

        private async Task<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                return relayJoinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to get relay join code: {e.Message}", gameObject);
                return default;
            }
        }

        private async Task<JoinAllocation> JoinRelay(string relayJoinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to join relay: {e.Message}", gameObject);
                return default;
            }
        }

        private Player AddLobbyPlayer()
        {
            var me = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {DISPLAY_NAME, new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Member,
                        Multiplayer.Instance.PlayerName) }
                }
            };

            return me;
        }

        private void ConfigureTransportForHost(Allocation allocation, UnityTransport transport)
        {
            bool useWebGL = encryption == EncryptionType.WSS;

            // DTLS (güvenli) endpoint’i seç
            var endpoint = allocation.ServerEndpoints
                .First(e => e.ConnectionType == "dtls" && (!useWebGL || e.Secure == true));

            // Host’ta connectionData ve hostConnectionData aynýdýr
            var data = new RelayServerData(
                host: endpoint.Host,
                port: (ushort)endpoint.Port,
                allocationId: allocation.AllocationIdBytes,
                connectionData: allocation.ConnectionData,
                hostConnectionData: allocation.ConnectionData,
                key: allocation.Key,
                isSecure: true,               // dtls
                isWebSocket: useWebGL         // WebGL’de true
            );

            transport.SetRelayServerData(data);
        }

        private void ConfigureTransportForClient(JoinAllocation join, UnityTransport transport)
        {
            bool useWebGL = encryption == EncryptionType.WSS;

            var endpoint = join.ServerEndpoints
                .First(e => e.ConnectionType == "dtls" && (!useWebGL || e.Secure == true));

            var data = new RelayServerData(
                host: endpoint.Host,
                port: (ushort)endpoint.Port,
                allocationId: join.AllocationIdBytes,
                connectionData: join.ConnectionData,          // client’ýn kendi
                hostConnectionData: join.HostConnectionData,  // host’unki
                key: join.Key,
                isSecure: true,
                isWebSocket: useWebGL
            );

            transport.SetRelayServerData(data);
        }

        private void LobbyHearbeatTimer()
        {
            if (CurrentLobby == null) return;

            // Poll: tüm oyuncularda çalýþýr
            _pollTimer -= Time.deltaTime;
            if (_pollTimer <= 0f)
            {
                _pollTimer = LOBBY_POLL_INTERVAL;
                _ = HandlePollForUpdatesAsync();
            }

            // Heartbeat: sadece host
            if (!IsLobbyHost) return;

            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = LOBBY_HEARTBEAT_INTERVAL;
                _ = HandleHeartbeatAsync();
            }
        }

        private async Task HandleHeartbeatAsync()
        {
            if (!IsLobbyHost) return;
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                Debug.Log("Heartbeat sent to: " + CurrentLobby.Name);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Heartbeat failed: " + e.Message);
            }
        }


        private async Task HandlePollForUpdatesAsync()
        {
            try
            {
                var lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                CurrentLobby = lobby;
                Debug.Log("Polled updates for: " + lobby.Name);
                RaisePlayersChangedIfNeeded(CurrentLobby.Players);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Poll failed: " + e.Message);
            }
        }

        private void ResetTimer()
        {
            _pollTimer = 0f;                                                // poll herkeste
            _heartbeatTimer = IsLobbyHost ? 0f : float.PositiveInfinity;    // host deðilse asla çalýþmasýn
        }

    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
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
        private const float LOBBY_HEARTBEAT_INTERVAL = 20.0f;
        private const float LOBBY_POLL_INTERVAL = 65.0f;
        private const string KEY_JOIN_CODE = "RelayJoinCode";
        private const string DTLS_ENCRYPTION = "dtls"; // Datagram Transport Layer Security
        private const string WSS_ENCRYPTION = "wss"; // Web Socket Secure, use for WebGL builds


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

        private string ConnectionType => encryption == EncryptionType.DTLS ? DTLS_ENCRYPTION : WSS_ENCRYPTION;

        private async void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            await Authenticate();
        }

        private void Update()
        {
            LobbyHearbeatTimer();
        }

        public async Task<bool> CreateLobby(string lobbyName, int maxPlayer)
        {
            try
            {
                Allocation allocation = await AllocateRelay(maxPlayer);
                string joinCode = await GetRelayJoinCode(allocation);

                CreateLobbyOptions options = new CreateLobbyOptions()
                {
                    IsPrivate = false
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
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
                ResetTimer();

                string relayJoinCode = CurrentLobby.Data[KEY_JOIN_CODE].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                ConfigureTransportForClient(joinAllocation, unityTransport);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Joined lobby: {CurrentLobby.Name}\tWith Code: {CurrentLobby.LobbyCode}", gameObject);
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
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                ResetTimer();

                string relayJoinCode = CurrentLobby.Data[KEY_JOIN_CODE].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                ConfigureTransportForClient(joinAllocation, unityTransport);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Joined lobby: {CurrentLobby.Name}\tWith Code: {CurrentLobby.LobbyCode}", gameObject);
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to allocate relay: {e.Message}", gameObject);
                return false;
            }
        }

        /// <summary>
        /// Oyuncu herghangi bir isim girmediginde bu fonksiyon cagrilir "Player", random 0,1000" arasinda bir deger verilir
        /// </summary>
        /// <returns></returns>
        private async Task Authenticate()
        {
            await Authenticate("Player" + UnityEngine.Random.Range(0, 1000));
        }

        /// <summary>
        /// Oyuncu bir isim belirttiginde bu fonksiyon kullanilir
        /// </summary>
        /// <param name="playerName">oyuncunun ismi ile bir Authenticate olusturulur.</param>
        /// <returns></returns>
        private async Task Authenticate(string playerName)
        {
            // Unity Service Henus Baslatilmadiysa:
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions options = new InitializationOptions(); // Unity Service initialize ayarlari olusturulur
                options.SetProfile(playerName); // oyuncunun Profili (bir nevi kimligi) olusturulur

                await UnityServices.InitializeAsync(options); // Unity Service asenkron sekilde baslatilir.
            }

            // Oyuncu Unity Authentication Service'a baglanir.
            AuthenticationService.Instance.SignedIn += () =>
                Debug.Log($"Signed in as Name: {AuthenticationService.Instance.PlayerName}\tID: {AuthenticationService.Instance.PlayerId}", gameObject);

            // Oyuncu giris yapmadiysa / yapamadiysa
            // Oyuncunun anaonim olarak giris yapmasini saglar.
            // Email/sifre istemez; Unity otomatik olarak cihaz veya profil bazli giris yapar.
            // (Anonim giriþte genelde "Player_xxxxx" gibi bir ad olur.)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // Oyuncunun bilgileri rastgele olusturulur.
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                // Yerel degiskenlere oyuncunun bilgileri girilir.
                PlayerID = AuthenticationService.Instance.PlayerId;
                PlayerName = AuthenticationService.Instance.PlayerName;
            }
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

        private void LobbyHearbeatTimer()
        {
            if (CurrentLobby == null) return;

            _heartbeatTimer -= Time.deltaTime;
            _pollTimer -= Time.deltaTime;

            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = LOBBY_HEARTBEAT_INTERVAL;
                _ = HandleHeartbeatAsync();
            }

            if (_pollTimer <= 0f)
            {
                _pollTimer = LOBBY_POLL_INTERVAL;
                _ = HandlePollForUpdatesAsync();
            }
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

        private async Task HandleHeartbeatAsync()
        {
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
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Poll failed: " + e.Message);
            }
        }

        private void ResetTimer()
        {
            _heartbeatTimer = 0.0f;
            _pollTimer = 0.0f;
        }
    }
}
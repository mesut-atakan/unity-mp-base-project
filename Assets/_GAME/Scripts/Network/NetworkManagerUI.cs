using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class NetworkManager : MonoBehaviour
    {
        [SerializeField] private Button btnServer;
        [SerializeField] private Button btnHost;
        [SerializeField] private Button btnClient;

        private void OnEnable()
        {
            btnServer.onClick.AddListener(OnServer);
            btnHost.onClick.AddListener(OnHost);
            btnClient.onClick.AddListener(OnClient);
        }

        private void OnDisable()
        {
            btnServer.onClick.RemoveListener(OnServer);
            btnHost.onClick.RemoveListener(OnHost);
            btnClient.onClick.AddListener(OnClient);            
        }

        private void OnServer()
        {
            Unity.Netcode.NetworkManager.Singleton.StartServer();
        }

        private void OnHost()
        {
            Unity.Netcode.NetworkManager.Singleton.StartHost();
        }

        private void OnClient()
        {
            Unity.Netcode.NetworkManager.Singleton.StartClient();
        }
    }
}
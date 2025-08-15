using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

namespace Aventra.Game
{
    public class PlayerInLobbyCard : MonoBehaviour
    {
        [SerializeField] private Image imgPlayer;
        [SerializeField] private TMP_Text lblPlayerName;
        [SerializeField] private Image imgAdmin;
        [SerializeField] private Image imgReady;
        public Player Player { get; private set; }

        public void SetPlayer(string playerName, bool isAdmin, Player player)
        {
            lblPlayerName.text = playerName;
            imgAdmin.gameObject.SetActive(isAdmin);
            Player = player;
        }
    }
}
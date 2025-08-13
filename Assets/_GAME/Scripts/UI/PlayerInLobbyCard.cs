using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Aventra.Game
{
    public class PlayerInLobbyCard : MonoBehaviour
    {
        [SerializeField] private Image imgPlayer;
        [SerializeField] private TMP_Text lblPlayerName;
        [SerializeField] private Image imgAdmin;
        [SerializeField] private Image imgReady;

        public void SetPlayer(string playerName, bool isAdmin)
        {
            lblPlayerName.text = playerName;
            imgAdmin.gameObject.SetActive(isAdmin);
        }
    }
}
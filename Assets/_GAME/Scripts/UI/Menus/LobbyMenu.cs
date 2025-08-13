using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aventra.Game.Utils;

namespace Aventra.Game
{
    public sealed class LobbyMenu : BaseMenu
    {
        [SerializeField] private TMP_Text lblLobbyName;
        [SerializeField] private TMP_Text lblMaxPlayerAmount;
        [SerializeField] private Button btnExit;
        [SerializeField] private Button btnReady;
        [SerializeField] private PlayerInLobbyCard playerLobbyCardPrefab;
        [SerializeField] private Transform playerGroup;

        private void CreatePlayerCard()
        {
            var obj = Instantiate(playerLobbyCardPrefab, playerGroup);
            obj.SetPlayer(PlayerAccount.GetPlayerName(), false);
        }
    }
}
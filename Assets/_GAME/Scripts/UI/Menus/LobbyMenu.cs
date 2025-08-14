using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aventra.Game.Utils;
using Unity.Services.Authentication;

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

        public override void Open()
        {
            base.Open();
            if (Multiplayer.Instance.IsEnteredLobby)
            {
                SetLobbyName(Multiplayer.Instance.CurrentLobby.Name);
                SetMaxPlayerAmount(Multiplayer.Instance.CurrentLobby.MaxPlayers);
                CreatePlayerCard();
            }
        }

        private void CreatePlayerCard()
        {
            var obj = Instantiate(playerLobbyCardPrefab, playerGroup);
            obj.SetPlayer(AuthenticationService.Instance.PlayerName + "\t" + AuthenticationService.Instance.PlayerId, false);
        }

        private void SetLobbyName(string lobbyName) => lblLobbyName.text = lobbyName;
        private void SetMaxPlayerAmount(int maxPlayerAmount) => lblMaxPlayerAmount.text = maxPlayerAmount.ToString();
    }
}
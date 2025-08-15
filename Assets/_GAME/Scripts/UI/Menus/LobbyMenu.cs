using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aventra.Game.Utils;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

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

        private List<PlayerInLobbyCard> _playerCards = new List<PlayerInLobbyCard>();

        public override void Open()
        {
            base.Open();
            if (Multiplayer.Instance.IsEnteredLobby)
            {
                SetLobbyName(Multiplayer.Instance.CurrentLobby.Name);
                SetMaxPlayerAmount(Multiplayer.Instance.CurrentLobby.MaxPlayers);
                LoadPlayers();
            }
        }

        private void CreatePlayerCard(string playerName, bool isAdmin, string playerID = "")
        {
            var obj = Instantiate(playerLobbyCardPrefab, playerGroup);
            obj.SetPlayer(playerName + "\t" + playerID, isAdmin);
            _playerCards.Add(obj);
        }

        private void LoadPlayers()
        {
            ClearAllCards();
            var players = Multiplayer.Instance.CurrentLobby.Players;
            foreach (var player in players)
            {
                CreatePlayerCard(GetPlayerName(player), false);
            }
        }

        private void SetLobbyName(string lobbyName) => lblLobbyName.text = lobbyName;
        private void SetMaxPlayerAmount(int maxPlayerAmount) => lblMaxPlayerAmount.text = maxPlayerAmount.ToString();

        private string GetPlayerName(Player player)
        {
            return player.Data != null
                    && player.Data.TryGetValue(Multiplayer.DISPLAY_NAME, out var obj)
                    ? obj.Value
                    : player.Id;
        }

        private void ClearAllCards()
        {
            foreach (var playerCard in _playerCards)
            {
                Destroy(playerCard.gameObject);
            }
            _playerCards = new List<PlayerInLobbyCard>();
        }
    }
}
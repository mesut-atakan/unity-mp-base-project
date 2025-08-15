using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aventra.Game.Utils;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Linq;

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
                Multiplayer.Instance.OnLobbyPlayersChanged += OnHandlePlayersChanged;
            }
        }

        public override void Close()
        {
            base.Close();

            if (Multiplayer.Instance != null)
            {
                Multiplayer.Instance.OnLobbyPlayersChanged -= OnHandlePlayersChanged;
            }
        }

        private void CreatePlayerCard(string playerName, bool isAdmin, Player player)
        {
            var obj = Instantiate(playerLobbyCardPrefab, playerGroup);
            obj.SetPlayer(playerName + "\t" + player.Id, isAdmin, player);
            _playerCards.Add(obj);
        }

        private void LoadPlayers()
        {
            var players = Multiplayer.Instance.CurrentLobby.Players;
            foreach (var player in players)
            {
                CreatePlayerCard(GetPlayerName(player), false, player);
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

        private bool RemovePlayerCard(Player player)
        {
            PlayerInLobbyCard card = _playerCards.Find(p=> p.Player.Id == player.Id);
            if (card != null)
            {
                _playerCards.Remove(card);
                Destroy(card.gameObject);
                return true;
            }
            return false;
        }

        private void OnHandlePlayersChanged(IReadOnlyList<Player> list)
        {
            var (added, removed) = GetNewPlayerList(list.ToList());

            foreach(var p in added)
            {
                CreatePlayerCard(GetPlayerName(p), false, p);
            }

            foreach (var p in removed)
            {
                RemovePlayerCard(p);
            }
        }

        private (List<Player> added, List<Player> removed) GetNewPlayerList(List<Player> newPlayers)
        {
            var added = new List<Player>();
            var removed = new List<Player>();

            // Yeni girenler
            foreach (var player in newPlayers)
            {
                if (!_playerCards.Any(p => p.Player.Id == player.Id))
                    added.Add(player);
            }

            // Çýkanlar
            foreach (var currentCard in _playerCards)
            {
                if (!newPlayers.Any(p => p.Id == currentCard.Player.Id))
                    removed.Add(currentCard.Player);
            }

            return (added, removed);
        }

    }
}
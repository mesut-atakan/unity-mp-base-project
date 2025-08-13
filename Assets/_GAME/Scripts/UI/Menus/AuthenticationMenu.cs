using Aventra.Game.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Aventra.Game
{
    public sealed class AuthenticationMenu : BaseMenu
    {
        [SerializeField] private TMP_InputField nickname;
        [SerializeField] private Button applyButton;
        [SerializeField] private CreateOrJoinLobbyMenu createOrJoinLobbyMenu;


        public override void Open()
        {
            base.Open();
            applyButton.onClick.AddListener(Apply);
            nickname.onValueChanged.AddListener(OnValueChanged);
        }

        public override void Close()
        {
            base.Close();
            applyButton.onClick.RemoveListener(Apply);
            nickname.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(string arg0)
        {
            applyButton.interactable = !nickname.text.IsNullOrEmpty();
        }

        private void Apply()
        {
            if (!nickname.text.IsNullOrEmpty())
            {
                PlayerAccount.SetPlayerName(nickname.text);
                Close();
                createOrJoinLobbyMenu.Open();
            }
        }
    }
}
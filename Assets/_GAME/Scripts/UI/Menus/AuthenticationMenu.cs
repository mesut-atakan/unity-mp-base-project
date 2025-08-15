using Aventra.Game.Core;
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

            if (PlayerPrefs.HasKey(Constants.PlayerPrefsKeys.PLAYER_NICK_NAME))
                nickname.text = PlayerPrefs.GetString(Constants.PlayerPrefsKeys.PLAYER_NICK_NAME);

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

        private async void Apply()
        {
            if (!nickname.text.IsNullOrEmpty())
            {
                applyButton.interactable = false;
                PlayerPrefs.SetString(Constants.PlayerPrefsKeys.PLAYER_NICK_NAME, nickname.text);
                bool isSuccess = await Multiplayer.Instance.CreateUser(nickname.text);
                if (!isSuccess)
                {
                    applyButton.interactable = true;
                    return;
                }

                applyButton.interactable = true;
                Close();
                createOrJoinLobbyMenu.Open();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aventra.Game.Utils;
using System;
using WebSocketSharp;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

namespace Aventra.Game
{
    public sealed class CreateOrJoinLobbyMenu : BaseMenu
    {
        [SerializeField] private TMP_InputField lobbyNameInputLabel;
        [SerializeField] private TMP_InputField lobbyPasswordInputLabel;
        [SerializeField] private TMP_InputField lobbyIDInputLabel;
        [SerializeField] private Slider lobbyMaxPlayerSlider;
        [SerializeField] private TMP_Text lblPlayerAmount;
        [SerializeField] private Toggle lobbyPrivateToggle;
        [SerializeField] private Toggle lobbyLockToggle;
        [SerializeField] private Button btnCreateLobby;
        [SerializeField] private Button btnJoinLobby;
        [SerializeField] private TMP_Text lblPlayerName;

        private void Start()
        {
            ApplyPlayerAmountLabel();
            CreateLobbyButtonInteractable();
            JoinLobbyButtonInteractable();
        }

        public override void Open()
        {
            base.Open();
            lobbyNameInputLabel.onValueChanged.AddListener(LobbyNameOnValueChanged);
            lobbyMaxPlayerSlider.onValueChanged.AddListener(SliderOnValueChanged);
            lobbyLockToggle.onValueChanged.AddListener(LobbyLockOnValueChanged);
            lobbyPasswordInputLabel.onValueChanged.AddListener(LobbyPasswordOnValueChanged);
            btnCreateLobby.onClick.AddListener(CreateLobby);

            lobbyIDInputLabel.onValueChanged.AddListener(LobbyIdOnValueChanged);
            btnJoinLobby.onClick.AddListener(JoinLobby);
            ApplyPlayerName();
        }

        public override void Close()
        {
            base.Close();
            lobbyNameInputLabel.onValueChanged.RemoveListener(LobbyNameOnValueChanged);
            lobbyMaxPlayerSlider.onValueChanged.RemoveListener(SliderOnValueChanged);
            lobbyLockToggle.onValueChanged.RemoveListener(LobbyLockOnValueChanged);
            lobbyPasswordInputLabel.onValueChanged.RemoveListener(LobbyPasswordOnValueChanged);
            btnCreateLobby.onClick.RemoveListener(CreateLobby);

            lobbyIDInputLabel.onValueChanged.RemoveListener(LobbyIdOnValueChanged);
            btnJoinLobby.onClick.RemoveListener(JoinLobby);
        }


        private async void CreateLobby()
        {
            await HandleCreateLobby();
        }
        private void JoinLobby()
        {
            throw new NotImplementedException();
        }

        private void LobbyIdOnValueChanged(string arg0)
        {
            JoinLobbyButtonInteractable();
        }

        private void LobbyNameOnValueChanged(string arg0)
        {
            CreateLobbyButtonInteractable();
        }

        private void SliderOnValueChanged(float arg0)
        {
            ApplyPlayerAmountLabel();
        }

        private void LobbyLockOnValueChanged(bool arg0)
        {
            lobbyPasswordInputLabel.interactable = arg0;
            CreateLobbyButtonInteractable();

            if (!arg0)
                lobbyPasswordInputLabel.text = string.Empty;
        }

        private void LobbyPasswordOnValueChanged(string arg0)
        {
            CreateLobbyButtonInteractable();
        }

        private void CreateLobbyButtonInteractable()
        {
            btnCreateLobby.interactable =
                (!lobbyLockToggle.isOn
                && InputFieldIsFull(lobbyNameInputLabel))
                || (lobbyLockToggle.isOn 
                && InputFieldIsFull(lobbyNameInputLabel) 
                && InputFieldIsFull(lobbyPasswordInputLabel));
        }

        private void JoinLobbyButtonInteractable()
        {
            btnJoinLobby.interactable = InputFieldIsFull(lobbyIDInputLabel);
        }

        private void ApplyPlayerAmountLabel()
        {
            lblPlayerAmount.text = lobbyMaxPlayerSlider.value.ToString();
        }

        private void ApplyPlayerName()
        {
            if (PlayerAccount.HasName)
                lblPlayerName.text = PlayerAccount.GetPlayerName().Trim();
        }

        private async Task<bool> HandleCreateLobby()
        {
            try
            {
                LobbyHandler lobbyHandler = new LobbyHandler(
                    lobbyNameInputLabel.text.Trim(),
                    (int)lobbyMaxPlayerSlider.value,
                    !lobbyPrivateToggle.isOn,
                    lobbyLockToggle.isOn,
                    lobbyPasswordInputLabel.text
                    );

                CreateLobbyOptions options = new CreateLobbyOptions()
                {
                    IsLocked = lobbyHandler.Lock,
                    IsPrivate = !lobbyHandler.IsVisible,
                    Password = lobbyHandler.Password
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyHandler.LobbyName, lobbyHandler.MaxPlayer, options);
                Debug.Log($"Lobby Olusturuldu!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lobby Olusturulamadi: {ex.Message}");
                return false;
            }
        }

        private bool InputFieldIsFull(TMP_InputField inputField)
            => !inputField.text.Trim().IsNullOrEmpty();
    }
}
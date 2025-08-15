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
        [SerializeField] private TMP_InputField IFLobbyName;
        [SerializeField] private TMP_InputField IFLobbyPassword;
        [SerializeField] private TMP_InputField IFLobbyJoinCode;
        [SerializeField] private Slider lobbyMaxPlayerSlider;
        [SerializeField] private TMP_Text lblPlayerAmount;
        [SerializeField] private Toggle lobbyPrivateToggle;
        [SerializeField] private Toggle lobbyLockToggle;
        [SerializeField] private Button btnCreateLobby;
        [SerializeField] private Button btnJoinLobby;
        [SerializeField] private Button btnQuickJoin;
        [SerializeField] private TMP_Text lblPlayerName;
        [Header("Menus")]
        [SerializeField] private LobbyMenu lobbyMenu;

        private int MaxPlayerAmount => (int)lobbyMaxPlayerSlider.value;
        private string LobbyName => IFLobbyName.text;
        private string LobbyPassword => IFLobbyPassword.text;
        private string JoinCode => IFLobbyJoinCode.text;
        private bool LobbyPrivate => lobbyPrivateToggle.isOn;
        private bool LobbyLock => lobbyLockToggle.isOn;

        private void Start()
        {
            ApplyPlayerAmountLabel();
            CreateLobbyButtonInteractable();
            JoinLobbyButtonInteractable();
        }

        public override void Open()
        {
            base.Open();
            lobbyMaxPlayerSlider.onValueChanged.AddListener(SliderOnValueChanged);
            lobbyLockToggle.onValueChanged.AddListener(LobbyLockOnValueChanged);
            IFLobbyName.onValueChanged.AddListener(LobbyNameOnValueChanged);
            IFLobbyPassword.onValueChanged.AddListener(LobbyPasswordOnValueChanged);
            IFLobbyJoinCode.onValueChanged.AddListener(LobbyIdOnValueChanged);

            btnJoinLobby.onClick.AddListener(JoinLobbyByCode);
            btnQuickJoin.onClick.AddListener(OnQuickJoin);
            btnCreateLobby.onClick.AddListener(CreateLobby);
            ApplyPlayerName();
        }

        public override void Close()
        {
            base.Close();
            lobbyMaxPlayerSlider.onValueChanged.RemoveListener(SliderOnValueChanged);
            lobbyLockToggle.onValueChanged.RemoveListener(LobbyLockOnValueChanged);
            IFLobbyName.onValueChanged.RemoveListener(LobbyNameOnValueChanged);
            IFLobbyPassword.onValueChanged.RemoveListener(LobbyPasswordOnValueChanged);
            IFLobbyJoinCode.onValueChanged.RemoveListener(LobbyIdOnValueChanged);

            btnJoinLobby.onClick.RemoveListener(JoinLobbyByCode);
            btnQuickJoin.onClick.RemoveListener(OnQuickJoin);
            btnCreateLobby.onClick.RemoveListener(CreateLobby);
        }

        private async void CreateLobby()
        {
            SetAllInputInteractable(false);
            bool isSuccess = await Multiplayer.Instance.CreateLobby(LobbyName, MaxPlayerAmount);
            if (isSuccess)
            {
                lobbyMenu.Open();
                Close();
                return;
            }
            SetAllInputInteractable(true);
        }
        private async void JoinLobbyByCode()
        {
            SetAllInputInteractable(false);
            bool isSuccess = await Multiplayer.Instance.JoinLobby(JoinCode);
            if (isSuccess)
            {
                lobbyMenu.Open();
                Close();
                return;
            }
            SetAllInputInteractable(true);
        }

        private async void OnQuickJoin()
        {
            SetAllInputInteractable(false);
            bool isSuccess = await Multiplayer.Instance.QuickJoinLobby();
            if (isSuccess)
            {
                lobbyMenu.Open();
                Close();
                return;
            }
            SetAllInputInteractable(true);
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
            IFLobbyPassword.interactable = arg0;
            CreateLobbyButtonInteractable();

            if (!arg0)
                IFLobbyPassword.text = string.Empty;
        }

        private void LobbyPasswordOnValueChanged(string arg0)
        {
            CreateLobbyButtonInteractable();
        }

        private void CreateLobbyButtonInteractable()
        {
            btnCreateLobby.interactable =
                (!lobbyLockToggle.isOn
                && InputFieldIsFull(IFLobbyName))
                || (lobbyLockToggle.isOn 
                && InputFieldIsFull(IFLobbyName) 
                && InputFieldIsFull(IFLobbyPassword));
        }

        private void JoinLobbyButtonInteractable()
        {
            btnJoinLobby.interactable = InputFieldIsFull(IFLobbyJoinCode);
        }

        private void ApplyPlayerAmountLabel()
        {
            lblPlayerAmount.text = lobbyMaxPlayerSlider.value.ToString();
        }

        private void ApplyPlayerName()
        {
            lblPlayerName.text = Multiplayer.Instance.PlayerName;
        }

        private void SetAllInputInteractable(bool v)
        {
            IFLobbyName.interactable = v;
            lobbyMaxPlayerSlider.interactable = v;
            lobbyPrivateToggle.interactable = v;
            lobbyLockToggle.interactable = v;
            IFLobbyPassword.interactable = v;
            IFLobbyJoinCode.interactable = v;
            btnJoinLobby.interactable = v;
            btnQuickJoin.interactable = v;
            btnCreateLobby.interactable = v;
        }

        private bool InputFieldIsFull(TMP_InputField inputField)
            => !inputField.text.Trim().IsNullOrEmpty();
    }
}
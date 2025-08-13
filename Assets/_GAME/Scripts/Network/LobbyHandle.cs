namespace Aventra.Game
{
    public struct LobbyHandler
    {
        private readonly string _lobbyName;
        private readonly int _maxPlayer;
        private readonly bool _isVisible;
        private readonly bool _lock;
        private readonly string _password;
        public string LobbyID { get; private set; }

        public string LobbyName => _lobbyName;
        public int MaxPlayer => _maxPlayer;
        public bool IsVisible => _isVisible;
        public bool Lock => _lock;
        public string Password => _password;

        public LobbyHandler(string lobbyName, int maxPlayer) : this()
        {
            _lobbyName = lobbyName;
            _maxPlayer = maxPlayer;
        }

        public LobbyHandler(string lobbyName, int maxPlayer, bool isVisible) : this(lobbyName, maxPlayer)
        {
            _isVisible = isVisible;
        }

        public LobbyHandler(string lobbyName, int maxPlayer, bool isVisible, bool @lock, string password) : this(lobbyName, maxPlayer, isVisible)
        {
            _lock = @lock;
            _password = password;
        }

        public override bool Equals(object obj)
        {
            return (obj is LobbyHandler).GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return LobbyID.GetHashCode();
        }
    }
}
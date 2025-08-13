using WebSocketSharp;

namespace Aventra.Game
{
    public static class PlayerAccount
    {
        private static string PlayerName = "";

        public static bool HasName => !PlayerName.IsNullOrEmpty();

        public static void SetPlayerName(string playerName)
        {
            PlayerName = playerName;
        }

        public static string GetPlayerName()
        {
            return PlayerName;
        }
    }
}
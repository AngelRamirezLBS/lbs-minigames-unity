using Lbs.MiniGames.Navigation;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class AppServices
    {
        public AppServices(GameSession session, GameLauncher gameLauncher)
        {
            Session = session;
            GameLauncher = gameLauncher;
        }

        public GameSession Session { get; }
        public GameLauncher GameLauncher { get; }
    }
}

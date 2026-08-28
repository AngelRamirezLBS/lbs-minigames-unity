using Lbs.MiniGames.Navigation;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class AppServices
    {
        public AppServices(GameSession session, IGameLauncher gameLauncher)
        {
            Session = session;
            GameLauncher = gameLauncher;
        }

        public GameSession Session { get; }
        public IGameLauncher GameLauncher { get; }
    }
}

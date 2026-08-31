using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared.Audio;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class AppServices
    {
        public AppServices(GameSession session, IGameLauncher gameLauncher, IAppAudioService audioService)
        {
            Session = session;
            GameLauncher = gameLauncher;
            Audio = audioService;
        }

        public AppServices(GameSession session, IGameLauncher gameLauncher)
            : this(session, gameLauncher, null)
        {
        }

        public GameSession Session { get; }
        public IGameLauncher GameLauncher { get; }
        public IAppAudioService Audio { get; }
    }
}

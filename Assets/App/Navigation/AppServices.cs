using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared.Audio;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class AppServices
    {
        public AppServices(GameSession session, GameLauncher gameLauncher, IAppAudioService audioService)
        {
            Session = session;
            GameLauncher = gameLauncher;
            Audio = audioService;
        }

        // Backwards compatibility: allow existing Configure flow without audio (legacy fallback)
        public AppServices(GameSession session, GameLauncher gameLauncher)
            : this(session, gameLauncher, null)
        {
        }

        public GameSession Session { get; }
        public GameLauncher GameLauncher { get; }
        public IAppAudioService Audio { get; }
    }
}

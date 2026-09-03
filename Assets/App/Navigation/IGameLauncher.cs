using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Catalog;

namespace Lbs.MiniGames.Navigation
{
    public interface IGameLauncher
    {
        void Launch(GameDefinition game);
        void Launch(GameLaunchRequest request);
        void Complete(MiniGameResult result);
        void ShowLobby();
    }
}

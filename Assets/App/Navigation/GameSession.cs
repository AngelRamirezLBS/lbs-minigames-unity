using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Navigation
{
    public sealed class GameSession
    {
        public GameDefinition SelectedGame { get; private set; }
        public MiniGameResult? LastResult { get; private set; }

        public void SelectGame(GameDefinition game)
        {
            SelectedGame = game;
        }

        public void RecordResult(MiniGameResult result)
        {
            LastResult = result;
        }
    }
}

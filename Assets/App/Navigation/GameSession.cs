using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Navigation
{
    public sealed class GameSession
    {
        public GameDefinition SelectedGame { get; private set; }
        public GameLaunchRequest? CurrentRequest { get; private set; }
        public DifficultyDefinition SelectedDifficulty => CurrentRequest?.Difficulty;
        public string SelectedDifficultyId => SelectedDifficulty != null ? SelectedDifficulty.DifficultyId : null;
        public MiniGameResult? LastResult { get; private set; }

        public void SelectGame(GameDefinition game)
        {
            SelectedGame = game;
            if (game != null)
            {
                DifficultyDefinition difficulty = game.GetDefaultDifficulty();
                CurrentRequest = new GameLaunchRequest(game, difficulty);
            }
            else
            {
                CurrentRequest = null;
            }
        }

        public void SelectRequest(GameLaunchRequest request)
        {
            SelectedGame = request.Game;
            CurrentRequest = request;
        }

        public void RecordResult(MiniGameResult result)
        {
            LastResult = result;
        }
    }
}

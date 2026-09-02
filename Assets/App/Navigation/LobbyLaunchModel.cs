using Lbs.MiniGames.Catalog;

namespace Lbs.MiniGames.Navigation
{
    /// <summary>
    /// Small reusable selection boundary for Lobby. Currently auto-selects default difficulty
    /// without changing visual flow; future UI can inject a selector and pass explicit difficulty.
    /// </summary>
    public static class LobbyLaunchModel
    {
        public static GameLaunchRequest CreateRequest(GameDefinition game)
        {
            if (game == null) throw new System.ArgumentNullException(nameof(game));
            DifficultyDefinition difficulty = game.GetDefaultDifficulty();
            return new GameLaunchRequest(game, difficulty);
        }

        public static GameLaunchRequest CreateRequest(GameDefinition game, DifficultyDefinition difficulty)
        {
            if (game == null) throw new System.ArgumentNullException(nameof(game));
            if (difficulty != null && game.SupportedDifficulties != null && game.SupportedDifficulties.Count > 0 && !game.SupportsDifficulty(difficulty))
            {
                throw new System.ArgumentException($"Difficulty {difficulty.DifficultyId} not supported by {game.GameId}", nameof(difficulty));
            }
            // Null difficulty allowed only for legacy fallback.
            return new GameLaunchRequest(game, difficulty);
        }
    }
}

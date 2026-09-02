using System;
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
            if (!CurrentRequest.HasValue)
            {
                throw new ArgumentException("Cannot record a mini-game result without an active launch request.", nameof(result));
            }

            GameLaunchRequest request = CurrentRequest.Value;
            if (!string.Equals(result.GameId, request.Game.GameId, StringComparison.Ordinal))
            {
                throw new ArgumentException("The result game ID does not match the active launch request.", nameof(result));
            }

            if (!string.Equals(result.DifficultyId, request.DifficultyId, StringComparison.Ordinal))
            {
                throw new ArgumentException("The result difficulty ID does not match the active launch request.", nameof(result));
            }

            LastResult = result;
        }
    }
}

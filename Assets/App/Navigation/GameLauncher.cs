using System;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Navigation
{
    public sealed class GameLauncher
    {
        private readonly GameSession session;
        private readonly ISceneLoader sceneLoader;
        private readonly string lobbySceneName;

        public GameLauncher(GameSession session, ISceneLoader sceneLoader, string lobbySceneName)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            this.lobbySceneName = string.IsNullOrWhiteSpace(lobbySceneName)
                ? throw new ArgumentException("A lobby scene name is required.", nameof(lobbySceneName))
                : lobbySceneName;
        }

        public void Launch(GameDefinition game)
        {
            if (game == null || !game.IsValid())
            {
                throw new ArgumentException("The selected game definition is invalid.", nameof(game));
            }

            DifficultyDefinition difficulty = game.GetDefaultDifficulty();
            Launch(new GameLaunchRequest(game, difficulty));
        }

        public void Launch(GameLaunchRequest request)
        {
            if (request.Game == null || !request.Game.IsValid())
            {
                throw new ArgumentException("The selected game definition is invalid.", nameof(request));
            }

            if (request.Difficulty != null && !request.Difficulty.IsValid())
            {
                throw new ArgumentException("The selected difficulty is invalid.", nameof(request));
            }

            // If difficulty is specified but not supported, fallback to default or throw? Allow but validate support.
            if (request.Difficulty != null && request.Game.SupportedDifficulties != null && request.Game.SupportedDifficulties.Count > 0)
            {
                if (!request.Game.SupportsDifficulty(request.Difficulty))
                {
                    throw new ArgumentException($"Difficulty '{request.DifficultyId}' is not supported by game '{request.Game.GameId}'.", nameof(request));
                }
            }

            session.SelectRequest(request);
            sceneLoader.Load(request.Game.SceneName);
        }

        public void Complete(MiniGameResult result)
        {
            session.RecordResult(result);
        }

        public void ShowLobby()
        {
            sceneLoader.Load(lobbySceneName);
        }
    }
}

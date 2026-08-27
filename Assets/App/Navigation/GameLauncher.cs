using System;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Navigation
{
    public sealed class GameLauncher : IGameLauncher
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

            session.SelectGame(game);
            sceneLoader.Load(game.SceneName);
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

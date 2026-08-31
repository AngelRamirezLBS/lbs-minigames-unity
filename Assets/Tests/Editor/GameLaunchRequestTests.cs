using NUnit.Framework;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;

namespace Lbs.MiniGames.Tests
{
    public sealed class GameLaunchRequestTests
    {
        private GameCategory cat;
        private GameDefinition game;
        private DifficultyDefinition easy;
        private DifficultyDefinition medium;

        [SetUp]
        public void Setup()
        {
            cat = ScriptableObject.CreateInstance<GameCategory>();
            cat.Configure("cat", "Cat", "desc");
            easy = ScriptableObject.CreateInstance<DifficultyDefinition>();
            easy.Configure("easy", "Easy", 0, "desc");
            medium = ScriptableObject.CreateInstance<DifficultyDefinition>();
            medium.Configure("medium", "Medium", 1, "desc");
            game = ScriptableObject.CreateInstance<GameDefinition>();
            game.Configure("game.id", "Game", cat, "Scene", "desc");
            game.ConfigureDifficulties(new System.Collections.Generic.List<DifficultyDefinition> { easy, medium }, medium);
        }

        [TearDown]
        public void TearDown()
        {
            if (game != null) Object.DestroyImmediate(game);
            if (medium != null) Object.DestroyImmediate(medium);
            if (easy != null) Object.DestroyImmediate(easy);
            if (cat != null) Object.DestroyImmediate(cat);
        }

        [Test]
        public void Request_IsValid_And_DifficultyId()
        {
            var req = new GameLaunchRequest(game, medium);
            Assert.IsTrue(req.IsValid);
            Assert.AreEqual("medium", req.DifficultyId);
            var legacy = new GameLaunchRequest(game, null);
            Assert.IsTrue(legacy.IsValid); // legacy fallback allowed
            Assert.IsNull(legacy.DifficultyId);
        }

        [Test]
        public void GameSession_Preserves_SelectedGame_Compatibility()
        {
            var session = new GameSession();
            var req = new GameLaunchRequest(game, easy);
            session.SelectRequest(req);
            Assert.AreEqual(game, session.SelectedGame);
            Assert.AreEqual(easy, session.SelectedDifficulty);
            Assert.AreEqual("easy", session.SelectedDifficultyId);
            Assert.AreEqual(req, session.CurrentRequest.Value);

            // Legacy SelectGame fallback still sets request with default
            var session2 = new GameSession();
            session2.SelectGame(game);
            Assert.AreEqual(game, session2.SelectedGame);
            Assert.AreEqual(medium, session2.SelectedDifficulty);
        }

        [Test]
        public void GameLauncher_SelectsDefault_WhenGivenGameDefinition()
        {
            var session = new GameSession();
            var loader = new FakeLoader();
            var launcher = new GameLauncher(session, loader, "Lobby");
            launcher.Launch(game);
            Assert.AreEqual("Scene", loader.LastScene);
            Assert.AreEqual(game, session.SelectedGame);
            Assert.AreEqual(medium.DifficultyId, session.SelectedDifficultyId);
        }

        [Test]
        public void GameLauncher_LaunchRequest_ValidatesSupport()
        {
            var session = new GameSession();
            var loader = new FakeLoader();
            var launcher = new GameLauncher(session, loader, "Lobby");
            var other = ScriptableObject.CreateInstance<DifficultyDefinition>();
            try
            {
                other.Configure("hard", "Hard", 2, "desc");
                var badReq = new GameLaunchRequest(game, other);
                Assert.Throws<System.ArgumentException>(() => launcher.Launch(badReq));
                Assert.IsFalse(badReq.IsValid);
            }
            finally { Object.DestroyImmediate(other); }
        }

        [Test]
        public void MiniGameResult_Preserves_Compatibility_And_Difficulty()
        {
            var r1 = new MiniGameResult("game.id", MiniGameCompletionState.Completed, 10, 1, 1);
            Assert.IsNull(r1.DifficultyId);
            var r2 = new MiniGameResult("game.id", MiniGameCompletionState.Completed, 10, 1, 1, "easy");
            Assert.AreEqual("easy", r2.DifficultyId);
            Assert.AreEqual(10, r2.Score);
        }

        private sealed class FakeLoader : ISceneLoader
        {
            public string LastScene;
            public void Load(string sceneName) { LastScene = sceneName; }
        }
    }
}

using System.Linq;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.NumberPull;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Games.NumberPull.Tests
{
    public sealed class NumberPullSceneIntegrationTests
    {
        private const string ScenePath = "Assets/App/Games/NumberPull/NumberPull.unity";

        [Test]
        public void SceneBuildsDifficultySelectorBeforeMatchWithoutAnEventSystemInputRoute()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                NumberPullGame game = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<NumberPullGame>(true))
                    .Single();
                GameSession session = new();
                AppServices services = new(session, new GameLauncher(session, new NoOpSceneLoader(), "Lobby"));

                game.Configure(services);
                Canvas.ForceUpdateCanvases();

                Transform[] hierarchy = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .ToArray();
                Assert.That(hierarchy.Any(item => item.name == "LeftCard"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "RightCard"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "RopeMarker"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "ResultOverlay"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "DifficultySelector"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "DifficultyLowerPrimary"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "DifficultyUpperPrimary"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "DifficultyPreparatory"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "VioletNebula"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "AmberNebula"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "ArenaHorizon"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "StageAura"), Is.True);
                Assert.That(hierarchy.Count(item => item.name == "CharacterVisual"), Is.EqualTo(2));
                Assert.That(game.HasSelectedDifficulty, Is.False);
                Assert.That(game.IsCompleted, Is.False);
                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)), Is.Empty);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void CatalogAssetsExposeStableMathematicsRegistration()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/NumberPullGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");

            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.GameId, Is.EqualTo(NumberPullGame.StableGameId));
            Assert.That(definition.SceneName, Is.EqualTo("NumberPull"));
            Assert.That(definition.Category.CategoryId, Is.EqualTo("mathematics"));
            Assert.That(catalog.Categories, Does.Contain(definition.Category));
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
        }

        private sealed class NoOpSceneLoader : ISceneLoader
        {
            public void Load(string sceneName)
            {
            }
        }
    }
}

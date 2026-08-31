using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Tests
{
    public sealed class ClothesSelectionIntegrationTests
    {
        [Test]
        public void ShapeSuccessRoute_TargetsClothesSelection()
        {
            Assert.AreEqual("clothes.selection", LevelSequenceRoute.ShapeAnalogySuccessTarget);
        }

        [Test]
        public void AutomaticSequenceArrival_ConfiguresIncomingSceneOnceWithoutRestartingInstructionLifecycle()
        {
            Scene incoming = SceneManager.GetActiveScene();
            GameObject root = new("IncomingClothesSelection");
            SceneManager.MoveGameObjectToScene(root, incoming);
            ConfigurationProbe probe = root.AddComponent<ConfigurationProbe>();
            AppSceneConfigurationGate gate = new();

            try
            {
                Assert.IsTrue(gate.Configure(incoming, null));
                Assert.IsFalse(gate.Configure(incoming, null));
                Assert.AreEqual(1, probe.ConfigureCount);
                Assert.AreEqual(1, probe.InstructionStarts);
            }
            finally
            {
                Object.DestroyImmediate(root);
                gate.Forget(incoming);
            }
        }

        [Test]
        public void SlideMotion_MovesWholeOutgoingBoardLeftAndIncomingBoardRightOverOneSecond()
        {
            const float width = 1920f;
            Assert.AreEqual(1f, LevelSequenceController.SlideDurationSeconds);
            Assert.AreEqual(Vector2.zero, LevelSlideMotion.OutgoingPosition(width, 0f));
            Assert.AreEqual(new Vector2(-width, 0f), LevelSlideMotion.OutgoingPosition(width, 1f));
            Assert.AreEqual(new Vector2(width, 0f), LevelSlideMotion.IncomingPosition(width, 0f));
            Assert.AreEqual(Vector2.zero, LevelSlideMotion.IncomingPosition(width, 1f));
        }

        [Test]
        public void CatalogAndBuildSettings_WireClothesSelectionWithShapeCategoryAndDifficulties()
        {
            GameDefinition shape = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/ShapeAnalogyGame.asset");
            GameDefinition clothes = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/ClothesSelectionGame.asset");
            Assert.NotNull(shape);
            Assert.NotNull(clothes);
            Assert.AreEqual("Clothes Selection", clothes.VisibleName);
            Assert.AreEqual(shape.Category, clothes.Category);
            Assert.AreEqual(shape.SupportedDifficulties.Count, clothes.SupportedDifficulties.Count);
            Assert.AreEqual(shape.DefaultDifficulty, clothes.DefaultDifficulty);
            Assert.AreEqual("ClothesSelection", clothes.SceneName);
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Object>("Assets/App/Games/ClothesSelection/ClothesSelection.unity"));
            Assert.IsTrue(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/App/Games/ClothesSelection/ClothesSelection.unity"));
        }

        private sealed class ConfigurationProbe : MonoBehaviour, IAppScene
        {
            public int ConfigureCount { get; private set; }
            public int InstructionStarts { get; private set; }

            public void Configure(AppServices services)
            {
                ConfigureCount++;
                InstructionStarts++;
            }
        }
    }
}

using NUnit.Framework;
using Lbs.MiniGames.Catalog;
using UnityEngine;

namespace Lbs.MiniGames.Tests
{
    public sealed class DifficultyDefinitionTests
    {
        [TearDown]
        public void TearDown()
        {
            // Unity objects created via CreateInstance must be destroyed to avoid leakage between tests
            // (Editor tests run in same domain; not strictly required but good hygiene)
        }
        [Test]
        public void IsValid_RequiresIdAndDisplayName()
        {
            var d = ScriptableObject.CreateInstance<DifficultyDefinition>();
            try
            {
                d.Configure("", "Easy", 0, "desc");
                Assert.IsFalse(d.IsValid());
                d.Configure("easy", "", 0, "desc");
                Assert.IsFalse(d.IsValid());
                d.Configure("easy", "Easy", 0, "desc");
                Assert.IsTrue(d.IsValid());
            }
            finally { Object.DestroyImmediate(d); }
        }

        [Test]
        public void GameDefinition_Fallback_EmptyDifficultyListIsValidAndLaunchable()
        {
            var cat = ScriptableObject.CreateInstance<GameCategory>();
            var game = ScriptableObject.CreateInstance<GameDefinition>();
            try
            {
                cat.Configure("cat", "Cat", "desc");
                game.Configure("game.id", "Game", cat, "Scene", "desc");
                Assert.IsTrue(game.IsValid());
                Assert.IsTrue(game.IsValidWithDifficulties());
                Assert.IsNull(game.DefaultDifficulty);
                Assert.IsNull(game.GetDefaultDifficulty());
                Assert.IsEmpty(game.SupportedDifficulties);
            }
            finally { Object.DestroyImmediate(game); Object.DestroyImmediate(cat); }
        }

        [Test]
        public void GameDefinition_WithDifficulties_ValidationAndDefault()
        {
            var cat = ScriptableObject.CreateInstance<GameCategory>();
            var easy = ScriptableObject.CreateInstance<DifficultyDefinition>();
            var medium = ScriptableObject.CreateInstance<DifficultyDefinition>();
            var game = ScriptableObject.CreateInstance<GameDefinition>();
            try
            {
                cat.Configure("cat", "Cat", "desc");
                easy.Configure("easy", "Easy", 0, "desc");
                medium.Configure("medium", "Medium", 1, "desc");
                game.Configure("game.id", "Game", cat, "Scene", "desc");
                game.ConfigureDifficulties(new System.Collections.Generic.List<DifficultyDefinition> { easy, medium }, medium);
                Assert.IsTrue(game.IsValidWithDifficulties());
                Assert.AreEqual(medium, game.DefaultDifficulty);
                Assert.AreEqual(medium, game.GetDefaultDifficulty());
                Assert.IsTrue(game.SupportsDifficulty(medium));
                Assert.IsFalse(game.SupportsDifficulty(null));
                game.ConfigureDifficulties(new System.Collections.Generic.List<DifficultyDefinition> { easy, medium }, null);
                Assert.IsFalse(game.IsValidWithDifficulties());
                Assert.AreEqual(easy, game.GetDefaultDifficulty());
            }
            finally { Object.DestroyImmediate(game); Object.DestroyImmediate(medium); Object.DestroyImmediate(easy); Object.DestroyImmediate(cat); }
        }
    }
}

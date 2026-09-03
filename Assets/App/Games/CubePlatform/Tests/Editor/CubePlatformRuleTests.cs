using Lbs.MiniGames.Games.CubePlatform;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class CubePlatformRuleTests
    {
        [Test] public void Box3_IsTheCorrectAnswer() => Assert.IsTrue(CubePlatformRule.IsCorrect("box3"));
        [Test] public void OtherAnswers_AreIncorrect()
        {
            Assert.IsFalse(CubePlatformRule.IsCorrect("box1"));
            Assert.IsFalse(CubePlatformRule.IsCorrect("box2"));
        }
        [Test] public void CorrectAnswer_Constant_IsBox3() => Assert.AreEqual("box3", CubePlatformRule.CorrectAnswer);
    }
}

using Lbs.MiniGames.Games.TrianglesCount;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class TrianglesCountRuleTests
    {
        [Test] public void Three_IsTheCorrectAnswer() => Assert.IsTrue(TrianglesCountRule.IsCorrect("3"));
        [Test] public void OtherAnswers_AreIncorrect()
        {
            Assert.IsFalse(TrianglesCountRule.IsCorrect("4"));
            Assert.IsFalse(TrianglesCountRule.IsCorrect("2"));
            Assert.IsFalse(TrianglesCountRule.IsCorrect("1"));
        }
        [Test] public void CorrectAnswer_Constant_IsThree() => Assert.AreEqual("3", TrianglesCountRule.CorrectAnswer);
    }
}

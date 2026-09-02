using Lbs.MiniGames.Games.SquaresSuccession;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class SquaresSuccessionRuleTests
    {
        [Test] public void Option3_IsTheCorrectAnswer() => Assert.IsTrue(SquaresSuccessionRule.IsCorrect("option3"));
        [Test] public void Option1_IsIncorrect() => Assert.IsFalse(SquaresSuccessionRule.IsCorrect("option1"));
        [Test] public void Option2_IsIncorrect() => Assert.IsFalse(SquaresSuccessionRule.IsCorrect("option2"));
        [Test] public void CorrectAnswer_Constant_IsOption3() => Assert.AreEqual("option3", SquaresSuccessionRule.CorrectAnswer);
    }
}

using Lbs.MiniGames.Games.CandiesLogic;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class CandiesLogicRuleTests
    {
        [Test] public void Sweets_IsTheCorrectAnswer() => Assert.IsTrue(CandiesLogicRule.IsCorrect("sweets"));
        [Test] public void Candies_IsIncorrect() => Assert.IsFalse(CandiesLogicRule.IsCorrect("candies"));
        [Test] public void CorrectAnswer_Constant_IsSweets() => Assert.AreEqual("sweets", CandiesLogicRule.CorrectAnswer);
    }
}

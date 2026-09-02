using Lbs.MiniGames.Games.KitchenMathLogic;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class KitchenMathLogicRuleTests
    {
        [Test] public void Option4_IsTheCorrectAnswer() => Assert.IsTrue(KitchenMathLogicRule.IsCorrect("option4"));
        [Test] public void OtherAnswers_AreIncorrect()
        {
            Assert.IsFalse(KitchenMathLogicRule.IsCorrect("option1"));
            Assert.IsFalse(KitchenMathLogicRule.IsCorrect("option2"));
            Assert.IsFalse(KitchenMathLogicRule.IsCorrect("option3"));
        }
        [Test] public void CorrectAnswer_Constant_IsOption4() => Assert.AreEqual("option4", KitchenMathLogicRule.CorrectAnswer);
    }
}

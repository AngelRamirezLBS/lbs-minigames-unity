using Lbs.MiniGames.Games.BubbleMath;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class BubbleMathRuleTests
    {
        [Test] public void CorrectAnswer_Constant_IsOption2() => Assert.That(BubbleMathRule.CorrectAnswer, Is.EqualTo("option2"));
        [Test] public void Option1_IsIncorrect() => Assert.That(BubbleMathRule.IsCorrect("option1"), Is.False);
        [Test] public void Option2_IsCorrect() => Assert.That(BubbleMathRule.IsCorrect("option2"), Is.True);
        [Test] public void Option3_IsIncorrect() => Assert.That(BubbleMathRule.IsCorrect("option3"), Is.False);
        [Test] public void Option4_IsIncorrect() => Assert.That(BubbleMathRule.IsCorrect("option4"), Is.False);
    }
}

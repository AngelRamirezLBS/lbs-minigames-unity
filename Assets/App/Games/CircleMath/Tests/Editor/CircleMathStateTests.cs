using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.CircleMath;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class CircleMathStateTests
    {
        [Test] public void Option3_IsTheSemanticCorrectAnswer() => Assert.IsTrue(CircleMathRule.IsCorrect("option3"));
        [Test] public void Option1_IsIncorrect() => Assert.IsFalse(CircleMathRule.IsCorrect("option1"));
        [Test] public void Option2_IsIncorrect() => Assert.IsFalse(CircleMathRule.IsCorrect("option2"));

        [Test]
        public void Option1_LocksSelectionUntilIncorrectResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("option1", CircleMathRule.CorrectAnswer));
            Assert.IsFalse(state.Select("option3", CircleMathRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }

        [Test]
        public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("option3", CircleMathRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

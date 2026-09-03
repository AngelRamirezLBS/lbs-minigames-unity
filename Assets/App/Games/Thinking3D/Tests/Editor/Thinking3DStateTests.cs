using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.Thinking3D;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class Thinking3DStateTests
    {
        [Test] public void Option1_IsTheSemanticCorrectAnswer() => Assert.IsTrue(Thinking3DRule.IsCorrect("option1"));
        [Test] public void Option2_IsIncorrect() => Assert.IsFalse(Thinking3DRule.IsCorrect("option2"));
        [Test] public void Option3_IsIncorrect() => Assert.IsFalse(Thinking3DRule.IsCorrect("option3"));

        [Test]
        public void Option2_LocksSelectionUntilIncorrectResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("option2", Thinking3DRule.CorrectAnswer));
            Assert.IsFalse(state.Select("option1", Thinking3DRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }

        [Test]
        public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("option1", Thinking3DRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

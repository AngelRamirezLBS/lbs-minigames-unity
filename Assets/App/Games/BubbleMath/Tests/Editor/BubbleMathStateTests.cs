using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.BubbleMath;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class BubbleMathStateTests
    {
        [Test]
        public void IncorrectAnswer_UnlocksRetry()
        {
            var state = new SelectionGameState();

            Assert.That(state.Select("option1", BubbleMathRule.CorrectAnswer), Is.False);
            state.FinishIncorrect();

            Assert.That(state.Phase, Is.EqualTo(SelectionPhase.Ready));
        }

        [Test]
        public void CorrectAnswer_RequiresFinalInputToBeEnabled()
        {
            var state = new SelectionGameState();

            Assert.That(state.Select("option2", BubbleMathRule.CorrectAnswer), Is.True);
            state.FinishCelebration();
            Assert.That(state.AcceptFinalInput(), Is.False);
            state.EnableFinalInput();

            Assert.That(state.AcceptFinalInput(), Is.True);
        }
    }
}

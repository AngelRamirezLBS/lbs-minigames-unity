using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.SquaresSuccession;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class SquaresSuccessionStateTests
    {
        [Test] public void Option3_IsTheSemanticCorrectAnswer() => Assert.IsTrue(SquaresSuccessionRule.IsCorrect("option3"));
        [Test] public void FirstTouchLocksUntilResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("option1", SquaresSuccessionRule.CorrectAnswer));
            Assert.IsFalse(state.Select("option3", SquaresSuccessionRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }
        [Test] public void IncorrectAnswerUnlocksRetryAndUsesLowerTier()
        {
            var state = new SelectionGameState();
            state.Select("option1", SquaresSuccessionRule.CorrectAnswer);
            state.FinishIncorrect();
            Assert.AreEqual(SelectionPhase.Ready, state.Phase);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
        [Test] public void CorrectAnswerLocksAndUsesPerfectTier()
        {
            var state = new SelectionGameState();
            Assert.IsTrue(state.Select("option3", SquaresSuccessionRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.Celebrating, state.Phase);
            Assert.AreEqual(8, state.Score);
            Assert.AreEqual(2, state.StarCount);
        }
        [Test] public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("option3", SquaresSuccessionRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.TrianglesCount;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class TrianglesCountStateTests
    {
        [Test] public void Three_IsTheSemanticCorrectAnswer() => Assert.IsTrue(TrianglesCountRule.IsCorrect("3"));
        [Test] public void FirstTouchLocksUntilResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("4", TrianglesCountRule.CorrectAnswer));
            Assert.IsFalse(state.Select("3", TrianglesCountRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }
        [Test] public void IncorrectAnswerUnlocksRetryAndUsesLowerTier()
        {
            var state = new SelectionGameState();
            state.Select("2", TrianglesCountRule.CorrectAnswer);
            state.FinishIncorrect();
            Assert.AreEqual(SelectionPhase.Ready, state.Phase);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
        [Test] public void CorrectAnswerLocksAndUsesPerfectTier()
        {
            var state = new SelectionGameState();
            Assert.IsTrue(state.Select("3", TrianglesCountRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.Celebrating, state.Phase);
            Assert.AreEqual(8, state.Score);
            Assert.AreEqual(2, state.StarCount);
        }
        [Test] public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("3", TrianglesCountRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

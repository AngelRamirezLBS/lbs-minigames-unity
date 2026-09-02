using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.CandiesLogic;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class CandiesLogicStateTests
    {
        [Test] public void Sweets_IsTheSemanticCorrectAnswer() => Assert.IsTrue(CandiesLogicRule.IsCorrect("sweets"));
        [Test] public void FirstTouchLocksUntilResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("candies", CandiesLogicRule.CorrectAnswer));
            Assert.IsFalse(state.Select("sweets", CandiesLogicRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }
        [Test] public void IncorrectAnswerUnlocksRetryAndUsesLowerTier()
        {
            var state = new SelectionGameState();
            state.Select("candies", CandiesLogicRule.CorrectAnswer);
            state.FinishIncorrect();
            Assert.AreEqual(SelectionPhase.Ready, state.Phase);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
        [Test] public void CorrectAnswerLocksAndUsesPerfectTier()
        {
            var state = new SelectionGameState();
            Assert.IsTrue(state.Select("sweets", CandiesLogicRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.Celebrating, state.Phase);
            Assert.AreEqual(8, state.Score);
            Assert.AreEqual(2, state.StarCount);
        }
        [Test] public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("sweets", CandiesLogicRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

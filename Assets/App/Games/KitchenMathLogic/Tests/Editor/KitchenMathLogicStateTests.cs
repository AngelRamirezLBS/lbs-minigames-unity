using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.KitchenMathLogic;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class KitchenMathLogicStateTests
    {
        [Test] public void Option4_IsTheSemanticCorrectAnswer() => Assert.IsTrue(KitchenMathLogicRule.IsCorrect("option4"));
        [Test] public void FirstTouchLocksUntilResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("option1", KitchenMathLogicRule.CorrectAnswer));
            Assert.IsFalse(state.Select("option4", KitchenMathLogicRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }
        [Test] public void IncorrectAnswerUnlocksRetryAndUsesLowerTier()
        {
            var state = new SelectionGameState();
            state.Select("option1", KitchenMathLogicRule.CorrectAnswer);
            state.FinishIncorrect();
            Assert.AreEqual(SelectionPhase.Ready, state.Phase);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
        [Test] public void CorrectAnswerLocksAndUsesPerfectTier()
        {
            var state = new SelectionGameState();
            Assert.IsTrue(state.Select("option4", KitchenMathLogicRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.Celebrating, state.Phase);
            Assert.AreEqual(8, state.Score);
            Assert.AreEqual(2, state.StarCount);
        }
        [Test] public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("option4", KitchenMathLogicRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

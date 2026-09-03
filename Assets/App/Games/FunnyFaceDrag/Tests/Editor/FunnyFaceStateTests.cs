using Lbs.MiniGames.Games.FunnyFaceDrag;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class FunnyFaceStateTests
    {
        [Test] public void SingleCorrect_KeepsReadyUntilSecond()
        {
            var state = new FunnyFaceState();
            Assert.AreEqual(FunnyFacePhase.Ready, state.Phase);
            var o = state.Drop(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot, true);
            Assert.AreEqual(FunnyFaceDropOutcome.Correct, o);
            Assert.AreEqual(FunnyFacePhase.Ready, state.Phase);
            Assert.AreEqual(1, state.AcceptedCount);
        }

        [Test] public void TwoCorrects_TransitionsToCelebrating()
        {
            var state = new FunnyFaceState();
            state.Drop(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot, true);
            var o2 = state.Drop(FunnyFaceRule.GreenPiece, FunnyFaceRule.BottomSlot, true);
            Assert.AreEqual(FunnyFaceDropOutcome.Correct, o2);
            Assert.AreEqual(FunnyFacePhase.Celebrating, state.Phase);
            Assert.AreEqual(2, state.AcceptedCount);
            Assert.IsFalse(state.HadError);
            Assert.AreEqual(8, state.Score);
            Assert.AreEqual(2, state.StarCount);
        }

        [Test] public void Incorrect_SetsHadErrorAndResolving()
        {
            var state = new FunnyFaceState();
            var o = state.Drop(FunnyFaceRule.YellowPiece, FunnyFaceRule.TopSlot, true);
            Assert.AreEqual(FunnyFaceDropOutcome.Incorrect, o);
            Assert.AreEqual(FunnyFacePhase.ResolvingIncorrect, state.Phase);
            Assert.IsTrue(state.HadError);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
            state.FinishIncorrect();
            Assert.AreEqual(FunnyFacePhase.Ready, state.Phase);
        }

        [Test] public void Distractor_AnySlot_IsIncorrect()
        {
            var state = new FunnyFaceState();
            Assert.AreEqual(FunnyFaceDropOutcome.Incorrect, state.Drop(FunnyFaceRule.YellowPiece, FunnyFaceRule.BottomSlot, true));
        }

        [Test] public void Outside_ReturnsOutsideAndStaysReady()
        {
            var state = new FunnyFaceState();
            Assert.AreEqual(FunnyFaceDropOutcome.Outside, state.Drop(FunnyFaceRule.PurplePiece, null, false));
            Assert.AreEqual(FunnyFacePhase.Ready, state.Phase);
            Assert.AreEqual(0, state.AcceptedCount);
        }

        [Test] public void Celebration_Flow_FinalInput()
        {
            var state = new FunnyFaceState();
            state.Drop(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot, true);
            state.Drop(FunnyFaceRule.GreenPiece, FunnyFaceRule.BottomSlot, true);
            state.FinishCelebration();
            Assert.AreEqual(FunnyFacePhase.Final, state.Phase);
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

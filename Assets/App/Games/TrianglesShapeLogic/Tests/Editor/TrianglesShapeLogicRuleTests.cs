using Lbs.MiniGames.Games.TrianglesShapeLogic;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class TrianglesShapeLogicRuleTests
    {
        [Test] public void BluePieceMatchesBlueSplashSlot() => Assert.IsTrue(TrianglesShapeLogicRule.Matches(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.BlueSplashSlot));
        [Test] public void RedPieceMatchesRedSplashSlot() => Assert.IsTrue(TrianglesShapeLogicRule.Matches(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.RedSplashSlot));
        [Test] public void CrossedPiecesDoNotMatch()
        {
            Assert.IsFalse(TrianglesShapeLogicRule.Matches(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.RedSplashSlot));
            Assert.IsFalse(TrianglesShapeLogicRule.Matches(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.BlueSplashSlot));
        }

        [Test]
        public void EvaluateMapsOutsideIncorrectCorrect()
        {
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Outside, TrianglesShapeLogicRule.Evaluate(TrianglesShapeLogicRule.BluePiece, null, false));
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Incorrect, TrianglesShapeLogicRule.Evaluate(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.RedSplashSlot, true));
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Correct, TrianglesShapeLogicRule.Evaluate(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.RedSplashSlot, true));
        }

        [Test]
        public void TwoCorrectDropsCelebrate()
        {
            var state = new TrianglesShapeLogicState();
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Correct, state.Drop(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.BlueSplashSlot, true));
            Assert.AreEqual(TrianglesShapeLogicPhase.Ready, state.Phase);
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Correct, state.Drop(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.RedSplashSlot, true));
            Assert.AreEqual(TrianglesShapeLogicPhase.Celebrating, state.Phase);
            Assert.AreEqual(2, state.AcceptedCount);
            Assert.AreEqual(8, state.Score);
        }

        [Test]
        public void IncorrectDropLocksUntilResolution()
        {
            var state = new TrianglesShapeLogicState();
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Incorrect, state.Drop(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.RedSplashSlot, true));
            Assert.AreEqual(TrianglesShapeLogicPhase.ResolvingIncorrect, state.Phase);
            Assert.AreEqual(TrianglesShapeLogicDropOutcome.Outside, state.Drop(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.RedSplashSlot, true));
            state.FinishIncorrect();
            Assert.AreEqual(TrianglesShapeLogicPhase.Ready, state.Phase);
            Assert.IsTrue(state.HadError);
            Assert.AreEqual(4, state.Score);
        }

        [Test]
        public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new TrianglesShapeLogicState();
            state.Drop(TrianglesShapeLogicRule.BluePiece, TrianglesShapeLogicRule.BlueSplashSlot, true);
            state.Drop(TrianglesShapeLogicRule.RedPiece, TrianglesShapeLogicRule.RedSplashSlot, true);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}

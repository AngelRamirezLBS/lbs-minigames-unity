using Lbs.MiniGames.Games.AnimalDrag;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class AnimalDragStateTests
    {
        [Test]
        public void MatchingPieces_AreAcceptedInAnyOrder_AndCompleteOnSecondDrop()
        {
            AnimalDragState state = new();

            Assert.AreEqual(AnimalDragDropOutcome.Correct, state.Drop(AnimalDragRule.PigPiece, AnimalDragRule.GreenHouseSlot, true));
            Assert.AreEqual(1, state.AcceptedCount);
            Assert.AreEqual(AnimalDragPhase.Ready, state.Phase);
            Assert.AreEqual(AnimalDragDropOutcome.Correct, state.Drop(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, true));
            Assert.AreEqual(2, state.AcceptedCount);
            Assert.AreEqual(AnimalDragPhase.Celebrating, state.Phase);
        }

        [Test]
        public void IncorrectDrop_PreservesAcceptedPieces_AndUsesLowerTier()
        {
            AnimalDragState state = new();
            state.Drop(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, true);

            Assert.AreEqual(AnimalDragDropOutcome.Incorrect, state.Drop(AnimalDragRule.PigPiece, AnimalDragRule.YellowHouseSlot, true));
            Assert.AreEqual(1, state.AcceptedCount);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
            state.FinishIncorrect();

            Assert.AreEqual(AnimalDragPhase.Ready, state.Phase);
            Assert.AreEqual(AnimalDragDropOutcome.Correct, state.Drop(AnimalDragRule.PigPiece, AnimalDragRule.GreenHouseSlot, true));
        }

        [Test]
        public void Outside_DoesNotAffectState()
        {
            AnimalDragState state = new();
            Assert.AreEqual(AnimalDragDropOutcome.Outside, state.Drop(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, false));
            Assert.AreEqual(0, state.AcceptedCount);
            Assert.IsFalse(state.HadError);
        }

        [Test]
        public void DuplicatePiece_IsIgnored()
        {
            AnimalDragState state = new();
            state.Drop(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, true);
            Assert.AreEqual(AnimalDragDropOutcome.Outside, state.Drop(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, true));
            Assert.AreEqual(1, state.AcceptedCount);
        }
    }
}

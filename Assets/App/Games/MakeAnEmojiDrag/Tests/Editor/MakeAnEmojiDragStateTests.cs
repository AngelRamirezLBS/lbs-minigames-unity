using Lbs.MiniGames.Games.MakeAnEmojiDrag;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class MakeAnEmojiDragStateTests
    {
        [Test]
        public void MatchingPieces_AreAcceptedInAnyOrder_AndCompleteOnTheThirdDrop()
        {
            MakeAnEmojiDragState state = new();

            Assert.AreEqual(MakeAnEmojiDragDropOutcome.Correct, state.Drop(MakeAnEmojiDragRule.BottomPiece, MakeAnEmojiDragRule.BottomSlot, true));
            Assert.AreEqual(1, state.AcceptedCount);
            Assert.AreEqual(MakeAnEmojiDragPhase.Ready, state.Phase);
            Assert.AreEqual(MakeAnEmojiDragDropOutcome.Correct, state.Drop(MakeAnEmojiDragRule.TopPiece, MakeAnEmojiDragRule.TopSlot, true));
            Assert.AreEqual(MakeAnEmojiDragDropOutcome.Correct, state.Drop(MakeAnEmojiDragRule.MiddlePiece, MakeAnEmojiDragRule.MiddleSlot, true));
            Assert.AreEqual(3, state.AcceptedCount);
            Assert.AreEqual(MakeAnEmojiDragPhase.Celebrating, state.Phase);
        }

        [Test]
        public void IncorrectDrop_PreservesAcceptedPieces_AndUsesTheExistingLowerTier()
        {
            MakeAnEmojiDragState state = new();
            state.Drop(MakeAnEmojiDragRule.TopPiece, MakeAnEmojiDragRule.TopSlot, true);

            Assert.AreEqual(MakeAnEmojiDragDropOutcome.Incorrect, state.Drop(MakeAnEmojiDragRule.MiddlePiece, MakeAnEmojiDragRule.BottomSlot, true));
            Assert.AreEqual(1, state.AcceptedCount);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
            state.FinishIncorrect();

            Assert.AreEqual(MakeAnEmojiDragPhase.Ready, state.Phase);
            Assert.AreEqual(MakeAnEmojiDragDropOutcome.Correct, state.Drop(MakeAnEmojiDragRule.MiddlePiece, MakeAnEmojiDragRule.MiddleSlot, true));
        }
    }
}

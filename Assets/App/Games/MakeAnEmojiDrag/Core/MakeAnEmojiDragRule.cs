namespace Lbs.MiniGames.Games.MakeAnEmojiDrag
{
    public enum MakeAnEmojiDragDropOutcome { Outside, Incorrect, Correct }

    /// <summary>
    /// Pure correspondence rule for the three independently placeable emoji strips.
    /// </summary>
    public static class MakeAnEmojiDragRule
    {
        public const string TopPiece = "top-piece";
        public const string MiddlePiece = "middle-piece";
        public const string BottomPiece = "bottom-piece";
        public const string TopSlot = "top-slot";
        public const string MiddleSlot = "middle-slot";
        public const string BottomSlot = "bottom-slot";

        public static MakeAnEmojiDragDropOutcome Evaluate(string pieceId, string slotId, bool overlapsSlot)
        {
            if (!overlapsSlot) return MakeAnEmojiDragDropOutcome.Outside;
            return Matches(pieceId, slotId) ? MakeAnEmojiDragDropOutcome.Correct : MakeAnEmojiDragDropOutcome.Incorrect;
        }

        public static bool Matches(string pieceId, string slotId)
        {
            return (pieceId == TopPiece && slotId == TopSlot)
                || (pieceId == MiddlePiece && slotId == MiddleSlot)
                || (pieceId == BottomPiece && slotId == BottomSlot);
        }
    }
}

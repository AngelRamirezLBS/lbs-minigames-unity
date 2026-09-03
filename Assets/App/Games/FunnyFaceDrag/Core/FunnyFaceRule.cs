namespace Lbs.MiniGames.Games.FunnyFaceDrag
{
    public enum FunnyFaceDropOutcome { Outside, Incorrect, Correct }

    /// <summary>
    /// Pure correspondence rule for FunnyFace drag: two slots, two correct pieces, one distractor.
    /// TopSlot expects Drag3 (purple top), BottomSlot expects Drag2 (green bottom), Drag1 yellow is always incorrect.
    /// </summary>
    public static class FunnyFaceRule
    {
        public const string TopSlot = "TopSlot";
        public const string BottomSlot = "BottomSlot";
        public const string PurplePiece = "Drag3";
        public const string GreenPiece = "Drag2";
        public const string YellowPiece = "Drag1";

        public static FunnyFaceDropOutcome Evaluate(string pieceId, string slotId, bool overlapsSlot)
        {
            if (!overlapsSlot) return FunnyFaceDropOutcome.Outside;
            return Matches(pieceId, slotId) ? FunnyFaceDropOutcome.Correct : FunnyFaceDropOutcome.Incorrect;
        }

        public static bool Matches(string pieceId, string slotId)
        {
            return (pieceId == PurplePiece && slotId == TopSlot)
                || (pieceId == GreenPiece && slotId == BottomSlot);
        }

        public static bool IsCorrect(string pieceId, string slotId) => Matches(pieceId, slotId);
    }
}

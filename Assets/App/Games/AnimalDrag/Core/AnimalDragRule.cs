namespace Lbs.MiniGames.Games.AnimalDrag
{
    public enum AnimalDragDropOutcome { Outside, Incorrect, Correct }

    /// <summary>
    /// Pure correspondence rule for the two-animal homes drag level.
    /// Cat lives in the house that is not green (yellow house), pig lives in the green house.
    /// </summary>
    public static class AnimalDragRule
    {
        public const string CatPiece = "cat-piece";
        public const string PigPiece = "pig-piece";
        public const string YellowHouseSlot = "yellow-house-slot";
        public const string GreenHouseSlot = "green-house-slot";

        public static AnimalDragDropOutcome Evaluate(string pieceId, string slotId, bool overlapsSlot)
        {
            if (!overlapsSlot) return AnimalDragDropOutcome.Outside;
            return Matches(pieceId, slotId) ? AnimalDragDropOutcome.Correct : AnimalDragDropOutcome.Incorrect;
        }

        public static bool Matches(string pieceId, string slotId)
        {
            return (pieceId == CatPiece && slotId == YellowHouseSlot)
                || (pieceId == PigPiece && slotId == GreenHouseSlot);
        }
    }
}

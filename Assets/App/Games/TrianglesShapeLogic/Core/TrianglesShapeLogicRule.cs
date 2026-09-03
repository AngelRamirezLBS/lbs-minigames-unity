namespace Lbs.MiniGames.Games.TrianglesShapeLogic
{
    public enum TrianglesShapeLogicDropOutcome { Outside, Incorrect, Correct }

    /// <summary>
    /// Pure correspondence rule for the triangles shape-logic drag level.
    /// Blue triangle lives under the blue splash, red triangle under the red splash.
    /// The green triangle is pre-placed as the worked example.
    /// </summary>
    public static class TrianglesShapeLogicRule
    {
        public const string BluePiece = "blue-piece";
        public const string RedPiece = "red-piece";
        public const string BlueSplashSlot = "blue-splash-slot";
        public const string RedSplashSlot = "red-splash-slot";

        public static TrianglesShapeLogicDropOutcome Evaluate(string pieceId, string slotId, bool overlapsSlot)
        {
            if (!overlapsSlot) return TrianglesShapeLogicDropOutcome.Outside;
            return Matches(pieceId, slotId) ? TrianglesShapeLogicDropOutcome.Correct : TrianglesShapeLogicDropOutcome.Incorrect;
        }

        public static bool Matches(string pieceId, string slotId)
        {
            return (pieceId == BluePiece && slotId == BlueSplashSlot)
                || (pieceId == RedPiece && slotId == RedSplashSlot);
        }
    }
}

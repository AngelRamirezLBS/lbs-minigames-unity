namespace Lbs.MiniGames.Navigation
{
    public static class LevelSequenceRoute
    {
        public const string ShapeAnalogyGameId = "shape.analogy";
        public const string ClothesSelectionGameId = "clothes.selection";
        public const string ObjectSelectionGameId = "object.selection";

        public const string ShapeAnalogySuccessTarget = "clothes.selection";
        public const string ClothesSelectionSuccessTarget = "object.selection";

        /// <summary>
        /// Explicit membership boundary for games that share the logic-sequence BGM.
        /// Add future logic-sequence game IDs here.
        /// </summary>
        public static bool IsLogicSequenceGame(string gameId)
        {
            return gameId == ShapeAnalogyGameId
                || gameId == ClothesSelectionGameId
                || gameId == ObjectSelectionGameId;
        }
    }
}

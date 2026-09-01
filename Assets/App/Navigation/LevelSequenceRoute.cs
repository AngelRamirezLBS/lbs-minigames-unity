namespace Lbs.MiniGames.Navigation
{
    public static class LevelSequenceRoute
    {
        public const string ShapeAnalogyGameId = "shape.analogy";
        public const string ClothesSelectionGameId = "clothes.selection";
        public const string ObjectSelectionGameId = "object.selection";
        public const string MakeAnEmojiDragGameId = "make.emoji.drag";
        public const string AnimalDragGameId = "animal.drag";

        public const string ShapeAnalogySuccessTarget = "clothes.selection";
        public const string ClothesSelectionSuccessTarget = "object.selection";
        public const string ObjectSelectionSuccessTarget = "make.emoji.drag";
        public const string MakeAnEmojiDragSuccessTarget = "animal.drag";

        /// <summary>
        /// Explicit membership boundary for games that share the logic-sequence BGM.
        /// Add future logic-sequence game IDs here.
        /// </summary>
        public static bool IsLogicSequenceGame(string gameId)
        {
            return gameId == ShapeAnalogyGameId
                || gameId == ClothesSelectionGameId
                || gameId == ObjectSelectionGameId
                || gameId == MakeAnEmojiDragGameId
                || gameId == AnimalDragGameId;
        }
    }
}

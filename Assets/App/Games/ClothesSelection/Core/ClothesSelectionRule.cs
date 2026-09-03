namespace Lbs.MiniGames.Games.ClothesSelection
{
    public static class ClothesSelectionRule
    {
        public const string CorrectAnswer = "gloves";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

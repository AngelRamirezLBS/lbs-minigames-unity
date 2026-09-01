namespace Lbs.MiniGames.Games.ObjectSelection
{
    public static class ObjectSelectionRule
    {
        public const string CorrectAnswer = "tenis";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

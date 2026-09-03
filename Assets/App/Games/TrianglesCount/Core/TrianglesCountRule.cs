namespace Lbs.MiniGames.Games.TrianglesCount
{
    public static class TrianglesCountRule
    {
        public const string CorrectAnswer = "3";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

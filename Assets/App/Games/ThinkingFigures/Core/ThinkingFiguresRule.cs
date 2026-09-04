namespace Lbs.MiniGames.Games.ThinkingFigures
{
    public static class ThinkingFiguresRule
    {
        public const string CorrectAnswer = "option1";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

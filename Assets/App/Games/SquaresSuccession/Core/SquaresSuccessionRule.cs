namespace Lbs.MiniGames.Games.SquaresSuccession
{
    public static class SquaresSuccessionRule
    {
        public const string CorrectAnswer = "option3";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

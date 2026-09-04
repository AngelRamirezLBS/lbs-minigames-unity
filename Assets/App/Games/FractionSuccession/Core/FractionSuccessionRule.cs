namespace Lbs.MiniGames.Games.FractionSuccession
{
    public static class FractionSuccessionRule
    {
        public const string CorrectAnswer = "option1";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

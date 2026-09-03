namespace Lbs.MiniGames.Games.BubbleMath
{
    public static class BubbleMathRule
    {
        public const string CorrectAnswer = "option2";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

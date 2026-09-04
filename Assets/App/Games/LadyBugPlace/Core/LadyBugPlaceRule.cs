namespace Lbs.MiniGames.Games.LadyBugPlace
{
    public static class LadyBugPlaceRule
    {
        public const string CorrectAnswer = "option2";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

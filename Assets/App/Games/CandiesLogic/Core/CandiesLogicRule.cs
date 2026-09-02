namespace Lbs.MiniGames.Games.CandiesLogic
{
    public static class CandiesLogicRule
    {
        public const string CorrectAnswer = "sweets";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

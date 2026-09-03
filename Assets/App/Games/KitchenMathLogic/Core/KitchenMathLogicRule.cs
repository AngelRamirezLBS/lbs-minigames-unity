namespace Lbs.MiniGames.Games.KitchenMathLogic
{
    public static class KitchenMathLogicRule
    {
        public const string CorrectAnswer = "option4";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

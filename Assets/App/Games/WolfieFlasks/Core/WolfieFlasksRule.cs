namespace Lbs.MiniGames.Games.WolfieFlasks
{
    public static class WolfieFlasksRule
    {
        public const string CorrectAnswer = "option2";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

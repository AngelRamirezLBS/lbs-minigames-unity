namespace Lbs.MiniGames.Games.CubePlatform
{
    public static class CubePlatformRule
    {
        public const string CorrectAnswer = "box3";
        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

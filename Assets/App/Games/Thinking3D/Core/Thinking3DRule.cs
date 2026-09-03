namespace Lbs.MiniGames.Games.Thinking3D
{
    public static class Thinking3DRule
    {
        public const string CorrectAnswer = "option1";

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

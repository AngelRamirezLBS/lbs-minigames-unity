namespace Lbs.MiniGames.Games.ShapeAnalogy
{
    public enum ShapeAnalogyDropOutcome { Outside, Incorrect, Correct }

    public static class ShapeAnalogyRule
    {
        public const string CorrectAnswer = "outlined-heart";

        public static ShapeAnalogyDropOutcome Evaluate(string answerId, bool overlapsTarget)
        {
            if (!overlapsTarget) return ShapeAnalogyDropOutcome.Outside;
            return answerId == CorrectAnswer ? ShapeAnalogyDropOutcome.Correct : ShapeAnalogyDropOutcome.Incorrect;
        }
    }
}

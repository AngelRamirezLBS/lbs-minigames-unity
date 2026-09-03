namespace Lbs.MiniGames.Games.ChemistrySelection
{
    public static class ChemistrySelectionRule
    {
        public const string CorrectAnswer = "option1";

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}

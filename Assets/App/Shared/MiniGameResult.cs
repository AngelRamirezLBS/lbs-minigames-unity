namespace Lbs.MiniGames.Shared
{
    public enum MiniGameCompletionState
    {
        Completed,
        Abandoned
    }

    public readonly struct MiniGameResult
    {
        public MiniGameResult(
            string gameId,
            MiniGameCompletionState completionState,
            int score,
            int correctActions,
            int totalActions)
            : this(gameId, completionState, score, correctActions, totalActions, null)
        {
        }

        public MiniGameResult(
            string gameId,
            MiniGameCompletionState completionState,
            int score,
            int correctActions,
            int totalActions,
            string difficultyId)
        {
            GameId = gameId;
            CompletionState = completionState;
            Score = score;
            CorrectActions = correctActions;
            TotalActions = totalActions;
            DifficultyId = difficultyId;
        }

        public string GameId { get; }
        public MiniGameCompletionState CompletionState { get; }
        public int Score { get; }
        public int CorrectActions { get; }
        public int TotalActions { get; }
        public string DifficultyId { get; }
    }
}

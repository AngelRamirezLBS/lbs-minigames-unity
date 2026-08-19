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
        {
            GameId = gameId;
            CompletionState = completionState;
            Score = score;
            CorrectActions = correctActions;
            TotalActions = totalActions;
        }

        public string GameId { get; }
        public MiniGameCompletionState CompletionState { get; }
        public int Score { get; }
        public int CorrectActions { get; }
        public int TotalActions { get; }
    }
}

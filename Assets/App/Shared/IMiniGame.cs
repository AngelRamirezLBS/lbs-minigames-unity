using System;

namespace Lbs.MiniGames.Shared
{
    public interface IMiniGame
    {
        string GameId { get; }
        bool IsCompleted { get; }
        event Action<MiniGameResult> Completed;
    }
}

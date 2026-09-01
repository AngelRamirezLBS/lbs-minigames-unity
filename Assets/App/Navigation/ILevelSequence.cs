namespace Lbs.MiniGames.Navigation
{
    public interface ILevelSequence
    {
        bool IsTransitioning { get; }
        void Advance(string nextGameId);
    }
}

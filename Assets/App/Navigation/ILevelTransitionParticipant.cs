using UnityEngine;

namespace Lbs.MiniGames.Navigation
{
    /// <summary>
    /// Exposes a game's complete visual root and its deferred startup boundary to level navigation.
    /// </summary>
    public interface ILevelTransitionParticipant
    {
        RectTransform TransitionRoot { get; }
        void CompleteTransitionHandoff();
    }
}

using UnityEngine;

namespace Lbs.MiniGames.Navigation
{
    public static class LevelSlideMotion
    {
        public static Vector2 OutgoingPosition(float width, float progress) => new(-width * Mathf.Clamp01(progress), 0f);
        public static Vector2 IncomingPosition(float width, float progress) => new(width * (1f - Mathf.Clamp01(progress)), 0f);
    }
}

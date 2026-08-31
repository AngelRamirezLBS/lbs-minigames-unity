using UnityEngine;

namespace Lbs.MiniGames.Shared.UI
{
    /// <summary>
    /// Centralized layout constants for level chrome (Exit/Hong) at 1920x1080 reference.
    /// No duplicate literals elsewhere — single source of truth per Architecture spec.
    /// </summary>
    public static class LevelChromeLayout
    {
        public static readonly Vector2 ReferenceResolution = new(1920f, 1080f);
        public const float ReferenceMatch = 0.5f;

        // Approved coordinates (top-origin: (0,0) = top-left at 1920x1080)
        public static readonly Vector2 ExitCenter = new(145f, 150f);
        public static readonly Vector2 ExitSize = new(170f, 170f);
        public static readonly Vector2 HongCenter = new(145f, 930f);
        public static readonly Vector2 HongSize = new(220f, 220f);

        public const float ArtworkShadowOffset = 2f;
        public const float ArtworkShadowAlpha = 0.14f;
        public const float CardArtworkShadowOffset = 3f;
        public const float CardArtworkShadowAlpha = 0.18f;

        public static Vector2 ToAnchoredPosition(Vector2 topOriginCenter)
        {
            return new Vector2(topOriginCenter.x - ReferenceResolution.x * 0.5f, ReferenceResolution.y * 0.5f - topOriginCenter.y);
        }
    }
}

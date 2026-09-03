using UnityEngine;

namespace Lbs.MiniGames.Shared.Results
{
    [CreateAssetMenu(fileName = "DefaultFinalCelebrationConfiguration", menuName = "LBS Mini Games/Results/Final Celebration Configuration")]
    public sealed class FinalCelebrationConfiguration : ScriptableObject
    {
        [Header("Backdrop")]
        [SerializeField] private Color backdropColor = new(0.15f, 0.08f, 0.28f, 0f);
        [SerializeField, Range(0f, 1f)] private float backdropFinalAlpha = 0.18f;
        [SerializeField, Min(0f)] private float backdropFadeDuration = 0.18f;

        [Header("Layout")]
        [SerializeField] private Vector2 groupCenter = new(965f, 550f);
        [SerializeField] private Vector2 haloBlurSize = new(600f, 220f);
        [SerializeField] private Vector2 haloSize = new(520f, 180f);
        [SerializeField] private Vector2 scoreOffset = new(-125f, 3f);
        [SerializeField] private Vector2 scoreSize = new(200f, 200f);
        [SerializeField] private Vector2 firstStarOffset = new(78f, -22f);
        [SerializeField] private Vector2 firstStarSize = new(175f, 175f);
        [SerializeField] private Vector2 secondStarOffset = new(128f, 28f);
        [SerializeField] private Vector2 secondStarSize = new(195f, 195f);

        [Header("Style")]
        [SerializeField] private Color haloBlurColor = new(0.22f, 0.70f, 0.45f, 0.06f);
        [SerializeField] private Color haloColor = new(0.22f, 0.70f, 0.45f, 0.12f);
        [SerializeField] private Color haloPrimaryShadowColor = new(0.18f, 0.62f, 0.40f, 0.14f);
        [SerializeField] private Color haloSecondaryShadowColor = new(0.18f, 0.62f, 0.40f, 0.08f);
        [SerializeField] private Vector2 haloSecondaryShadowOffset = new(2f, -2f);
        [SerializeField, Min(1)] private int scoreFontSize = 165;
        [SerializeField] private Color scoreShadowColor = new(0f, 0f, 0f, 0.22f);
        [SerializeField] private Vector2 scoreShadowOffset = new(3f, -3f);
        [SerializeField, Min(0f)] private float starCornerRadius = 28f;
        [SerializeField] private Color starArtworkShadowColor = new(0f, 0f, 0f, 0.18f);
        [SerializeField] private Vector2 starArtworkShadowOffset = new(3f, -3f);
        [SerializeField] private Color starSurfaceShadowColor = new(0f, 0f, 0f, 0.18f);
        [SerializeField] private Vector2 starSurfaceShadowOffset = new(4f, -4f);

        [Header("Animation")]
        [SerializeField, Min(0f)] private float presentationDelay = 1f;
        [SerializeField, Min(0f)] private float entranceDuration = 0.35f;
        [SerializeField, Range(0f, 1f)] private float entranceStartScale = 0.85f;

        public Color BackdropColor => backdropColor;
        public float BackdropFinalAlpha => backdropFinalAlpha;
        public float BackdropFadeDuration => backdropFadeDuration;
        public Vector2 GroupCenter => groupCenter;
        public Vector2 HaloBlurSize => haloBlurSize;
        public Vector2 HaloSize => haloSize;
        public Vector2 ScoreOffset => scoreOffset;
        public Vector2 ScoreSize => scoreSize;
        public Vector2 FirstStarOffset => firstStarOffset;
        public Vector2 FirstStarSize => firstStarSize;
        public Vector2 SecondStarOffset => secondStarOffset;
        public Vector2 SecondStarSize => secondStarSize;
        public Color HaloBlurColor => haloBlurColor;
        public Color HaloColor => haloColor;
        public Color HaloPrimaryShadowColor => haloPrimaryShadowColor;
        public Color HaloSecondaryShadowColor => haloSecondaryShadowColor;
        public Vector2 HaloSecondaryShadowOffset => haloSecondaryShadowOffset;
        public int ScoreFontSize => scoreFontSize;
        public Color ScoreShadowColor => scoreShadowColor;
        public Vector2 ScoreShadowOffset => scoreShadowOffset;
        public float StarCornerRadius => starCornerRadius;
        public Color StarArtworkShadowColor => starArtworkShadowColor;
        public Vector2 StarArtworkShadowOffset => starArtworkShadowOffset;
        public Color StarSurfaceShadowColor => starSurfaceShadowColor;
        public Vector2 StarSurfaceShadowOffset => starSurfaceShadowOffset;
        public float PresentationDelay => presentationDelay;
        public float EntranceDuration => entranceDuration;
        public float EntranceStartScale => entranceStartScale;
    }
}

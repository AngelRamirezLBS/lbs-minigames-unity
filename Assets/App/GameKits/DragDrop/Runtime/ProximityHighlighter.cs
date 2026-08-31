using Lbs.MiniGames.Shared;
using UnityEngine;

namespace Lbs.MiniGames.GameKits.DragDrop
{
    /// <summary>
    /// Preserves ShapeAnalogy's orange outline interpolation for proximity feedback.
    /// Maps distance to alpha, outline thickness and scale with same constants.
    /// </summary>
    [RequireComponent(typeof(RoundedSurface))]
    public sealed class ProximityHighlighter : MonoBehaviour
    {
        private static readonly Color Orange = new(1f, .38f, .08f);
        private const float MaxDist = 350f;
        private const float ShowThreshold = 0.02f;

        private RoundedSurface surface;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        public float CurrentT { get; private set; }

        private void Awake()
        {
            surface = GetComponent<RoundedSurface>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            rectTransform = (RectTransform)transform;
            HideImmediate();
        }

        public void ShowForDistance(float distance)
        {
            float t = 1f - Mathf.Clamp01(distance / MaxDist);
            CurrentT = t;
            bool show = t > ShowThreshold;
            gameObject.SetActive(show);
            if (!show) return;

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0.35f, 1f, t);
            if (surface != null)
            {
                Color c = surface.color;
                c.r = Orange.r; c.g = Orange.g; c.b = Orange.b;
                c.a = Mathf.Lerp(0.45f, 1f, t);
                surface.color = c;
                surface.OutlineThickness = Mathf.Lerp(8f, 3f, t);
            }
            if (rectTransform != null) rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.02f, t);
        }

        public void ShowForPositions(Vector2 draggablePos, Vector2 targetPos)
        {
            float dist = Vector2.Distance(draggablePos, targetPos);
            ShowForDistance(dist);
        }

        public void HideImmediate()
        {
            CurrentT = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (surface != null)
            {
                Color pc = surface.color; pc.a = 0f; surface.color = pc;
                surface.OutlineThickness = 8f;
            }
            if (rectTransform != null) rectTransform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        // Pure mapping without side effects, for EditMode tests.
        public static ProximityFrame Compute(float distance)
        {
            float t = 1f - Mathf.Clamp01(distance / MaxDist);
            bool show = t > ShowThreshold;
            if (!show) return new ProximityFrame { visible = false, t = t, alpha = 0f, colorAlpha = 0f, outlineThickness = 8f, scale = 1f };
            return new ProximityFrame
            {
                visible = true,
                t = t,
                alpha = Mathf.Lerp(0.35f, 1f, t),
                colorAlpha = Mathf.Lerp(0.45f, 1f, t),
                outlineThickness = Mathf.Lerp(8f, 3f, t),
                scale = Mathf.Lerp(1f, 1.02f, t)
            };
        }

        public struct ProximityFrame
        {
            public bool visible;
            public float t;
            public float alpha;
            public float colorAlpha;
            public float outlineThickness;
            public float scale;
        }
    }
}

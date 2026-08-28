using UnityEngine;

namespace Lbs.MiniGames.Games.WildWhiz
{
    /// <summary>
    /// Safe-area helper that anchors its RectTransform to Screen.safeArea via UiFactory fractions.
    /// Keeps 1920x1080 landscape layout unclipped on tablets and 98-inch TVs.
    /// </summary>
    public sealed class WildWhizSafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = new(0, 0, 0, 0);
        private Vector2 lastScreenSize = Vector2.zero;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            // Device metrics can be reported as zero during sceneLoaded and become
            // valid on the next frame (notably during Android rotation/startup).
            Refresh();
        }

        public void Refresh()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    return;
                }
            }

            Vector2 screenSize = new(Screen.width, Screen.height);
            Rect safeArea = GetValidSafeArea(Screen.safeArea, screenSize);

            if (safeArea == lastSafeArea && screenSize == lastScreenSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreenSize = screenSize;

            Vector2 min = safeArea.position;
            Vector2 max = min + safeArea.size;
            if (screenSize.x > 0 && screenSize.y > 0)
            {
                min.x /= screenSize.x;
                min.y /= screenSize.y;
                max.x /= screenSize.x;
                max.y /= screenSize.y;
            }
            else
            {
                min = Vector2.zero;
                max = Vector2.one;
            }

            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void ApplySafeAnchors(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            Vector2 screenSize = new(Screen.width, Screen.height);
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                target.anchorMin = Vector2.zero;
                target.anchorMax = Vector2.one;
                target.offsetMin = Vector2.zero;
                target.offsetMax = Vector2.zero;
                return;
            }

            Rect safeArea = GetValidSafeArea(Screen.safeArea, screenSize);
            Vector2 min = new(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            Vector2 max = new(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);

            target.anchorMin = min;
            target.anchorMax = max;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        public static Rect GetValidSafeArea(Rect safeArea, Vector2 screenSize)
        {
            if (!IsFinite(screenSize.x) || !IsFinite(screenSize.y)
                || screenSize.x < 100f || screenSize.y < 100f
                || safeArea.width <= 0f || safeArea.height <= 0f
                || !IsFinite(safeArea.xMin) || !IsFinite(safeArea.yMin)
                || !IsFinite(safeArea.xMax) || !IsFinite(safeArea.yMax)
                || safeArea.width < screenSize.x * 0.05f || safeArea.height < screenSize.y * 0.05f
                || safeArea.xMin < 0f || safeArea.yMin < 0f
                || safeArea.xMax > screenSize.x || safeArea.yMax > screenSize.y)
            {
                return new Rect(0f, 0f, Mathf.Max(0f, screenSize.x), Mathf.Max(0f, screenSize.y));
            }

            return safeArea;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

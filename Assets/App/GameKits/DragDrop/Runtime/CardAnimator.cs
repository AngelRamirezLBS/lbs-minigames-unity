using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.GameKits.DragDrop
{
    /// <summary>
    /// Composition for card/board animations preserving ShapeAnalogy's current behavior.
    /// PunchPlace and board shake are idempotent and use unscaled time.
    /// </summary>
    public static class CardAnimator
    {
        public static IEnumerator PunchPlace(RectTransform cardRect)
        {
            if (cardRect == null) yield break;
            Vector2 basePos = cardRect.anchoredPosition;
            Vector3 baseScale = Vector3.one;
            const float duration = 0.22f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                float scaleT = 1f - 4f * (p - 0.5f) * (p - 0.5f);
                float scale = Mathf.Lerp(1f, 1.08f, Mathf.Clamp01(scaleT));
                float yOffset;
                if (p < 0.4f)
                {
                    float lp = p / 0.4f;
                    yOffset = Mathf.LerpUnclamped(10f, -4f, lp);
                }
                else
                {
                    float lp = (p - 0.4f) / 0.6f;
                    float c1 = 1.70158f; float c3 = c1 + 1f;
                    float eased = 1f + c3 * Mathf.Pow(lp - 1f, 3f) + c1 * Mathf.Pow(lp - 1f, 2f);
                    yOffset = Mathf.LerpUnclamped(-4f, 0f, eased);
                }
                cardRect.localScale = baseScale * scale;
                cardRect.anchoredPosition = basePos + new Vector2(0f, yOffset);
                yield return null;
            }
            cardRect.localScale = baseScale;
            cardRect.anchoredPosition = basePos;
        }

        public static IEnumerator ShakeBoard(RectTransform board, float duration = 0.48f, float amplitude = 18f)
        {
            if (board == null) yield break;
            Vector2 origin = board.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float progress = Mathf.Min(t / duration, 1f);
                float offset = Mathf.Sin(progress * Mathf.PI * 4f) * amplitude * (1f - progress);
                board.anchoredPosition = origin + new Vector2(offset, 0f);
                yield return null;
            }
            board.anchoredPosition = origin;
        }

        public static IEnumerator FadeScaleIn(RectTransform tr, float duration = 0.35f)
        {
            if (tr == null) yield break;
            CanvasGroup cg = tr.GetComponent<CanvasGroup>();
            Graphic graphic = tr.GetComponent<Graphic>();
            Text txt = tr.GetComponent<Text>();
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.85f;
            Vector3 endScale = Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                tr.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
                float alpha = Mathf.Lerp(0f, 1f, eased);
                if (cg) cg.alpha = alpha;
                else if (txt) txt.canvasRenderer.SetAlpha(alpha);
                else if (graphic) graphic.canvasRenderer.SetAlpha(alpha);
                yield return null;
            }
            tr.localScale = endScale;
            if (cg) cg.alpha = 1f;
            else if (txt) txt.canvasRenderer.SetAlpha(1f);
            else if (graphic) graphic.canvasRenderer.SetAlpha(1f);
        }
    }
}

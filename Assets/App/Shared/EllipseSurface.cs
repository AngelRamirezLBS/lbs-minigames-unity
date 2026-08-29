using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// True elliptic UGUI surface (MaskableGraphic). Generates a filled ellipse
    /// via a center + perimeter fan, unlike <see cref="RoundedSurface"/> which is a
    /// rectangle with rounded corners (stadium/capsule). Use for oval backgrounds.
    /// </summary>
    public sealed class EllipseSurface : MaskableGraphic
    {
        [SerializeField, Range(3, 64)]
        private int segments = 32;

        public int Segments
        {
            get => segments;
            set
            {
                segments = Mathf.Clamp(value, 3, 64);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            float rx = rect.width * 0.5f;
            float ry = rect.height * 0.5f;
            if (rx <= 0f || ry <= 0f || segments < 3)
            {
                return;
            }

            Vector2 center = rect.center;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = center;
            vh.AddVert(vert);

            // Perimeter vertices
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                // Perfect ellipse — true oval, not capsule. Subtle organic irregularity
                // could be added as: rx * (1f + 0.03f * Mathf.Sin(angle * 2f)) etc., but
                // keeping it exact per request "verdadero ovalo eliptico".
                Vector2 pos = center + new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
                vert.position = pos;
                vert.color = color;
                vh.AddVert(vert);
            }

            // Fan triangles: center (0), perimeter 1..segments
            for (int i = 0; i < segments; i++)
            {
                int i0 = 0;
                int i1 = i + 1;
                int i2 = i + 2 <= segments ? i + 2 : 1;
                vh.AddTriangle(i0, i1, i2);
            }
        }
    }
}

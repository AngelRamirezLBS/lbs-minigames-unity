using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// A dependency-free rounded UGUI surface that also remains a normal graphic raycast target.
    /// </summary>
    public sealed class RoundedSurface : MaskableGraphic
    {
        // Cap the total tessellation (a near-full circle needs a lot; a small card corner
        // does not). Larger radii get more segments, so the avatar reads as a smooth curve
        // while card corners stay cheap — this keeps the Hub UGUI vertex count low enough
        // to scroll at full frame rate on Android tablets/TVs.
        private const int MaxCornerSegments = 32;
        private const int MinCornerSegments = 8;
        private const float SegmentsPerPixel = 0.35f;

        [SerializeField, Min(0f)] private float cornerRadius = 24f;

        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = rectTransform.rect;
            float radius = Mathf.Min(cornerRadius, rect.width * 0.5f, rect.height * 0.5f);
            if (radius <= 0f)
            {
                AddRectangle(vertexHelper, rect);
                return;
            }

            // Adaptive tessellation: the bigger the radius, the more segments (smooth);
            // small card corners use few segments (cheap). Clamped so it stays sane.
            int cornerSegments = Mathf.Clamp(Mathf.RoundToInt(radius * SegmentsPerPixel), MinCornerSegments, MaxCornerSegments);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = rect.center;
            vertexHelper.AddVert(vertex);

            Vector2[] centers =
            {
                new(rect.xMax - radius, rect.yMin + radius),
                new(rect.xMax - radius, rect.yMax - radius),
                new(rect.xMin + radius, rect.yMax - radius),
                new(rect.xMin + radius, rect.yMin + radius)
            };
            float[] startAngles = { -90f, 0f, 90f, 180f };

            int perimeterCount = 0;
            for (int corner = 0; corner < centers.Length; corner++)
            {
                int firstSegment = corner == 0 ? 0 : 1;
                for (int segment = firstSegment; segment <= cornerSegments; segment++)
                {
                    float angle = (startAngles[corner] + (90f * segment / cornerSegments)) * Mathf.Deg2Rad;
                    vertex.position = centers[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    vertexHelper.AddVert(vertex);
                    perimeterCount++;
                }
            }

            for (int index = 0; index < perimeterCount; index++)
            {
                vertexHelper.AddTriangle(0, index + 1, index == perimeterCount - 1 ? 1 : index + 2);
            }
        }

        private void AddRectangle(VertexHelper vertexHelper, Rect rect)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector2(rect.xMin, rect.yMin);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(rect.xMin, rect.yMax);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(rect.xMax, rect.yMax);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(rect.xMax, rect.yMin);
            vertexHelper.AddVert(vertex);

            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// A dependency-free rounded UGUI surface that also remains a normal graphic raycast target.
    /// </summary>
    public sealed class RoundedSurface : MaskableGraphic
    {
        private const int CornerSegments = 12;

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
                for (int segment = firstSegment; segment <= CornerSegments; segment++)
                {
                    float angle = (startAngles[corner] + (90f * segment / CornerSegments)) * Mathf.Deg2Rad;
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

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
        [SerializeField, Min(0f)] private float outlineThickness = 0f;

        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public float OutlineThickness
        {
            get => outlineThickness;
            set
            {
                outlineThickness = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            if (outlineThickness > 0.01f)
            {
                float thickness = Mathf.Min(outlineThickness, Mathf.Min(rect.width, rect.height) * 0.5f);
                Rect innerRect = new(rect.xMin + thickness, rect.yMin + thickness, rect.width - thickness * 2f, rect.height - thickness * 2f);
                if (innerRect.width > 0f && innerRect.height > 0f)
                {
                    float outerR = Mathf.Min(cornerRadius, rect.width * 0.5f, rect.height * 0.5f);
                    float innerR = Mathf.Max(0f, outerR - thickness);
                    innerR = Mathf.Min(innerR, innerRect.width * 0.5f, innerRect.height * 0.5f);
                    if (outerR <= 0f && innerR <= 0f)
                    {
                        UIVertex vt = UIVertex.simpleVert;
                        vt.color = color;
                        Vector2[] outer = { new(rect.xMin, rect.yMin), new(rect.xMin, rect.yMax), new(rect.xMax, rect.yMax), new(rect.xMax, rect.yMin) };
                        Vector2[] inner = { new(innerRect.xMin, innerRect.yMin), new(innerRect.xMin, innerRect.yMax), new(innerRect.xMax, innerRect.yMax), new(innerRect.xMax, innerRect.yMin) };
                        for (int i = 0; i < 4; i++) { vt.position = outer[i]; vertexHelper.AddVert(vt); }
                        for (int i = 0; i < 4; i++) { vt.position = inner[i]; vertexHelper.AddVert(vt); }
                        for (int i = 0; i < 4; i++)
                        {
                            int o0 = i, o1 = (i + 1) % 4, i0 = 4 + i, i1 = 4 + (i + 1) % 4;
                            vertexHelper.AddTriangle(o0, o1, i1);
                            vertexHelper.AddTriangle(o0, i1, i0);
                        }
                        return;
                    }
                    Vector2[] outerCenters =
                    {
                        new(rect.xMax - outerR, rect.yMin + outerR),
                        new(rect.xMax - outerR, rect.yMax - outerR),
                        new(rect.xMin + outerR, rect.yMax - outerR),
                        new(rect.xMin + outerR, rect.yMin + outerR)
                    };
                    Vector2[] innerCenters =
                    {
                        new(innerRect.xMax - innerR, innerRect.yMin + innerR),
                        new(innerRect.xMax - innerR, innerRect.yMax - innerR),
                        new(innerRect.xMin + innerR, innerRect.yMax - innerR),
                        new(innerRect.xMin + innerR, innerRect.yMin + innerR)
                    };
                    float[] startAngles = { -90f, 0f, 90f, 180f };
                    var outerPts = new System.Collections.Generic.List<Vector2>();
                    var innerPts = new System.Collections.Generic.List<Vector2>();
                    for (int corner = 0; corner < 4; corner++)
                    {
                        int firstSegment = corner == 0 ? 0 : 1;
                        for (int segment = firstSegment; segment <= CornerSegments; segment++)
                        {
                            float angle = (startAngles[corner] + (90f * segment / CornerSegments)) * Mathf.Deg2Rad;
                            Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
                            outerPts.Add(outerCenters[corner] + dir * outerR);
                            innerPts.Add(innerCenters[corner] + dir * innerR);
                        }
                    }
                    UIVertex v = UIVertex.simpleVert;
                    v.color = color;
                    int n = outerPts.Count;
                    for (int i = 0; i < n; i++) { v.position = outerPts[i]; vertexHelper.AddVert(v); }
                    for (int i = 0; i < n; i++) { v.position = innerPts[i]; vertexHelper.AddVert(v); }
                    for (int i = 0; i < n; i++)
                    {
                        int o0 = i, o1 = (i + 1) % n, i0 = n + i, i1 = n + (i + 1) % n;
                        vertexHelper.AddTriangle(o0, o1, i1);
                        vertexHelper.AddTriangle(o0, i1, i0);
                    }
                    return;
                }
            }

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
            float[] startAnglesFilled = { -90f, 0f, 90f, 180f };

            int perimeterCount = 0;
            for (int corner = 0; corner < centers.Length; corner++)
            {
                int firstSegment = corner == 0 ? 0 : 1;
                for (int segment = firstSegment; segment <= CornerSegments; segment++)
                {
                    float angle = (startAnglesFilled[corner] + (90f * segment / CornerSegments)) * Mathf.Deg2Rad;
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

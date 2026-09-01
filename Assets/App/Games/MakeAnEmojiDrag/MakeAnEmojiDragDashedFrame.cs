using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.MakeAnEmojiDrag
{
    /// <summary>
    /// Presentation-only dashed destination grid for the emoji assembly board.
    /// </summary>
    public sealed class MakeAnEmojiDragDashedFrame : MaskableGraphic
    {
        private const float BorderThickness = 7f;
        private const float CornerRadius = 34f;
        private const float DashLength = 22f;
        private const float DashGap = 14f;
        private const int CornerSegments = 12;
        private static readonly Color PanelColor = Color.white;
        private static readonly Color BorderColor = new(0.78f, 0.82f, 0.95f);

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            AddRoundedFill(vertexHelper, rect, Mathf.Min(CornerRadius, rect.width * .5f, rect.height * .5f));
            AddDashedRoundedOutline(vertexHelper, rect);

            float dividerOffset = rect.height * (127.5f / 660f);
            float inset = BorderThickness * .5f;
            AddDashedLine(vertexHelper, new Vector2(rect.xMin + inset, dividerOffset), new Vector2(rect.xMax - inset, dividerOffset));
            AddDashedLine(vertexHelper, new Vector2(rect.xMin + inset, -dividerOffset), new Vector2(rect.xMax - inset, -dividerOffset));
        }

        private static void AddRoundedFill(VertexHelper vertexHelper, Rect rect, float radius)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = PanelColor;
            vertex.position = rect.center;
            vertexHelper.AddVert(vertex);

            List<Vector2> perimeter = RoundedPerimeter(rect, radius);
            for (int index = 0; index < perimeter.Count; index++)
            {
                vertex.position = perimeter[index];
                vertexHelper.AddVert(vertex);
            }

            for (int index = 0; index < perimeter.Count; index++)
            {
                vertexHelper.AddTriangle(0, index + 1, index == perimeter.Count - 1 ? 1 : index + 2);
            }
        }

        private static void AddDashedRoundedOutline(VertexHelper vertexHelper, Rect rect)
        {
            float inset = BorderThickness * .5f;
            Rect outlineRect = new(rect.xMin + inset, rect.yMin + inset, rect.width - BorderThickness, rect.height - BorderThickness);
            float radius = Mathf.Max(0f, Mathf.Min(CornerRadius - inset, outlineRect.width * .5f, outlineRect.height * .5f));
            AddDashedPolyline(vertexHelper, RoundedPerimeter(outlineRect, radius), true);
        }

        private static List<Vector2> RoundedPerimeter(Rect rect, float radius)
        {
            if (radius <= 0f)
            {
                return new List<Vector2>
                {
                    new(rect.xMax, rect.yMin), new(rect.xMax, rect.yMax), new(rect.xMin, rect.yMax), new(rect.xMin, rect.yMin)
                };
            }

            Vector2[] centers =
            {
                new(rect.xMax - radius, rect.yMin + radius), new(rect.xMax - radius, rect.yMax - radius),
                new(rect.xMin + radius, rect.yMax - radius), new(rect.xMin + radius, rect.yMin + radius)
            };
            float[] startAngles = { -90f, 0f, 90f, 180f };
            List<Vector2> perimeter = new();
            for (int corner = 0; corner < centers.Length; corner++)
            {
                int firstSegment = corner == 0 ? 0 : 1;
                for (int segment = firstSegment; segment <= CornerSegments; segment++)
                {
                    float angle = (startAngles[corner] + 90f * segment / CornerSegments) * Mathf.Deg2Rad;
                    perimeter.Add(centers[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                }
            }
            return perimeter;
        }

        private static void AddDashedLine(VertexHelper vertexHelper, Vector2 start, Vector2 end)
        {
            AddDashedPolyline(vertexHelper, new List<Vector2> { start, end }, false);
        }

        private static void AddDashedPolyline(VertexHelper vertexHelper, List<Vector2> points, bool closed)
        {
            float distance = 0f;
            int segmentCount = closed ? points.Count : points.Count - 1;
            for (int index = 0; index < segmentCount; index++)
            {
                Vector2 start = points[index];
                Vector2 end = points[(index + 1) % points.Count];
                Vector2 line = end - start;
                float length = line.magnitude;
                if (length <= 0.01f) continue;

                Vector2 direction = line / length;
                float progressed = 0f;
                while (progressed < length)
                {
                    float phase = distance % (DashLength + DashGap);
                    float remaining = phase < DashLength ? DashLength - phase : DashLength + DashGap - phase;
                    float step = Mathf.Min(remaining, length - progressed);
                    if (phase < DashLength)
                    {
                        AddStrokeSegment(vertexHelper, start + direction * progressed, start + direction * (progressed + step));
                    }
                    progressed += step;
                    distance += step;
                }
            }
        }

        private static void AddStrokeSegment(VertexHelper vertexHelper, Vector2 start, Vector2 end)
        {
            Vector2 line = end - start;
            if (line.sqrMagnitude <= 0.0001f) return;

            Vector2 normal = new Vector2(-line.y, line.x).normalized * (BorderThickness * .5f);
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = BorderColor;
            int startIndex = vertexHelper.currentVertCount;
            vertex.position = start - normal;
            vertexHelper.AddVert(vertex);
            vertex.position = start + normal;
            vertexHelper.AddVert(vertex);
            vertex.position = end + normal;
            vertexHelper.AddVert(vertex);
            vertex.position = end - normal;
            vertexHelper.AddVert(vertex);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
    }
}

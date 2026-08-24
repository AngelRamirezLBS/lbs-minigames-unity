using System;

namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public readonly struct HorizontalLayoutBounds
    {
        public HorizontalLayoutBounds(float minimum, float maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public float Minimum { get; }
        public float Maximum { get; }
    }

    public readonly struct CharacterLayoutBounds
    {
        public CharacterLayoutBounds(HorizontalLayoutBounds left, HorizontalLayoutBounds right)
        {
            Left = left;
            Right = right;
        }

        public HorizontalLayoutBounds Left { get; }
        public HorizontalLayoutBounds Right { get; }
    }

    public readonly struct ReservedInputLayoutBounds
    {
        public ReservedInputLayoutBounds(float leftMaximum, float rightMinimum)
        {
            LeftMaximum = leftMaximum;
            RightMinimum = rightMinimum;
        }

        public float LeftMaximum { get; }
        public float RightMinimum { get; }
    }

    public static class NumberPullBoardLayout
    {
        public const float LeftInputMaximum = 0.355f;
        public const float RightInputMinimum = 0.645f;
        public const float CentralStageLeft = 0.370f;
        public const float CentralStageRight = 0.630f;
        public const float RopeLeftAnchor = 0.380f;
        public const float RopeRightAnchor = 0.620f;
        public const float LeftCharacterAnchor = 0.405f;
        public const float RightCharacterAnchor = 0.595f;
        public const float CharacterWidth = 118f;
        public const float CharacterHeight = 143f;
        public const float MaximumHorizontalMotion = 12f;
        public const float MaximumInputMotion = 6f;
        public const float KnotStep = 24f;
        public const float KnotDiameter = 52f;
        public const int MaxPullSteps = 5;

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float RopeEdgeMargin = 0.015f;

        public static CharacterLayoutBounds CalculateCharacterBounds(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            double safeCanvasWidth = CalculateSafeCanvasWidth(screenWidth, screenHeight, safeAreaWidth);
            float horizontalExtent = (float)((CharacterWidth * 0.5f + MaximumHorizontalMotion) / safeCanvasWidth);

            return new CharacterLayoutBounds(
                new HorizontalLayoutBounds(LeftCharacterAnchor - horizontalExtent, LeftCharacterAnchor + horizontalExtent),
                new HorizontalLayoutBounds(RightCharacterAnchor - horizontalExtent, RightCharacterAnchor + horizontalExtent));
        }

        public static ReservedInputLayoutBounds CalculateReservedInputBounds(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            double safeCanvasWidth = CalculateSafeCanvasWidth(screenWidth, screenHeight, safeAreaWidth);
            float motion = (float)(MaximumInputMotion / safeCanvasWidth);
            return new ReservedInputLayoutBounds(LeftInputMaximum + motion, RightInputMinimum - motion);
        }

        public static HorizontalLayoutBounds CalculateRopeBounds()
        {
            return new HorizontalLayoutBounds(RopeLeftAnchor, RopeRightAnchor);
        }

        public static HorizontalLayoutBounds CalculateKnotCenterBounds(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            double safeCanvasWidth = CalculateSafeCanvasWidth(screenWidth, screenHeight, safeAreaWidth);
            float maxOffset = (MaxPullSteps * KnotStep) / (float)safeCanvasWidth;
            const float center = 0.5f;
            return new HorizontalLayoutBounds(center - maxOffset, center + maxOffset);
        }

        public static bool IsRopeContainedWithMargins()
        {
            return RopeLeftAnchor >= LeftInputMaximum + RopeEdgeMargin &&
                   RopeRightAnchor <= RightInputMinimum - RopeEdgeMargin &&
                   RopeLeftAnchor >= CentralStageLeft + 0.005f &&
                   RopeRightAnchor <= CentralStageRight - 0.005f;
        }

        public static bool IsKnotContainedAtMaxPull(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            HorizontalLayoutBounds rope = CalculateRopeBounds();
            HorizontalLayoutBounds knot = CalculateKnotCenterBounds(screenWidth, screenHeight, safeAreaWidth);
            double safeCanvasWidth = CalculateSafeCanvasWidth(screenWidth, screenHeight, safeAreaWidth);
            float knotHalfNormalized = (KnotDiameter * 0.5f) / (float)safeCanvasWidth;
            return knot.Minimum - knotHalfNormalized >= rope.Minimum &&
                   knot.Maximum + knotHalfNormalized <= rope.Maximum;
        }

        public static float CalculateCharacterSeparation(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            CharacterLayoutBounds bounds = CalculateCharacterBounds(screenWidth, screenHeight, safeAreaWidth);
            return bounds.Right.Minimum - bounds.Left.Maximum;
        }

        private static double CalculateSafeCanvasWidth(int screenWidth, int screenHeight, float safeAreaWidth)
        {
            if (screenWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenWidth));
            }

            if (screenHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenHeight));
            }

            if (safeAreaWidth <= 0f || safeAreaWidth > screenWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(safeAreaWidth));
            }

            double widthScale = screenWidth / ReferenceWidth;
            double heightScale = screenHeight / ReferenceHeight;
            double canvasScale = Math.Sqrt(widthScale * heightScale);
            return safeAreaWidth / canvasScale;
        }
    }
}

using System;

namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public readonly struct LayoutRect
    {
        public LayoutRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
    }

    public sealed class SafeAreaLayoutState
    {
        private int screenWidth;
        private int screenHeight;
        private LayoutRect safeArea;
        private LayoutRect normalizedArea;
        private bool initialized;

        public bool TryUpdate(int width, int height, LayoutRect area, out LayoutRect normalized)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            bool changed = !initialized || width != screenWidth || height != screenHeight || !SameRect(area, safeArea);
            if (changed)
            {
                screenWidth = width;
                screenHeight = height;
                safeArea = area;
                float minimumX = Clamp(area.X, 0f, width);
                float minimumY = Clamp(area.Y, 0f, height);
                float maximumX = Clamp(area.X + Math.Max(0f, area.Width), minimumX, width);
                float maximumY = Clamp(area.Y + Math.Max(0f, area.Height), minimumY, height);
                normalizedArea = new LayoutRect(
                    minimumX / width,
                    minimumY / height,
                    (maximumX - minimumX) / width,
                    (maximumY - minimumY) / height);
                initialized = true;
            }

            normalized = normalizedArea;
            return changed;
        }

        private static bool SameRect(LayoutRect left, LayoutRect right)
        {
            return left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    internal static class UiFactory
    {
        public static Image CreateImage(RectTransform parent, string name, Color color)
        {
            GameObject gameObject = new(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(RectTransform parent, string name, Font font, int size, TextAnchor alignment, Color color)
        {
            GameObject gameObject = new(name, typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
            return text;
        }

        public static void ApplySyntheticHeaderStroke(Text text, Color strokeColor)
        {
            text.fontStyle = FontStyle.Normal;
            text.resizeTextForBestFit = false;
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = strokeColor;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        public static Button CreateButton(RectTransform parent, string name, Font font, string label, Color color)
        {
            Image image = CreateImage(parent, name, color);
            Button button = image.gameObject.AddComponent<Button>();
            Text text = CreateText(image.rectTransform, "Label", font, 28, TextAnchor.MiddleCenter, Color.white);
            text.text = label;
            Stretch(text.rectTransform, 8f);
            return button;
        }

        public static RoundedSurface CreateRoundedSurface(
            RectTransform parent,
            string name,
            Color color,
            float cornerRadius,
            bool raycastTarget = true)
        {
            GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RoundedSurface));
            gameObject.transform.SetParent(parent, false);
            RoundedSurface surface = gameObject.GetComponent<RoundedSurface>();
            surface.color = color;
            surface.CornerRadius = cornerRadius;
            surface.raycastTarget = raycastTarget;
            return surface;
        }

        public static Button CreateRoundedButton(
            RectTransform parent,
            string name,
            Font font,
            string label,
            Color surfaceColor,
            Color labelColor,
            float cornerRadius)
        {
            RoundedSurface surface = CreateRoundedSurface(parent, name, surfaceColor, cornerRadius);
            Button button = surface.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;

            Text text = CreateText(surface.rectTransform, "Label", font, 24, TextAnchor.MiddleCenter, labelColor);
            text.text = label;
            text.raycastTarget = false;
            Stretch(text.rectTransform, 12f);
            return button;
        }

        public static void Stretch(RectTransform rectTransform, float padding)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(padding, padding);
            rectTransform.offsetMax = new Vector2(-padding, -padding);
        }

        public static void Anchor(RectTransform rectTransform, Vector2 min, Vector2 max)
        {
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}

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

        public static Button CreateButton(RectTransform parent, string name, Font font, string label, Color color)
        {
            Image image = CreateImage(parent, name, color);
            Button button = image.gameObject.AddComponent<Button>();
            Text text = CreateText(image.rectTransform, "Label", font, 28, TextAnchor.MiddleCenter, Color.white);
            text.text = label;
            Stretch(text.rectTransform, 8f);
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

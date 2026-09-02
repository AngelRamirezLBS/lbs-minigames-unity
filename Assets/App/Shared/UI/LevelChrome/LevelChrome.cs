using System;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared.UI
{
    /// <summary>
    /// Controller for reusable Exit + Hong chrome. Created via LevelChromeFactory; owns injected callbacks, no singleton.
    /// </summary>
    public sealed class LevelChrome : MonoBehaviour
    {
        private Button exitButton;
        private Button hongButton;
        private Image hongImage;
        private Action onExit;
        private Action onHong;

        public Button ExitButton => exitButton;
        public Button HongButton => hongButton;
        public Image HongImage => hongImage;

        public void Configure(Button exit, Button hong, Image hongImg, Action exitCallback, Action hongCallback)
        {
            exitButton = exit;
            hongButton = hong;
            hongImage = hongImg;
            onExit = exitCallback;
            onHong = hongCallback;

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                if (onExit != null) exitButton.onClick.AddListener(() => onExit.Invoke());
            }

            if (hongButton != null)
            {
                hongButton.onClick.RemoveAllListeners();
                if (onHong != null) hongButton.onClick.AddListener(() => onHong.Invoke());
            }
        }

        public void SetHongSprite(Sprite sprite)
        {
            if (hongImage != null) hongImage.sprite = sprite;
        }

        private void OnDestroy()
        {
            if (exitButton != null) exitButton.onClick.RemoveAllListeners();
            if (hongButton != null) hongButton.onClick.RemoveAllListeners();
            onExit = null;
            onHong = null;
        }
    }
}

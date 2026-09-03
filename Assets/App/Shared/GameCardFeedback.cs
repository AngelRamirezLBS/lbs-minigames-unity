using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// Keeps press and opening feedback scoped to one game card.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class GameCardFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        private readonly HashSet<int> activePointers = new();

        private RoundedSurface outline;
        private GameObject openingCue;
        private Color defaultOutline;
        private Color pressedOutline;
        private bool isOpening;
        // Absorbs the "ghost tap" that can leak from the previous scene into the Hub the
        // same frame it loads: when a game returns to the Hub, the pointer-up that confirmed
        // the return is still active and, if it lands on a newly-created card, would launch
        // that game. Lock input for the first frame(s) after the card is built so that tap is
        // dropped instead of selecting a card under the finger. A frame counter (not a bool)
        // covers the order-of-Update race with the EventSystem.
        private int inputLockFrames;

        public event Action<GameCardFeedback> SelectionRequested;

        public void Configure(
            RoundedSurface cardOutline,
            GameObject cardOpeningCue,
            Color normalOutline,
            Color activeOutline)
        {
            outline = cardOutline;
            openingCue = cardOpeningCue;
            defaultOutline = normalOutline;
            pressedOutline = activeOutline;
            openingCue.SetActive(false);
            outline.color = defaultOutline;
            // Drop any in-flight pointer event for this frame so returning to the Hub does
            // not launch a card under the finger. Release after a couple of frames.
            inputLockFrames = 2;
        }

        private void Update()
        {
            if (inputLockFrames > 0)
            {
                inputLockFrames--;
            }
        }

        public void MarkOpening()
        {
            if (isOpening)
            {
                return;
            }

            isOpening = true;
            openingCue.SetActive(true);
        }

        public void ResetOpening()
        {
            isOpening = false;
            openingCue.SetActive(false);
            activePointers.Clear();
            outline.color = defaultOutline;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isOpening || inputLockFrames > 0)
            {
                return;
            }

            activePointers.Add(eventData.pointerId);
            RefreshPressPresentation();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            activePointers.Remove(eventData.pointerId);
            RefreshPressPresentation();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            activePointers.Remove(eventData.pointerId);
            RefreshPressPresentation();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // The ghost tap from the returning scene must not select a card under the finger.
            if (!isOpening && inputLockFrames <= 0)
            {
                SelectionRequested?.Invoke(this);
            }
        }

        private void RefreshPressPresentation()
        {
            if (isOpening)
            {
                return;
            }

            bool isPressed = activePointers.Count > 0;
            outline.color = isPressed ? pressedOutline : defaultOutline;
        }
    }
}

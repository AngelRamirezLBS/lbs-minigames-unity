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

        private RectTransform cardTransform;
        private RoundedSurface outline;
        private GameObject openingCue;
        private Color defaultOutline;
        private Color pressedOutline;
        private bool isOpening;

        public event Action<GameCardFeedback> SelectionRequested;

        public void Configure(
            RoundedSurface cardOutline,
            GameObject cardOpeningCue,
            Color normalOutline,
            Color activeOutline)
        {
            cardTransform = (RectTransform)transform;
            outline = cardOutline;
            openingCue = cardOpeningCue;
            defaultOutline = normalOutline;
            pressedOutline = activeOutline;
            openingCue.SetActive(false);
            outline.color = defaultOutline;
        }

        public void MarkOpening()
        {
            if (isOpening)
            {
                return;
            }

            isOpening = true;
            openingCue.SetActive(true);
            cardTransform.localScale = Vector3.one * 0.98f;
        }

        public void ResetOpening()
        {
            isOpening = false;
            openingCue.SetActive(false);
            activePointers.Clear();
            cardTransform.localScale = Vector3.one;
            outline.color = defaultOutline;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isOpening)
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
            if (!isOpening)
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
            cardTransform.localScale = isPressed ? Vector3.one * 0.96f : Vector3.one;
            outline.color = isPressed ? pressedOutline : defaultOutline;
        }
    }
}

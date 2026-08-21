using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lbs.MiniGames.Games.Common
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class DragDropToken : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private Vector2 origin;
        private bool accepted;
        private bool dropResolved;
        private int activePointerId = int.MinValue;

        public string TokenId { get; private set; }
        public event Action<DragDropToken> DragStarted;
        public event Action<DragDropToken> Dropped;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
            origin = ((RectTransform)transform).anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (accepted || eventData == null || activePointerId != int.MinValue)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            dropResolved = false;
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (accepted || !HasActivePointer(eventData))
            {
                return;
            }

            ((RectTransform)transform).anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!HasActivePointer(eventData))
            {
                return;
            }

            activePointerId = int.MinValue;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = !accepted;

            if (!accepted)
            {
                ((RectTransform)transform).anchoredPosition = origin;
            }
        }

        public bool TryResolveDrop(PointerEventData eventData)
        {
            if (accepted || dropResolved || !HasActivePointer(eventData))
            {
                return false;
            }

            dropResolved = true;
            return true;
        }

        public void SetTokenId(string value)
        {
            TokenId = value;
        }

        public void Accept(RectTransform resolvedTokensRoot, int slotIndex, int slotCount)
        {
            accepted = true;
            transform.SetParent(resolvedTokensRoot, false);

            RectTransform rectTransform = (RectTransform)transform;
            float horizontalPosition = (slotIndex + 1f) / (slotCount + 1f);
            rectTransform.anchorMin = new Vector2(horizontalPosition, 0.5f);
            rectTransform.anchorMax = new Vector2(horizontalPosition, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(136f, 136f);
            canvasGroup.blocksRaycasts = false;
            Dropped?.Invoke(this);
        }

        private bool HasActivePointer(PointerEventData eventData)
        {
            return eventData != null && activePointerId == eventData.pointerId;
        }
    }
}

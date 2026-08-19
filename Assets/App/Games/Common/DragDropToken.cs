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

        public event Action<DragDropToken> Dropped;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
            origin = ((RectTransform)transform).anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            accepted = false;
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            ((RectTransform)transform).anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (!accepted)
            {
                ((RectTransform)transform).anchoredPosition = origin;
            }
        }

        public void Accept()
        {
            accepted = true;
            Dropped?.Invoke(this);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lbs.MiniGames.GameKits.DragDrop
{
    /// <summary>
    /// Reusable drag-drop token with explicit one-pointer ownership, symmetric cleanup, and lift/restore/accept hooks.
    /// One pointer at a time; fair same-frame resolution via pointerId ownership.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class DragDropCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup group;
        private RectTransform rect;
        private Vector2 origin;
        private int activePointer = int.MinValue;
        private bool accepted;
        private string tokenId;

        public string TokenId => tokenId;
        public bool IsDragging => activePointer != int.MinValue;
        public bool IsAccepted => accepted;

        public event Action<DragDropCard, PointerEventData> DragStarted;
        public event Action<DragDropCard, PointerEventData> DragMoved;
        public event Action<DragDropCard, PointerEventData> DragEnded;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            rect = (RectTransform)transform;
            origin = rect.anchoredPosition;
        }

        public void SetTokenId(string id) { tokenId = id; }

        public void Setup(string id, CanvasGroup canvasGroup, Vector2 originPos)
        {
            tokenId = id;
            group = canvasGroup != null ? canvasGroup : GetComponent<CanvasGroup>();
            rect = (RectTransform)transform;
            origin = originPos;
            rect.anchoredPosition = originPos;
            accepted = false;
            activePointer = int.MinValue;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (accepted || eventData == null || activePointer != int.MinValue) return;
            activePointer = eventData.pointerId;
            DragStarted?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (accepted || !HasActivePointer(eventData)) return;
            DragMoved?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!HasActivePointer(eventData)) return;
            activePointer = int.MinValue;
            DragEnded?.Invoke(this, eventData);
            // If not accepted, restore is handled by owner after rule evaluation; fallback restore if owner didn't handle.
            // We do NOT auto-restore here to allow owner to decide (outside vs incorrect vs correct).
        }

        public void Lift()
        {
            if (group != null)
            {
                group.alpha = 0.75f;
                group.blocksRaycasts = false;
            }
            transform.SetAsLastSibling();
        }

        public void Restore()
        {
            if (rect != null) rect.anchoredPosition = origin;
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = !accepted;
            }
            activePointer = int.MinValue;
        }

        public void Accept(RectTransform targetRoot)
        {
            accepted = true;
            if (targetRoot != null)
            {
                rect.anchoredPosition = targetRoot.anchoredPosition;
            }
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = false;
            }
            activePointer = int.MinValue;
        }

        public void ResetCard()
        {
            accepted = false;
            activePointer = int.MinValue;
            if (rect != null) rect.anchoredPosition = origin;
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }
        }

        private bool HasActivePointer(PointerEventData eventData)
        {
            return eventData != null && activePointer == eventData.pointerId;
        }

        private void OnDisable()
        {
            // Symmetric cleanup: release pointer, restore visual if not accepted.
            if (activePointer != int.MinValue)
            {
                activePointer = int.MinValue;
            }
            if (!accepted && group != null)
            {
                group.blocksRaycasts = true;
            }
        }

        private void OnDestroy()
        {
            DragStarted = null;
            DragMoved = null;
            DragEnded = null;
            activePointer = int.MinValue;
        }

        // For board shake / animator to re-anchor origin after layout changes.
        public void UpdateOrigin(Vector2 newOrigin) { origin = newOrigin; }
    }
}

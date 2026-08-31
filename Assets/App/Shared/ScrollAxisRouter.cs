using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// Resolves the classic nested-ScrollRect drag conflict between a horizontal card row
    /// and the vertically-scrolling page. Axis is decided by ACCUMULATED movement beyond a
    /// threshold (not the noisy first-frame delta), then locked for the rest of the gesture:
    /// mostly-vertical -> page scroll (primary); mostly-horizontal -> row scroll (secondary).
    /// </summary>
    public sealed class ScrollAxisRouter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float AxisThreshold = 18f;

        [SerializeField] private ScrollRect horizontalScroll;

        // Drag state.
        private bool dragging;
        private bool lockedHorizontal;
        private bool axisAssigned;
        private Vector2 dragStart;

        public void Configure(ScrollRect rowScroll)
        {
            horizontalScroll = rowScroll;
        }

        private ScrollRect FindPageVerticalScroll()
        {
            foreach (ScrollRect candidate in GetComponentsInParent<ScrollRect>(true))
            {
                if (candidate == horizontalScroll)
                {
                    continue;
                }

                if (candidate.vertical)
                {
                    return candidate;
                }
            }

            return null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ScrollRect page = FindPageVerticalScroll();
            if (page == null)
            {
                return;
            }

            dragging = true;
            lockedHorizontal = false;
            axisAssigned = false;
            dragStart = eventData.position;
            // The row scroll must not consume the gesture while the axis is still being
            // decided, so neither scroll is disabled yet (both idle until the lock).
            eventData.pointerDrag = gameObject;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            ScrollRect page = FindPageVerticalScroll();
            if (page == null)
            {
                return;
            }

            if (!axisAssigned)
            {
                Vector2 moved = eventData.position - dragStart;
                if (Mathf.Abs(moved.x) < AxisThreshold && Mathf.Abs(moved.y) < AxisThreshold)
                {
                    // Not enough movement to decide yet.
                    return;
                }

                lockedHorizontal = Mathf.Abs(moved.x) > Mathf.Abs(moved.y);

                if (lockedHorizontal && horizontalScroll != null)
                {
                    // Take over horizontally: the page scroll is disabled and future OnDrag
                    // events route to the row via pointerDrag. Prime the row's drag once.
                    axisAssigned = true;
                    page.enabled = false;
                    horizontalScroll.enabled = true;
                    eventData.pointerDrag = horizontalScroll.gameObject;
                    horizontalScroll.OnBeginDrag(eventData);
                }
                else
                {
                    // Let the page vertical scroll own the rest of the gesture.
                    axisAssigned = true;
                    if (horizontalScroll != null)
                    {
                        horizontalScroll.enabled = false;
                    }

                    page.enabled = true;
                    eventData.pointerDrag = page.gameObject;
                    page.OnBeginDrag(eventData);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            lockedHorizontal = false;
            axisAssigned = false;
            if (horizontalScroll != null)
            {
                horizontalScroll.enabled = true;
            }

            ScrollRect page = FindPageVerticalScroll();
            if (page != null)
            {
                page.enabled = true;
            }
        }
    }
}

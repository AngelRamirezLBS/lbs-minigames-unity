using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared
{
    /// <summary>
    /// Resolves the classic nested-ScrollRect drag conflict between a horizontal card row
    /// and the vertically-scrolling page. Unity's EventSystem drag threshold filters noisy
    /// movement first; this router then locks the gesture to its dominant axis immediately.
    /// </summary>
    public sealed class ScrollAxisRouter : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private ScrollRect horizontalScroll;

        private ScrollRect pageVerticalScroll;

        public void Configure(ScrollRect rowScroll)
        {
            horizontalScroll = rowScroll;
            CachePageVerticalScroll();
        }

        private void CachePageVerticalScroll()
        {
            foreach (ScrollRect candidate in GetComponentsInParent<ScrollRect>(true))
            {
                if (candidate == horizontalScroll)
                {
                    continue;
                }

                if (candidate.vertical)
                {
                    pageVerticalScroll = candidate;
                    return;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (pageVerticalScroll == null)
            {
                CachePageVerticalScroll();
            }

            if (pageVerticalScroll == null)
            {
                return;
            }

            Vector2 moved = eventData.position - eventData.pressPosition;
            bool horizontalGesture = Mathf.Abs(moved.x) > Mathf.Abs(moved.y);

            if (horizontalGesture && horizontalScroll != null)
            {
                pageVerticalScroll.enabled = false;
                horizontalScroll.enabled = true;
                eventData.pointerDrag = horizontalScroll.gameObject;
                return;
            }

            // The page did not receive the EventSystem's begin-drag callback because the
            // gesture started over the nested row, so prime it before handing off the drag.
            // Close the row's begin-drag state because it will not receive the end callback
            // after pointerDrag changes to the page.
            if (horizontalScroll != null)
            {
                horizontalScroll.OnEndDrag(eventData);
            }

            pageVerticalScroll.enabled = true;
            eventData.pointerDrag = pageVerticalScroll.gameObject;
            pageVerticalScroll.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (horizontalScroll != null)
            {
                horizontalScroll.enabled = true;
            }

            if (pageVerticalScroll != null)
            {
                pageVerticalScroll.enabled = true;
            }
        }
    }
}

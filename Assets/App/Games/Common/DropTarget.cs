using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lbs.MiniGames.Games.Common
{
    public sealed class DropTarget : MonoBehaviour, IDropHandler
    {
        [SerializeField] private string classificationId;

        public string ClassificationId => classificationId;
        public event Action<DropTarget, DragDropToken> TokenDropped;

        public void SetClassificationId(string value)
        {
            classificationId = value;
        }

        public void OnDrop(PointerEventData eventData)
        {
            DragDropToken token = eventData == null || eventData.pointerDrag == null
                ? null
                : eventData.pointerDrag.GetComponent<DragDropToken>();

            if (token != null && token.TryResolveDrop(eventData))
            {
                TokenDropped?.Invoke(this, token);
            }
        }
    }
}

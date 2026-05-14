using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkLandsUI.Scripts.Equipment
{
    public class SlotItemGrab : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public Slot slot;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slot.IsEmpty()) return;
            DragItemHolder.Instance.StartDrag(slot);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragItemHolder.Instance.DropItem();
        }

        public void OnDrag(PointerEventData eventData)
        {
            //blank implementation. IDragHandler is required for IBeginDragHandler, IEndDragHandler
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkLandsUI.Scripts.Equipment
{
    public class SlotItemDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Slot slot;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            var dragItemHolder = DragItemHolder.Instance;
            if (dragItemHolder.dragging && slot.IsEmpty() && slot.IsItemFit(dragItemHolder.dragItem))
            {
                dragItemHolder.TargetSlotToDrop(slot);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DragItemHolder.Instance.RemoveTarget(slot);
        }
    }
}
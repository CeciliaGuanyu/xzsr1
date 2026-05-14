using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkLandsUI.Scripts.Equipment
{
    public class InventorySlotItemDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Inventory inventory;
        private InventorySlot _placeholdedSlot;

        public void OnPointerEnter(PointerEventData eventData)
        {
            var dragItemHolder = DragItemHolder.Instance;
            if (!dragItemHolder.dragging || !inventory.HasFreeSpace()) return;
            _placeholdedSlot = inventory.GetNextEmptySlotForItem(dragItemHolder.dragItem);
            dragItemHolder.TargetSlotToDrop(_placeholdedSlot);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_placeholdedSlot == null) return;
            DragItemHolder.Instance.RemoveTarget(_placeholdedSlot);
            _placeholdedSlot = null;
        }
    }
}
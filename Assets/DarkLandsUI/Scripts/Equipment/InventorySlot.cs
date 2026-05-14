using UnityEngine;

namespace DarkLandsUI.Scripts.Equipment
{
    public class InventorySlot : Slot
    {
        public Inventory inventory;
        [HideInInspector]
        public InventorySlot linkedSlot;

        public override void PlaceItem(Item itemToPlace)
        {
            if (linkedSlot)
            {
                base.ClearSlot();
                linkedSlot.PlaceItem(itemToPlace);
            }
            else
            {
                base.PlaceItem(itemToPlace);
                if (inventory) inventory.RecalculateFreeSpace();
            }
        }

        public override void ClearSlot()
        {
            if (linkedSlot) linkedSlot.ClearSlot();
            base.ClearSlot();
        }

        public void UnlinkedClear()
        {
            base.ClearSlot();
        }

        public void PlaceUnlinked(Item itemToPlace)
        {
            base.PlaceItem(itemToPlace);
        }
    }
}

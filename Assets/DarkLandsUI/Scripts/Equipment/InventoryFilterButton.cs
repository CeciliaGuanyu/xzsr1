using UnityEngine;

namespace DarkLandsUI.Scripts.Equipment
{
    public class InventoryFilterButton : MonoBehaviour
    {
        public ItemType[] types;
        public Inventory inventory;

        public void Filter()
        {
            inventory.FilterByItemType(types);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DarkLandsUI.Scripts.Equipment
{
    public class FilterInventory : MonoBehaviour
    {
        private List<InventorySlot> _slots;

        private void Start()
        {
            _slots = GetComponentsInChildren<InventorySlot>().ToList();
        }

        public void FillFilteredSlots(List<InventorySlot> originalSlots)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (i < originalSlots.Count)
                {
                    _slots[i].PlaceUnlinked(originalSlots[i].item[0]);
                    _slots[i].linkedSlot = originalSlots[i];
                }
                else
                {
                    _slots[i].UnlinkedClear();
                }
            }
        }

        public void RemoveBlanks()
        {
            var items = _slots.Where(s => !s.IsEmpty()).Select(s => s.item[0]).ToList();
            for (var i = 0; i < _slots.Count; i++)
            {
                if (i < items.Count)
                {
                    _slots[i].PlaceItem(items[i]);
                }
                else
                {
                    _slots[i].ClearSlot();
                }
            }
        }
    }
}
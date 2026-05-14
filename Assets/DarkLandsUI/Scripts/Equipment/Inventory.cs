using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DarkLandsUI.Scripts.Equipment
{
    public class Inventory : MonoBehaviour
    {
        private const string BagSpaceFormat = "{0}/{1}";
        public TMP_Text[] spaceValue;
        public FilterInventory filterInventory;
        private List<InventorySlot> _slots;
        private ItemType[] _lastAppliedFilter;

        private void Start()
        {
            _slots = GetComponentsInChildren<InventorySlot>().ToList();
            RecalculateFreeSpace();
        }

        public bool HasFreeSpace()
        {
            return _slots.Any(slot => slot.IsEmpty());
        }

        public InventorySlot GetNextEmptySlotForItem(Item item)
        {
            return _slots.Find(s => s.IsEmpty() && s.IsItemFit(item));
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

            RecalculateFreeSpace();
        }

        public void RecalculateFreeSpace()
        {
            if (spaceValue.Length == 0) return;
            var bagSpace = _slots.Count;
            var freeSpace = _slots.Count(slot => slot.IsEmpty());
            foreach (var tmpText in spaceValue)
            {
                tmpText.text = string.Format(BagSpaceFormat, bagSpace - freeSpace, bagSpace);
            }
            ApplyFilter();
        }

        public void FilterByItemType(ItemType[] types)
        {
            _lastAppliedFilter = types;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_lastAppliedFilter == null || _lastAppliedFilter.Length == 0) return;
            var inventorySlots = _slots.FindAll(slot => !slot.IsEmpty() && _lastAppliedFilter.Contains(slot.item[0].itemType));
            filterInventory.FillFilteredSlots(inventorySlots);
        }
    }
}
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.Equipment
{
    public class Slot : MonoBehaviour
    {
        private static readonly Item[] EmptyItem = new Item[0];
        public Item[] item = EmptyItem;
        public Image itemIcon;
        public TMP_Text countText;
        public Image placedItemImage;
        public SlotType[] allowedSlotTypes = Enum.GetValues(typeof(SlotType)).Cast<SlotType>().ToArray();

        public bool IsEmpty()
        {
            return item.Length == 0;
        }

        public bool IsItemFit(Item itemToPlace)
        {
            return allowedSlotTypes.Contains(itemToPlace.slotType);
        }

        public virtual void PlaceItem(Item itemToPlace)
        {
            item = new[] {itemToPlace};
            ShowItem();
        }

        public void PlaceholdItem(Item itemToPlacehold)
        {
            itemIcon.sprite = itemToPlacehold.sprite;
            itemIcon.color = new Color(1f, 1f, 1f, 0.5f);
            itemIcon.enabled = true;
        }

        public virtual void ClearSlot()
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.white;
            itemIcon.enabled = false;
            countText.enabled = false;
            item = EmptyItem;
            if (placedItemImage)
            {
                placedItemImage.enabled = false;
            }
        }

        private void ShowItem()
        {
            itemIcon.sprite = item[0].sprite;
            itemIcon.color = Color.white;
            itemIcon.enabled = true;
            countText.text = item[0].stackedCount.ToString();
            countText.enabled = item[0].stackedCount > 1;
            if (placedItemImage)
            {
                placedItemImage.enabled = true;
            }
        }

        private void OnValidate()
        {
            if (itemIcon == null || countText == null) return;
            if (item.Length == 0)
            {
                ClearSlot();
                return;
            }

            if (item.Length > 1)
            {
                item = new[] {item[0]};
            }

            ShowItem();
        }
    }
}
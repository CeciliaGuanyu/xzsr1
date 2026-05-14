using System;
using UnityEngine;

namespace DarkLandsUI.Scripts.Equipment
{
    [Serializable]
    public class Item
    {
        public ItemType itemType;
        public SlotType slotType;
        public Sprite sprite;
        [Min(1)]
        public int stackedCount = 1;
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkLandsUI.Scripts.Equipment
{
    public class InventorySlotItemGrab : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private const string InventorySlot = "InventorySlot";
        public Inventory inventory;

        public void OnBeginDrag(PointerEventData eventData)
        {
            var inventorySlot = FindSlot(eventData.position);
            if (inventorySlot.IsEmpty()) return;
            DragItemHolder.Instance.StartDrag(inventorySlot);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragItemHolder.Instance.DropItem();
            inventory.RemoveBlanks();
        }

        public void OnDrag(PointerEventData eventData)
        {
            //blank implementation. IDragHandler is required for IBeginDragHandler, IEndDragHandler
        }
        
        private static InventorySlot FindSlot(Vector3 mousePosition){
            var pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1, position = mousePosition
            };


            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            var slotGo = results.Select(r => r.gameObject).FirstOrDefault(go => go.CompareTag(InventorySlot));
            return slotGo == null ? null : slotGo.GetComponentInParent<InventorySlot>();
        }
    }
}
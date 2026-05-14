using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkLandsUI.Scripts.UI
{
    public class TooltipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TooltipWindow tooltipWindow;
        public string header;
        public string description;
        public Sprite icon;

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipWindow.ShowTooltip(header, description, icon);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipWindow.HideTooltip();
        }
    }
}
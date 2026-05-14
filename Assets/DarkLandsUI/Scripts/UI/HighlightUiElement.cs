using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    public class HighlightUiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image highlight;

        private void Start()
        {
            highlight.fillAmount = 0;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlight.fillAmount = 1;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlight.fillAmount = 0;
        }
    }
}

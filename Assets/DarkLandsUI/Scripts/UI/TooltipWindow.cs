using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    public class TooltipWindow : MonoBehaviour
    {
        public TMP_Text headerText;
        public TMP_Text descriptionText;
        public Image iconSprite;

        private void Start()
        {
            HideTooltip();
        }

        public void ShowTooltip(string header, string description, Sprite icon)
        {
            headerText.text = header;
            descriptionText.text = description;
            iconSprite.sprite = icon;
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}
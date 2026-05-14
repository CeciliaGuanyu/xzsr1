using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    public class SliderPercentController : MonoBehaviour
    {
        private const string FormatText = "{0:F0} %";
        public TMP_Text textComponent;
        public Slider sliderComponent;

        private void Start()
        {
            if (textComponent == null || sliderComponent == null)
            {
                gameObject.SetActive(false);
            }

            sliderComponent.minValue = 0;
            sliderComponent.maxValue = 1;
            sliderComponent.onValueChanged.AddListener(UpdatePercentValue);
            UpdatePercentValue(sliderComponent.value);
        }

        private void UpdatePercentValue(float value)
        {
            textComponent.text = string.Format(FormatText, value * 100f);
        }

        private void OnDestroy()
        {
            if (sliderComponent)
            {
                sliderComponent.onValueChanged.RemoveListener(UpdatePercentValue);
            }
        }
    }
}
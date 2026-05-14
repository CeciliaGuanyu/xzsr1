using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    public class SliderStepController : MonoBehaviour
    {
        public TMP_Text textComponent;
        public Slider sliderComponent;
        public string[] values;

        private void Start()
        {
            if (textComponent == null
                || sliderComponent == null
                || values.Length == 0)
            {
                gameObject.SetActive(false);
            }

            sliderComponent.maxValue = values.Length - 1;
            sliderComponent.wholeNumbers = true;
            sliderComponent.onValueChanged.AddListener(UpdatePercentValue);
            UpdatePercentValue(sliderComponent.value);
        }

        private void UpdatePercentValue(float value)
        {
            var intValue = (int)value;
            textComponent.text = values[intValue];
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
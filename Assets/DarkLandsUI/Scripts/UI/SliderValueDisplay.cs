using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    public class SliderValueDisplay : MonoBehaviour
    {
        private const string DisplayFormat = "{0:0}/{1:0}";
        public Slider slider;
        public TMP_Text textComponent;

        private void Start()
        {
            if (slider == null || textComponent == null) return;
            
            slider.onValueChanged.AddListener(OnValueChange);
            OnValueChange(slider.value);
        }

        private void OnValueChange(float arg0)
        {
            textComponent.text = string.Format(DisplayFormat, arg0, slider.maxValue);
        }

        private void OnDestroy()
        {
            if (slider)
            {
                slider.onValueChanged.RemoveListener(OnValueChange);
            }
        }
    }
}
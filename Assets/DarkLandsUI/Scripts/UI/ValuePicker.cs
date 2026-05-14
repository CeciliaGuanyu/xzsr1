using TMPro;
using UnityEngine;

namespace DarkLandsUI.Scripts.UI
{
    public class ValuePicker : MonoBehaviour
    {
        public string[] values;
        public int valueIndex;
        public TMP_Text textComponentToDisplay;
        [HideInInspector]
        public string value;

        public void NextValue()
        {
            UpdateValue(valueIndex + 1);
        }

        public void PreviousValue()
        {
            UpdateValue(valueIndex - 1);
        }

        private void Start()
        {
            enabled = values.Length != 0;
            UpdateValue(valueIndex);
        }

        private void UpdateValue(int newValue)
        {
            if (!enabled) return;
            valueIndex = (newValue + values.Length) % values.Length;
            value = values[valueIndex];
            textComponentToDisplay.text = value;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace DarkLandsUI.Scripts.UI
{
    [ExecuteInEditMode]
    public class ColbMaterialControllerAlternative : MonoBehaviour
    {
        private static readonly int FillLevelPropertyId = Shader.PropertyToID("_FillLevel1");
        public Slider slider;
        public Image image;

        private void Awake()
        {
            if (!slider || !image || image.material == null) return;
            slider.onValueChanged.AddListener(value => UpdateValue());
        }

        private void UpdateValue()
        {
            image.materialForRendering.SetFloat(FillLevelPropertyId, slider.normalizedValue);
        }
    }
}
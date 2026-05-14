using UnityEngine;
using UnityEngine.UI;

public class ClueSlot : MonoBehaviour
{
    public ClueData myData; // 自动关联的数据会存到这里
    private Image iconImage;

    public void SetupSlot(ClueData data)
    {
        myData = data;

        // 自动获取子物体中名为 "Icon" 的图片组件并显示
        iconImage = transform.Find("Icon").GetComponent<Image>();
        //iconImage.rectTransform.anchoredPosition = Vector2.zero; // 强制归零到父物体中心
        //iconImage.rectTransform.localScale = Vector3.one;       // 确保缩放是1
        if (iconImage != null && myData.icon != null)
        {
            iconImage.sprite = myData.icon;
        }

        // 自动绑定点击事件：点击就弹窗
        GetComponent<Button>().onClick.AddListener(() => {
            if (myData != null)
            {
                ClueUIController.Instance.ShowClue(myData);
            }
        });
    }
}
using UnityEngine;

public class ClueItem : MonoBehaviour
{
    [Header("线索配置信息")]
    [Tooltip("本子中显示的物品名称")]
    public string clueName;

    [Tooltip("本子中显示的物品图标")]
    public Sprite clueSprite;

    [TextArea(3, 10)]
    [Tooltip("本子中显示的详细解谜描述文本")]
    public string clueDescription;
}
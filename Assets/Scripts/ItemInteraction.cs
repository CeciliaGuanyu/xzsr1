using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    public ClueData associatedClue; // 1. 直接在此处拖入对应的 ScriptableObject

    void Update()
    {
        //// 伪代码：检测靠近且按下 F
        //if (PlayerInRange() && Input.GetKeyDown(KeyCode.F))
        //{
        //    // 调用管理器的触发函数
        //    InventoryManager.Instance.TriggerClue(associatedClue.clueID);

        //    // 触发后通常销毁或禁用场景中的物体
        //    gameObject.SetActive(false);
        //}
    }
}
using UnityEngine;
using static UnityEngine.Rendering.STP;

public class BagManager : MonoBehaviour
{
    public static BagManager Instance;

    public GameObject slotPrefab;      // 你的 Button 预制体
    public Transform inventoryContent; // 挂了 Grid Layout Group 的父物体

    void Awake() => Instance = this;

    // 当物体被触发时，调用这个函数
    public void AddClueToInventory(ClueData data)
    {
        // 1. 生成一个新的槽位按钮
        GameObject newSlot = Instantiate(slotPrefab, inventoryContent);

        // 2. 将数据“推”给这个槽位
        ClueSlot slotScript = newSlot.GetComponent<ClueSlot>();
        if (slotScript != null)
        {
            slotScript.SetupSlot(data);
        }
        Debug.Log($"[测试成功] 按钮触发： 已并入库。");
    }
}
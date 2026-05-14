using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    [Header("这件物品对应的线索数据")]
    public ClueData config;

    // --- 这个函数专门给 UI Button 的 On Click() 列表调用 ---
    public void TriggerThisItem()
    {
        // 1. 安全检查：确保单例和数据都存在
        if (config == null)
        {
            Debug.LogError("未给 ClueTrigger 挂载 ClueData！");
            return;
        }

        if (ClueUIController.Instance == null )
        {
            Debug.LogError("场景中缺少 UI 控制器");
            return;
        }
        //if (BagManager.Instance == null)
        //{
        //    Debug.LogError("场景中缺少 背包管理器！");
        //    return;
        //}

        // 2. 执行展示逻辑（大面板）
        ClueUIController.Instance.ShowClue(config);

        // 3. 执行背包逻辑（存入格子）
        BagManager.Instance.AddClueToInventory(config);

        // 4. 反馈：隐藏这个测试按钮，防止连点导致背包刷屏
        // 如果你想反复测试，可以把下面这行注释掉
      //  this.gameObject.SetActive(false);

        Debug.Log($"[测试成功] 按钮触发：{config.clueName} 已展示并入库。");
    }
}
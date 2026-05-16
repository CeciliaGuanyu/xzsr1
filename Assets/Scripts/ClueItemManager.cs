using UnityEngine;
using System.Collections.Generic;

public class ClueItemManager : MonoBehaviour
{
    public static ClueItemManager Instance;

    // 存储游戏中所有的线索数据
    public List<ClueData> allClues;

    void Awake() => Instance = this;

    // 核心功能：触发线索（由交互脚本调用）
    public void TriggerClue(string id)
    {
        ClueData clue = allClues.Find(c => c.clueID == id);

        if (clue != null && !clue.isTriggered)
        {
            clue.isTriggered = true; // 3. 标记为已触发

            //// 自动分发信息：
            //// A. 弹出大图 UI
            ClueUIController.Instance.ShowClue(clue);

            //// B. 自动添加到背包快捷栏
            //HotbarController.Instance.AddIcon(clue);

            //// C. 激活笔记本中的对应页码逻辑
            //NotebookManager.Instance.UnlockPage(clue.notebookPageIndex);

            //Debug.Log($"线索 {clue.clueName} 已自动同步到所有系统");
        }
    }
}
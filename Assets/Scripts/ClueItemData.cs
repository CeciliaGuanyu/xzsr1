using UnityEngine;

[CreateAssetMenu(fileName = "Clue Items", menuName = "AdventureGame/Clue Item Data")]
public class ClueData : ScriptableObject
{
    [Header("基础信息")]
    public string clueID;          // 唯一ID，用于程序识别
    public string clueName;        // 物品名称
    [TextArea]
    public string description;    // 详细信息/背景故事

    [Header("美术资源")]
    public Sprite icon;           // 背包里的小图标
    public Sprite portrait;       // 屏幕弹出的精美立绘

    [Header("关联配置")]
    public GameObject scenePrefab; // 场景中对应的模型预制体
    public int notebookPageIndex; // 对应 EndlessBook 的页码

    [Header("状态追踪")]
    public bool isTriggered = false; // 是否已被玩家发现/触发
}
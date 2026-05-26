using System;
using UnityEngine;

[Serializable]
public class DialogueLineData
{
    [TextArea(2, 6)]
    public string text;

    [Tooltip("说话者名字（可选，显示在对话框上方）")]
    public string speakerName;

    [Tooltip("人物立绘/头像")]
    public Sprite portrait;

    [Header("播放（可选，覆盖本段触发器设置）")]
    public bool overridePlayback;

    public DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Manual;

    [Tooltip("自动模式：本句完全显示后等待秒数；-1 表示用全局默认值")]
    public float autoDelay = -1f;
}

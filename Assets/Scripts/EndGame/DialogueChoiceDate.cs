using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoiceData
{
    [Tooltip("选择按钮显示的文本")]
    public string choiceText;

    [Tooltip("选择后播放的分支对话")]
    public List<DialogueLineData> branchDialogue = new();

    [Tooltip("选择后执行的显示/隐藏效果")]
    public DialogueRevealAction choiceRevealAction = new();

    [Tooltip("选择后是否覆盖全局播放模式")]
    public bool overridePlaybackMode;
    public DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Manual;

    [Tooltip("自动模式下的行延迟（仅覆盖时生效）")]
    public float autoLineDelay = 1.5f;
}
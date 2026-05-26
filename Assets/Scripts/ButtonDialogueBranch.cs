using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 在 Inspector 的 Branches 列表里添加任意多条：每条拖入一个 Button 并配置对应对话。
/// 不修改现有脚本；场景内需已有 DialogueManager。
/// </summary>
public class ButtonDialogueBranch : MonoBehaviour
{
    [Serializable]
    public class Branch
    {
        [Tooltip("仅用于识别，例如 Button A")]
        public string branchName = "Button";

        [Tooltip("拖入 UI Button")]
        public Button button;

        [Tooltip("点击该按钮后播放的对话")]
        public List<DialogueLineData> dialogueLines = new();
    }
    [Header("独立控制")]
    public bool isBranchDialogueActive = false;/// <summary>///////////////
    /// ////////
    /// </summary>
    [Header("按钮分支（数量不限）")]
    [Tooltip("点 + 增加条目，拖入 Button 并填写对话；有几条就支持几个按钮")]
    public List<Branch> branches = new();

    [Header("播放设置（可选）")]
    public bool overridePlaybackMode;
    public DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Manual;

    public bool overrideAutoLineDelay;
    public float autoLineDelay = 1.5f;

    [Header("行为")]
    [Tooltip("已有对话在播放时，忽略新的按钮点击")]
    public bool ignoreClicksWhilePlaying = true;

    [Tooltip("选中某条分支后禁用全部按钮（防止连点）")]
    public bool disableAllButtonsAfterPick;

    UnityAction[] clickHandlers;

    void OnEnable()
    {
        RegisterButtonListeners();
    }

    void OnDisable()
    {
        UnregisterButtonListeners();
    }

    void RegisterButtonListeners()
    {
        UnregisterButtonListeners();

        if (branches == null || branches.Count == 0) return;

        clickHandlers = new UnityAction[branches.Count];
        for (int i = 0; i < branches.Count; i++)
        {
            int index = i;
            var branch = branches[i];
            if (branch == null || branch.button == null) continue;

            clickHandlers[i] = () => PlayBranch(index);
            branch.button.onClick.AddListener(clickHandlers[i]);
        }
    }

    void UnregisterButtonListeners()
    {
        if (branches == null || clickHandlers == null) return;

        for (int i = 0; i < branches.Count && i < clickHandlers.Length; i++)
        {
            var branch = branches[i];
            if (branch?.button == null || clickHandlers[i] == null) continue;
            branch.button.onClick.RemoveListener(clickHandlers[i]);
        }

        clickHandlers = null;
    }

    public void PlayBranch(int index)
    {
        if (branches == null || index < 0 || index >= branches.Count)
            return;

        var branch = branches[index];
        if (branch == null)
            return;

        var mgr = DialogueManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[ButtonDialogueBranch] 场景缺少 DialogueManager。请先执行 Tools → Setup Dialogue System。");
            return;
        }

        if (ignoreClicksWhilePlaying && mgr.IsPlaying)
        {
            Debug.Log("[ButtonDialogueBranch] 已有对话播放中，忽略点击。");
            return;
        }

        if (branch.dialogueLines == null || branch.dialogueLines.Count == 0)
        {
            Debug.LogWarning($"[ButtonDialogueBranch] {branch.branchName} 未配置对话内容。");
            return;
        }
        HideButtons();/////////
        bool hasText = false;
        foreach (var line in branch.dialogueLines)
        {
            if (line != null && !string.IsNullOrWhiteSpace(line.text))
            {
                hasText = true;
                break;
            }
        }
        
        if (!hasText)
        {
            Debug.LogWarning($"[ButtonDialogueBranch] {branch.branchName} 对话列表里没有有效文字。");
            return;
        }

        DialoguePlaybackMode? mode = overridePlaybackMode ? playbackMode : (DialoguePlaybackMode?)null;
        float? delay = overrideAutoLineDelay ? autoLineDelay : (float?)null;
       // FindObjectOfType<ShowButton>().Hide();//////////////
        mgr.PlayLines(branch.dialogueLines, null, mode, delay);
        FindObjectOfType<ShowButton>().Show();
        if (disableAllButtonsAfterPick)
            SetAllButtonsInteractable(false);
    }
    // 显示分支按钮（只有这个对话能调用）//////////////////////////
    public void ShowBranchButtons()
    {
        isBranchDialogueActive = true;
        ShowButtons();
    }

    // 隐藏分支按钮
    public void HideBranchButtons()
    {
        isBranchDialogueActive = false;
        HideButtons();
    }

    // 你已有的 ShowButtons、HideButtons 保留不变
    public void ShowButtons() {
        foreach (var branch in branches)
        {
            if (branch.button != null)
                branch.button.gameObject.SetActive(true);
        }
    }
    public void HideButtons()
    {
        foreach (var branch in branches)
        {
            if (branch.button != null)
                branch.button.gameObject.SetActive(false);
        }
    }
    public void SetAllButtonsInteractable(bool interactable)
    {
        if (branches == null) return;

        foreach (var branch in branches)
        {
            if (branch?.button != null)
                branch.button.interactable = interactable;
        }
    }

    /// <summary>重新启用全部分支按钮（例如新章节开始时调用）。</summary>
    public void ResetButtons()
    {
        SetAllButtonsInteractable(true);
    }
}

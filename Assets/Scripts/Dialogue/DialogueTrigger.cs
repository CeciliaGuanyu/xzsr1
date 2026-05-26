using System.Collections.Generic;
using UnityEngine;

public enum DialogueTriggerType
{
    [Tooltip("场景开始后自动触发")]
    OnGameStart,

    [Tooltip("玩家进入触发器（需 Collider IsTrigger + 玩家 Tag=Player）")]
    OnPlayerEnter,

    [Tooltip("检视指定物品时触发（时机见 Inspect Moment）")]
    OnInspectTarget,

    [Tooltip("检视多个物品全部满足条件后触发（可只显示物体、不播对话）")]
    OnInspectAllRequired,

    [Tooltip("物体 A 靠近物体 B 到一定距离时触发")]
    OnObjectsNear,
}

public enum InspectTriggerMoment
{
    [Tooltip("打开检视面板、开始查看时")]
    OnInspectStart,

    [Tooltip("关闭检视面板、查看结束后")]
    OnInspectEnd,
}

public enum DialogueRevealTiming
{
    [Tooltip("对话全部播完后")]
    OnDialogueComplete,

    [Tooltip("满足检视条件时立即执行（对话可另说）")]
    OnConditionMet,

    [Tooltip("触发对话的同时")]
    OnTriggerStart,
}

/// <summary>
/// 挂到场景物体上配置多种触发方式。场景中需有 DialogueManager。
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("触发方式")]
    public DialogueTriggerType triggerType = DialogueTriggerType.OnGameStart;

    [Tooltip("仅触发一次")]
    public bool playOnce = true;

    [Tooltip("开始后延迟秒数（OnGameStart）")]
    public float startDelay = 0.5f;

    [Header("碰撞触发")]
    [Tooltip("玩家 Tag，默认 Player")]
    public string playerTag = "Player";

    [Header("物体靠近触发")]
    [Tooltip("靠近的一方（例如玩家、NPC、可移动道具）")]
    public Transform proximityObjectA;

    [Tooltip("被靠近的目标（例如门、祭坛、另一件物品）")]
    public Transform proximityObjectB;

    [Tooltip("两者距离小于等于此值时触发")]
    public float proximityDistance = 2f;

    [Tooltip("只计算水平距离（忽略高度差），室内场景建议勾选")]
    public bool useHorizontalDistance = true;

    [Tooltip("检测间隔（秒），避免每帧检测")]
    public float proximityCheckInterval = 0.15f;

    [Tooltip("勾选后在 Console 输出距离与触发信息")]
    public bool debugProximity;

    [Header("检视条件")]
    [Tooltip("OnInspectTarget / OnInspectAllRequired 的触发时机")]
    public InspectTriggerMoment inspectMoment = InspectTriggerMoment.OnInspectEnd;

    [Tooltip("OnInspectTarget：指定要检视的物体")]
    public InteractableObject inspectTarget;

    [Tooltip("OnInspectAllRequired：必须全部检视过的物体")]
    public InteractableObject[] requiredInspectTargets;

    [Tooltip("也可用字符串 ID 匹配（与 InteractableObject.inspectId 对应）")]
    public string[] requiredInspectIds;

    [Header("对话内容")]
    public List<DialogueLineData> dialogueLines = new();

    [Header("播放模式")]
    [Tooltip("勾选后覆盖 DialogueManager 上的默认播放模式")]
    public bool overridePlaybackMode;

    public DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Manual;

    [Tooltip("覆盖自动模式时每句之间的等待时间")]
    public bool overrideAutoLineDelay;

    public float autoLineDelay = 1.5f;

    [Tooltip("满足条件但不配置对话时，仍可只执行显示/隐藏物体")]
    public bool allowRevealWithoutDialogue = true;

    [Header("检视后显示物体（可选）")]
    [Tooltip("不勾选 = 只播对话，不显示/隐藏任何物体")]
    public bool enableReveal;

    public DialogueRevealAction revealAction = new();

    [Tooltip("检视完成→对话→本选项=对话播完后显示/隐藏物体")]
    public DialogueRevealTiming revealTiming = DialogueRevealTiming.OnDialogueComplete;

    [Header("调试")]
    public string triggerLabel;

    bool hasTriggered;
    bool wasInProximity;
    float nextProximityCheckTime;

    void OnEnable()
    {
        DialogueManager.OnInspectStarted += HandleInspectStarted;
        DialogueManager.OnInspectCompleted += HandleInspectCompleted;
    }

    void OnDisable()
    {
        DialogueManager.OnInspectStarted -= HandleInspectStarted;
        DialogueManager.OnInspectCompleted -= HandleInspectCompleted;
    }

    void Start()
    {
        if (triggerType == DialogueTriggerType.OnGameStart)
        {
            if (startDelay > 0f)
                Invoke(nameof(FireTrigger), startDelay);
            else
                FireTrigger();
        }

        if (triggerType == DialogueTriggerType.OnInspectAllRequired &&
            inspectMoment == InspectTriggerMoment.OnInspectEnd)
        {
            TryFireInspectAllCondition();
        }
    }

    void Update()
    {
        if (triggerType != DialogueTriggerType.OnObjectsNear) return;
        if (playOnce && hasTriggered) return;
        if (Time.unscaledTime < nextProximityCheckTime) return;

        nextProximityCheckTime = Time.unscaledTime + Mathf.Max(0.05f, proximityCheckInterval);
        CheckObjectsProximity();
    }

    void CheckObjectsProximity()
    {
        if (proximityObjectA == null || proximityObjectB == null)
        {
            if (debugProximity)
                Debug.LogWarning($"[DialogueTrigger] {name} 未设置 Proximity Object A 或 B");
            return;
        }

        float threshold = Mathf.Max(0.01f, proximityDistance);
        float dist = GetProximityDistance(proximityObjectA.position, proximityObjectB.position);
        bool inRange = dist <= threshold;

        if (debugProximity)
            Debug.Log($"[DialogueTrigger] {name} 距离={dist:F2} / 阈值={threshold:F2} 范围内={inRange}");

        if (!inRange)
        {
            wasInProximity = false;
            return;
        }

        if (wasInProximity)
            return;

        if (FireTrigger())
            wasInProximity = true;
    }

    float GetProximityDistance(Vector3 a, Vector3 b)
    {
        if (!useHorizontalDistance)
            return Vector3.Distance(a, b);

        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerType != DialogueTriggerType.OnPlayerEnter) return;
        if (!other.CompareTag(playerTag)) return;
        FireTrigger();
    }

    void HandleInspectStarted(InteractableObject inspected)
    {
        if (inspectMoment != InspectTriggerMoment.OnInspectStart) return;
        if (playOnce && hasTriggered) return;

        switch (triggerType)
        {
            case DialogueTriggerType.OnInspectTarget:
                TryFireSingleInspect(inspected);
                break;

            case DialogueTriggerType.OnInspectAllRequired:
                TryFireInspectAllOnStart(inspected);
                break;
        }
    }

    void HandleInspectCompleted(InteractableObject inspected)
    {
        if (inspectMoment != InspectTriggerMoment.OnInspectEnd) return;
        if (playOnce && hasTriggered) return;

        switch (triggerType)
        {
            case DialogueTriggerType.OnInspectTarget:
                TryFireSingleInspect(inspected);
                break;

            case DialogueTriggerType.OnInspectAllRequired:
                TryFireInspectAllCondition();
                break;
        }
    }

    void TryFireSingleInspect(InteractableObject inspected)
    {
        if (inspectTarget == null)
        {
            if (inspected != null)
                Debug.LogWarning($"[DialogueTrigger] {name} 未指定 inspectTarget");
            return;
        }

        if (inspected == inspectTarget)
            FireTrigger();
    }

    void TryFireInspectAllOnStart(InteractableObject justStarted)
    {
        if (triggerType != DialogueTriggerType.OnInspectAllRequired) return;
        if (playOnce && hasTriggered) return;
        if (justStarted == null) return;

        var mgr = DialogueManager.Instance;
        if (mgr == null) return;

        if (!IsPartOfRequirement(justStarted))
            return;

        if (!AreAllOthersInspected(mgr, justStarted))
            return;

        FireInspectAllResult(mgr);
    }

    void TryFireInspectAllCondition()
    {
        if (triggerType != DialogueTriggerType.OnInspectAllRequired) return;
        if (hasTriggered && playOnce) return;

        var mgr = DialogueManager.Instance;
        if (mgr == null) return;

        if (!IsAllRequiredInspected(mgr))
            return;

        FireInspectAllResult(mgr);
    }

    void FireInspectAllResult(DialogueManager mgr)
    {
        if (revealTiming == DialogueRevealTiming.OnConditionMet)
            TryApplyReveal(mgr);

        if (dialogueLines.Count > 0 || !allowRevealWithoutDialogue)
            FireTrigger();
        else if (!hasTriggered)
            CompleteWithoutDialogue();
    }

    bool IsPartOfRequirement(InteractableObject obj)
    {
        if (requiredInspectTargets != null)
        {
            foreach (var t in requiredInspectTargets)
            {
                if (t == obj) return true;
            }
        }

        if (requiredInspectIds != null)
        {
            foreach (var id in requiredInspectIds)
            {
                if (!string.IsNullOrEmpty(id) && id == obj.InspectId)
                    return true;
            }
        }

        return false;
    }

    bool AreAllOthersInspected(DialogueManager mgr, InteractableObject exclude)
    {
        if (requiredInspectTargets != null)
        {
            foreach (var t in requiredInspectTargets)
            {
                if (t == null || t == exclude) continue;
                if (!mgr.HasInspected(t))
                    return false;
            }
        }

        if (requiredInspectIds != null)
        {
            foreach (var id in requiredInspectIds)
            {
                if (string.IsNullOrEmpty(id) || id == exclude.InspectId) continue;
                if (!mgr.HasInspected(id))
                    return false;
            }
        }

        return HasAnyInspectRequirement();
    }

    bool IsAllRequiredInspected(DialogueManager mgr)
    {
        if (requiredInspectTargets != null)
        {
            foreach (var t in requiredInspectTargets)
            {
                if (t == null) continue;
                if (!mgr.HasInspected(t))
                    return false;
            }
        }

        if (requiredInspectIds != null)
        {
            foreach (var id in requiredInspectIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (!mgr.HasInspected(id))
                    return false;
            }
        }

        return HasAnyInspectRequirement();
    }

    bool HasAnyInspectRequirement()
    {
        return (requiredInspectTargets != null && requiredInspectTargets.Length > 0) ||
               (requiredInspectIds != null && requiredInspectIds.Length > 0);
    }

    void TryApplyReveal(DialogueManager mgr)
    {
        if (!enableReveal || mgr == null) return;
        if (!revealAction.HasContent()) return;
        mgr.ApplyReveal(revealAction);
    }

    /// <summary>供 UnityEvent 或外部脚本手动调用。返回是否成功开始对话。</summary>
    public bool FireTrigger()
    {
        if (playOnce && hasTriggered)
        {
            if (debugProximity)
                Debug.Log($"[DialogueTrigger] {name} 已触发过（Play Once）");
            return false;
        }

        var mgr = DialogueManager.Instance;
        if (mgr == null)
        {
            Debug.LogError($"[DialogueTrigger] 场景缺少 DialogueManager！请执行 Tools → Setup Dialogue System。触发器：{triggerLabel ?? name}");
            return false;
        }

        if (mgr.IsPlaying)
        {
            if (debugProximity)
                Debug.Log($"[DialogueTrigger] {name} 已有对话在播放，稍后再试");
            return false;
        }

        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            if (allowRevealWithoutDialogue)
            {
                if (playOnce) hasTriggered = true;
                CompleteWithoutDialogue();
                return true;
            }

            Debug.LogWarning($"[DialogueTrigger] {name} 没有配置对话内容。");
            return false;
        }

        bool hasValidLine = false;
        foreach (var line in dialogueLines)
        {
            if (line != null && !string.IsNullOrWhiteSpace(line.text))
            {
                hasValidLine = true;
                break;
            }
        }

        if (!hasValidLine)
        {
            Debug.LogWarning($"[DialogueTrigger] {name} 对话列表里没有有效文字。");
            return false;
        }

        if (revealTiming == DialogueRevealTiming.OnTriggerStart)
            TryApplyReveal(mgr);

        DialoguePlaybackMode? mode = overridePlaybackMode ? playbackMode : null;
        float? delay = overrideAutoLineDelay ? autoLineDelay : null;
        mgr.PlayLines(dialogueLines, OnDialogueFinished, mode, delay);

        if (playOnce)
            hasTriggered = true;

        if (debugProximity)
            Debug.Log($"[DialogueTrigger] {name} 对话已触发");

        return true;
    }

    void OnDialogueFinished()
    {
        if (revealTiming == DialogueRevealTiming.OnDialogueComplete)
            TryApplyReveal(DialogueManager.Instance);
        var branchComponent = GetComponent<ButtonDialogueBranch>();
        Debug.Log("开始检查分支");
        if (branchComponent != null)
        {
            Debug.Log("检查到分支");
            branchComponent.ShowButtons(); // 唤醒面板
        }

        if (playOnce)
            hasTriggered = true;
    }

    void CompleteWithoutDialogue()
    {
        if (revealTiming == DialogueRevealTiming.OnDialogueComplete ||
            revealTiming == DialogueRevealTiming.OnTriggerStart)
        {
            TryApplyReveal(DialogueManager.Instance);
        }

        if (playOnce)
            hasTriggered = true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (triggerType == DialogueTriggerType.OnObjectsNear &&
            proximityObjectA != null && proximityObjectB != null)
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawLine(proximityObjectA.position, proximityObjectB.position);
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(proximityObjectB.position, proximityDistance);
        }

        if (triggerType != DialogueTriggerType.OnPlayerEnter) return;
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
    }
#endif
}

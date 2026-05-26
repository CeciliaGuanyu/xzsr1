using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 挂到场景任意物体上即可。若未指定 UI，运行时会自动创建对话面板。
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI（留空则自动创建）")]
    public GameObject panelRoot;

    [Tooltip("对话 Canvas 根节点；留空则自动从 Panel Root 的父 Canvas 查找")]
    public GameObject dialogueCanvasRoot;

    public Image portraitImage;
    public TMP_Text speakerText;
    public TMP_Text bodyText;
    public TMP_Text hintText;
    public Button advanceButton;

    [Header("行为")]
    public bool pauseGameWhileDialogue = true;
    public KeyCode advanceKey = KeyCode.Space;

    [Header("打字机效果")]
    public bool useTypewriter = true;
    [Tooltip("每秒显示多少个字符（使用 unscaled 时间，暂停时也能播放）")]
    public float charsPerSecond = 40f;

    [Header("播放模式（默认）")]
    public DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Manual;
    [Tooltip("自动模式：每句完全显示后，等待多久切下一句")]
    public float autoLineDelay = 1.5f;

    public bool IsPlaying { get; private set; }

    public static event Action<InteractableObject> OnInspectStarted;
    public static event Action<InteractableObject> OnInspectCompleted;

    readonly HashSet<string> inspectedIds = new();

    Queue<DialogueLineData> lineQueue;
    Action onSequenceComplete;
    FirstPersonController playerController;

    Coroutine typewriterRoutine;
    Coroutine autoAdvanceRoutine;
    bool isRevealingText;
    DialoguePlaybackMode sessionPlaybackMode;
    float sessionAutoLineDelay;
    DialogueLineData currentLine;

    const string HintNextManual = "点击 / 空格 下一句";
    const string HintTyping = "";
    const string HintAutoWait = "自动播放中…";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureUI();
        ResolveDialogueCanvasRoot();
        HidePanel();
    }

    void Start()
    {
        playerController = FindObjectOfType<FirstPersonController>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!IsPlaying || !CanAcceptManualAdvance()) return;

        // 空格：推进对话
        if (Input.GetKeyDown(advanceKey))
        {
            Advance();
            return;
        }

        // 鼠标：仅在松开时且点在对话 UI 上，避免与「左键检视」冲突
        if (Input.GetMouseButtonUp(0) && IsPointerOverDialogueUI())
            Advance();
    }

    bool IsPointerOverDialogueUI()
    {
        if (EventSystem.current == null)
            return true;

        return EventSystem.current.IsPointerOverGameObject();
    }

    public static void NotifyInspectStarted(InteractableObject target)
    {
        if (Instance == null || target == null) return;
        OnInspectStarted?.Invoke(target);
    }

    public static void NotifyInspectCompleted(InteractableObject target)
    {
        if (Instance == null || target == null) return;
        Instance.RegisterInspect(target);
        OnInspectCompleted?.Invoke(target);
    }

    public void RegisterInspect(InteractableObject target)
    {
        if (target == null) return;
        inspectedIds.Add(target.InspectId);
    }

    public bool HasInspected(string inspectId)
    {
        return !string.IsNullOrEmpty(inspectId) && inspectedIds.Contains(inspectId);
    }

    public bool HasInspectedAll(IEnumerable<string> ids)
    {
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (!inspectedIds.Contains(id))
                return false;
        }
        return true;
    }

    public bool HasInspected(InteractableObject obj)
    {
        return obj != null && HasInspected(obj.InspectId);
    }

    public void PlayLines(
        IReadOnlyList<DialogueLineData> lines,
        Action onComplete = null,
        DialoguePlaybackMode? modeOverride = null,
        float? autoDelayOverride = null)
    {
        if (lines == null || lines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (IsPlaying)
        {
            Debug.LogWarning("[Dialogue] 已有对话进行中，新请求被忽略。");
            return;
        }

        lineQueue = new Queue<DialogueLineData>();
        foreach (var line in lines)
        {
            if (line != null && !string.IsNullOrWhiteSpace(line.text))
                lineQueue.Enqueue(line);
        }

        if (lineQueue.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        sessionPlaybackMode = modeOverride ?? playbackMode;
        sessionAutoLineDelay = autoDelayOverride ?? autoLineDelay;

        onSequenceComplete = onComplete;
        IsPlaying = true;

        if (pauseGameWhileDialogue)
            Time.timeScale = 0f;

        SetPlayerControl(false);

        if (playerController != null)
            playerController.SuppressInteraction(0.25f);

        ShowPanel();
        DisplayCurrentLine();
    }

    public void ApplyReveal(DialogueRevealAction action)
    {
        if (action == null) return;

        if (action.showObjects != null)
        {
            foreach (var go in action.showObjects)
            {
                if (go != null) go.SetActive(true);
            }
        }

        if (action.hideObjects != null)
        {
            foreach (var go in action.hideObjects)
            {
                if (go != null) go.SetActive(false);
            }
        }
    }

    bool CanAcceptManualAdvance()
    {
        return !isRevealingText
               && autoAdvanceRoutine == null
               && GetLinePlaybackMode() == DialoguePlaybackMode.Manual;
    }

    void Advance(bool fromAuto = false)
    {
        if (!IsPlaying) return;

        if (!fromAuto)
        {
            if (!CanAcceptManualAdvance()) return;
            StopAutoAdvance();
        }
        else
        {
            StopAutoAdvance();
        }

        if (lineQueue.Count > 0)
        {
            DisplayCurrentLine();
            return;
        }

        EndDialogue();
    }

    void DisplayCurrentLine()
    {
        if (lineQueue.Count == 0) return;

        StopTypewriter();
        StopAutoAdvance();

        var line = lineQueue.Dequeue();
        currentLine = line;

        if (speakerText != null)
        {
            bool hasName = !string.IsNullOrWhiteSpace(line.speakerName);
            speakerText.gameObject.SetActive(hasName);
            speakerText.text = hasName ? line.speakerName : string.Empty;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
            portraitImage.color = line.portrait != null ? Color.white : new Color(1, 1, 1, 0.2f);
        }

        if (bodyText != null)
        {
            bodyText.text = line.text;
            bodyText.ForceMeshUpdate();

            if (useTypewriter && !string.IsNullOrEmpty(line.text))
            {
                SetHint(HintTyping);
                RefreshAdvanceButton();
                typewriterRoutine = StartCoroutine(TypewriterRoutine());
            }
            else
            {
                bodyText.maxVisibleCharacters = int.MaxValue;
                isRevealingText = false;
                OnLineFullyDisplayed();
            }
        }
    }

    IEnumerator TypewriterRoutine()
    {
        isRevealingText = true;
        RefreshAdvanceButton();

        bodyText.maxVisibleCharacters = 0;
        int totalChars = bodyText.textInfo.characterCount;
        float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0.02f;

        for (int i = 1; i <= totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(delay);
        }

        isRevealingText = false;
        typewriterRoutine = null;
        OnLineFullyDisplayed();
    }

    void OnLineFullyDisplayed()
    {
        if (GetLinePlaybackMode() == DialoguePlaybackMode.Auto)
        {
            SetHint(HintAutoWait);
            RefreshAdvanceButton();
            ScheduleAutoAdvance(GetLineAutoDelay());
        }
        else
        {
            SetHint(HintNextManual);
            RefreshAdvanceButton();
        }
    }

    DialoguePlaybackMode GetLinePlaybackMode()
    {
        if (currentLine != null && currentLine.overridePlayback)
            return currentLine.playbackMode;
        return sessionPlaybackMode;
    }

    float GetLineAutoDelay()
    {
        if (currentLine != null && currentLine.overridePlayback && currentLine.autoDelay >= 0f)
            return currentLine.autoDelay;
        return sessionAutoLineDelay;
    }

    void ScheduleAutoAdvance(float delay)
    {
        StopAutoAdvance();
        autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine(Mathf.Max(0f, delay)));
    }

    IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        autoAdvanceRoutine = null;
        Advance(fromAuto: true);
    }

    void RefreshAdvanceButton()
    {
        if (advanceButton != null)
            advanceButton.interactable = CanAcceptManualAdvance();
    }

    void StopAutoAdvance()
    {
        if (autoAdvanceRoutine != null)
        {
            StopCoroutine(autoAdvanceRoutine);
            autoAdvanceRoutine = null;
        }
    }

    void StopTypewriter()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        isRevealingText = false;
    }

    void SetHint(string text)
    {
        if (hintText != null)
            hintText.text = text;
    }

    void EndDialogue()
    {
        StopTypewriter();
        StopAutoAdvance();
        currentLine = null;
        IsPlaying = false;
        //FindObjectOfType<ShowButton>().Show();////////////////
       ButtonDialogueBranch branch = FindFirstObjectByType<ButtonDialogueBranch>();
        if (branch != null && branch.isBranchDialogueActive)
        {
            FindObjectOfType<ShowButton>().Show();
        }

        HidePanel();


        if (pauseGameWhileDialogue)
            Time.timeScale = 1f;

        if (playerController != null)
            playerController.SuppressInteraction(0.35f);
                                                        
        SetPlayerControl(true);
        StartCoroutine(HideCursorAfterDialogueEnd());

        var cb = onSequenceComplete;
        onSequenceComplete = null;
        cb?.Invoke();
    }

    IEnumerator HideCursorAfterDialogueEnd()
    {
        yield return GameplayCursor.HideAfterFrames(this, 2);

        if (IsPlaying) yield break;

        HidePanel();

        if (playerController != null)
            playerController.SetRoamingEnabled(true);
        else
            GameplayCursor.HideForGameplay();
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();

        if (playerController != null)
            playerController.SetRoamingEnabled(enabled);

        if (enabled)
            GameplayCursor.HideForGameplay();
        else
            GameplayCursor.ShowForUI();
    }

    void ResolveDialogueCanvasRoot()
    {
        if (dialogueCanvasRoot != null) return;
        if (panelRoot == null) return;

        var canvas = panelRoot.GetComponentInParent<Canvas>(true);
        if (canvas != null)
            dialogueCanvasRoot = canvas.gameObject;
    }

    void ShowPanel()
    {
        if (dialogueCanvasRoot != null)
            dialogueCanvasRoot.SetActive(true);

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (dialogueCanvasRoot != null)
            dialogueCanvasRoot.SetActive(false);
    }

    void EnsureUI()
    {
        if (panelRoot != null && bodyText != null)
        {
            ResolveDialogueCanvasRoot();
            return;
        }

        var canvasGO = new GameObject("DialogueCanvas");
        dialogueCanvasRoot = canvasGO;
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("DialoguePanel");
        panelRoot.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelRoot.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.04f);
        panelRect.anchorMax = new Vector2(0.95f, 0.28f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var bg = panelRoot.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.92f);

        advanceButton = panelRoot.AddComponent<Button>();
        advanceButton.transition = Selectable.Transition.None;

        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(panelRoot.transform, false);
        var portraitRect = portraitGO.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0);
        portraitRect.anchorMax = new Vector2(0, 1);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.sizeDelta = new Vector2(220, -24);
        portraitRect.anchoredPosition = new Vector2(16, 0);
        portraitImage = portraitGO.AddComponent<Image>();
        portraitImage.preserveAspect = true;

        var speakerGO = new GameObject("SpeakerName");
        speakerGO.transform.SetParent(panelRoot.transform, false);
        var speakerRect = speakerGO.AddComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0, 1);
        speakerRect.anchorMax = new Vector2(1, 1);
        speakerRect.pivot = new Vector2(0, 1);
        speakerRect.sizeDelta = new Vector2(-260, 36);
        speakerRect.anchoredPosition = new Vector2(250, -8);
        speakerText = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerText.fontSize = 26;
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.color = new Color(1f, 0.85f, 0.5f);

        var bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(panelRoot.transform, false);
        var bodyRect = bodyGO.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.offsetMin = new Vector2(250, 16);
        bodyRect.offsetMax = new Vector2(-16, -44);
        bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyText.fontSize = 24;
        bodyText.color = Color.white;
        bodyText.enableWordWrapping = true;

        var hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(panelRoot.transform, false);
        var hintRect = hintGO.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 0);
        hintRect.anchorMax = new Vector2(1, 0);
        hintRect.pivot = new Vector2(1, 0);
        hintRect.sizeDelta = new Vector2(200, 28);
        hintRect.anchoredPosition = new Vector2(-12, 8);
        hintText = hintGO.AddComponent<TextMeshProUGUI>();
        hintText.fontSize = 18;
        hintText.alignment = TextAlignmentOptions.BottomRight;
        hintText.color = new Color(0.75f, 0.75f, 0.75f);
        hintText.text = "点击 / 空格 下一句";

        advanceButton.onClick.AddListener(() => Advance());
        HidePanel();
    }
}

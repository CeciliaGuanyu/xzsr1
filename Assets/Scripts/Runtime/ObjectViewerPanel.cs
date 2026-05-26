using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectViewerPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public RawImage previewRawImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button closeButton;
    public Image crosshair;
    public TMP_Text hintText;
    public RectTransform previewArea;

    [Header("Preview Setup")]
    public Camera previewCamera;
    public Light previewLight;
    public Transform previewSpawnPoint;
    public string previewLayerName = "ObjectPreview";
    public float targetPreviewSize = 1.5f;
    public float cameraDistance = 2.5f;

    [Header("Rotation")]
    public float dragSensitivity = 0.5f;

    private int previewLayer;
    private Vector3 previewOrigin;
    private GameObject previewRoot;
    private GameObject previewObject;
    private InteractableObject currentTarget;

    private bool isDragging;
    private Vector2 lastMousePos;
    private float currentYaw;
    private float currentPitch;

    void Awake()
    {
        previewLayer = LayerMask.NameToLayer(previewLayerName);
        if (previewLayer < 0)
            Debug.LogError($"[ObjectViewer-ERR] 图层 Layer '{previewLayerName}' 不存在！请去 Project Settings -> Tags and Layers 中添加它！");
        else
            Debug.Log($"[ObjectViewer-OK] 成功识别预览图层 '{previewLayerName}'，其 Layer ID 为: {previewLayer}");

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (previewSpawnPoint != null)
        {
            previewOrigin = previewSpawnPoint.position;
            Debug.Log($"[ObjectViewer-OK] 成功绑定出生点 SpawnPoint，坐标为: {previewOrigin}");
        }
        else
        {
            Debug.LogError("[ObjectViewer-WARN] previewSpawnPoint 为空(None)！物品将被强制生成在世界原点 (0,0,0)，这可能导致画面穿帮或看不见！");
            previewOrigin = Vector3.zero;
        }

        if (previewCamera != null)
        {
            previewCamera.cullingMask = 1 << previewLayer;
            Debug.Log($"[ObjectViewer-OK] 成功绑定预览相机，且相机的 Culling Mask 已强制锁定为 Layer: {previewLayerName}");
        }
        else
        {
            Debug.LogError("[ObjectViewer-ERR] previewCamera 为空(None)！渲染管道完全断裂！");
        }

        FixMissingSprites();
        SetPanelVisible(false);
        if (hintText != null) hintText.gameObject.SetActive(false);
    }

    void FixMissingSprites()
    {
        var allImages = transform.parent != null
            ? transform.parent.GetComponentsInChildren<Image>(true)
            : GetComponentsInChildren<Image>(true);

        foreach (var img in allImages)
        {
            if (img.sprite == null)
            {
                img.sprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 4, 4),
                    new Vector2(0.5f, 0.5f));
            }
        }
    }

    public void ShowHint(string objectName)
    {
        if (hintText != null)
        {
            hintText.text = "点击查看 " + objectName;
            hintText.gameObject.SetActive(true);
        }
    }

    public void HideHint()
    {
        if (hintText != null)
            hintText.gameObject.SetActive(false);
    }

    // ── 触发弹窗的核心入口 ───────────────────────────────
    public void Show(InteractableObject target)
    {
        Debug.Log($"<color=cyan>====== [开始调用 Show()] 正在准备渲染线索: {target.gameObject.name} ======</color>");

        currentTarget = target;
        DialogueManager.NotifyInspectStarted(target);
        CreatePreview(target.gameObject);
        SetPanelVisible(true);

        if (titleText != null) titleText.text = target.displayName;
        if (descriptionText != null) descriptionText.text = target.description;

        target.onInteracted?.Invoke();
    }

    public void Hide()
    {
        Debug.Log("<color=yellow>[ObjectViewer] 正在关闭面板，销毁克隆体...</color>");

        var finishedTarget = currentTarget;
        currentTarget = null;

        DestroyPreview();
        SetPanelVisible(false);

        if (finishedTarget != null)
            DialogueManager.NotifyInspectCompleted(finishedTarget);

        var controller = FindObjectOfType<FirstPersonController>();
        if (controller != null)
            StartCoroutine(RestorePlayerControlAfterInspect(controller));
    }

    IEnumerator RestorePlayerControlAfterInspect(FirstPersonController controller)
    {
        // 等一帧，让「检视结束」触发的对话有机会开始
        yield return null;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
        {
            // 有对话：等对话结束后再隐藏鼠标（由 DialogueManager 处理）
            controller.SuppressInteraction(0.35f);
            controller.SetRoamingEnabled(false);
            yield break;
        }

        // 无对话：检视结束即可恢复移动并隐藏鼠标
        controller.SuppressInteraction(0.35f);
        controller.SetRoamingEnabled(true);
        yield return GameplayCursor.HideAfterFrames(controller, 2);
    }

    // ── 核心排查：克隆与矩阵计算 ───────────────────────────
    void CreatePreview(GameObject original)
    {
        DestroyPreview();

        Debug.Log($"[1/4 克隆物体] 正在实例化原始模组: {original.name}");
        previewRoot = new GameObject("PreviewRoot");
        previewRoot.transform.position = previewOrigin;

        previewObject = Instantiate(original, previewRoot.transform);
        previewObject.transform.localPosition = Vector3.zero;
        previewObject.transform.localRotation = Quaternion.identity;
        Debug.Log($"[1/4 克隆物体-成功] 克隆体已挂载到 PreviewRoot 下，其世界坐标为: {previewObject.transform.position}");

        // 递归修改 Layer
        Debug.Log($"[2/4 刷图层] 开始将克隆体及其所有子物体的 Layer 变更为: {previewLayerName} (ID: {previewLayer})");
        SetLayerRecursive(previewObject, previewLayer);

        // 禁用无关组件
        DisableComponents(previewObject);

        // 自动缩放
        Debug.Log("[3/4 算缩放] 开始计算物体的 Renderer 包围盒大小...");
        CenterAndScale(previewObject);

        currentYaw = 180f;
        currentPitch = 0f;

        // 移动相机
        Debug.Log("[4/4 调相机] 开始将 PreviewCamera 移动到物品正前方...");
        UpdatePreviewCamera();

        if (previewCamera != null && previewCamera.targetTexture == null)
        {
            Debug.LogError("[渲染断档] PreviewCamera 身上没有挂载 Render Texture！画面无法传输到 UI 屏上！");
        }
        if (previewRawImage != null && previewRawImage.texture == null)
        {
            Debug.LogError("[渲染断档] UI 的 RawImage 身上没有挂载 Render Texture！无法接收画面！");
        }
    }

    void DestroyPreview()
    {
        if (previewRoot != null)
        {
            Destroy(previewRoot);
            previewRoot = null;
            previewObject = null;
        }
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    void DisableComponents(GameObject obj)
    {
        foreach (var comp in obj.GetComponents<MonoBehaviour>())
        {
            if (comp != null && comp != this) comp.enabled = false;
        }

        var col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var audio = obj.GetComponent<AudioSource>();
        if (audio != null) audio.enabled = false;

        var animator = obj.GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        var light = obj.GetComponent<Light>();
        if (light != null) light.enabled = false;

        foreach (Transform child in obj.transform)
            DisableComponents(child.gameObject);
    }

    void CenterAndScale(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError($"[缩放失败] 抓取失败！克隆体 '{obj.name}' 及其子级里找不到任何 Renderer 组件！算法无法计算大小！");
            return;
        }

        Bounds combined = renderers[0].bounds;
        Debug.Log($"[网格检测] 子物体 {renderers[0].name} 的世界包围盒大小为: {renderers[0].bounds.size}");

        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
            Debug.Log($"[网格合并] 合并子物体 {renderers[i].name} 的包围盒，当前总包围盒尺寸变为: {combined.size}");
        }

        float maxDim = Mathf.Max(combined.size.x, combined.size.y, combined.size.z);
        float scale = maxDim > 0.001f ? targetPreviewSize / maxDim : 1f;

        obj.transform.localScale = Vector3.one * scale;
        Debug.Log($"[矩阵计算完毕] 最大边界尺寸为: {maxDim}，TargetPreviewSize设为: {targetPreviewSize} -> 最终计算出缩放 LocalScale 应为: {obj.transform.localScale}");

        Vector3 pivot = obj.transform.position;
        Vector3 newCenter = pivot + Vector3.Scale(combined.center - pivot, obj.transform.localScale);
        obj.transform.localPosition = previewOrigin - newCenter;
        Debug.Log($"[中心对准] 修正轴心偏离，物体最终 LocalPosition 调整为: {obj.transform.localPosition}");
    }

    void UpdatePreviewCamera()
    {
        if (previewCamera == null) return;

        // 让相机退后 cameraDistance 的距离正对物体
        previewCamera.transform.position = previewOrigin + Vector3.back * cameraDistance + Vector3.up * 0.3f;
        previewCamera.transform.LookAt(previewOrigin);

        Debug.Log($"[相机就位] PreviewCamera 移动到了世界坐标: {previewCamera.transform.position}，并正对准出生点: {previewOrigin}");
    }

    void Update()
    {
        if (!panelRoot.activeSelf || previewRoot == null) return;

        HandleDrag();
        previewRoot.transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    void HandleDrag()
    {
        if (previewArea == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(previewArea, Input.mousePosition, null))
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            currentYaw -= delta.x * dragSensitivity;
            currentPitch += delta.y * dragSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, -89f, 89f);
            lastMousePos = Input.mousePosition;
        }
    }

    void SetPanelVisible(bool visible)
    {
        panelRoot.SetActive(visible);
        if (crosshair != null) crosshair.gameObject.SetActive(!visible);
        if (hintText != null && visible) hintText.gameObject.SetActive(false);
    }
}
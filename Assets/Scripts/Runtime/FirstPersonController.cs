using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Interaction")]
    public float interactRange = 3f;
    public LayerMask interactLayer = -1;

    [Header("References")]
    public Camera playerCamera;
    public ObjectViewerPanel viewerPanel;

    private CharacterController controller;
    private float verticalRotation;
    private InteractableObject currentHoverTarget;
    private bool isRoamingEnabled = true;
    private float suppressInteractUntil;

    [Header("Audio")]
    public AudioSource mainAudioSource;
    public AudioClip footstepClip;       // 脚步声
    public AudioClip mouseClickClip;     // 鼠标点击
    public AudioClip pageTurnClip;

    [Header("音量控制")]
    public float footstepVolume = 1f;
    public float mouseClickVolume = 0.6f;
    public float pageTurnVolume = 0.7f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        // Parent camera to player so it follows CharacterController movement
        if (playerCamera != null && playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
        }
    }

    void Start()
    {
        // 游戏开始默认进入漫游状态（隐藏鼠标、锁定视角）
        SetRoamingEnabled(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (mainAudioSource != null && mouseClickClip != null)
                mainAudioSource.PlayOneShot(mouseClickClip, pageTurnVolume); // 👈 音量
            Debug.Log("<color=blue>【鼠标点击】▶️ 播放点击音效</color>");
        }

        // 翻页音效
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mainAudioSource != null && pageTurnClip != null)

                mainAudioSource.PlayOneShot(pageTurnClip, pageTurnVolume); // 👈 音量
            Debug.Log("<color=yellow>【书本翻页】▶️ 播放翻页音效</color>");
        }

        // ⬇️ ====== 新增：每次点击鼠标左键时显示位置及 UI 穿透 Debug ======
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 获取鼠标在屏幕上的像素坐标 (以左下角为 0,0)
            Vector3 mousePos = Input.mousePosition;

            // 2. 检测当前鼠标穿透到了哪一个 UI 元素上
            string hitURIName = "无 (点在了空白处或物理世界)";
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                // 创建一个模拟的指针事件数据
                var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                eventData.position = mousePos;

                // 发射一条专门检测 UI 的射线矩阵
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

                if (results.Count > 0)
                {
                    // 抓取挡在最前面的那层 UI 名字
                    hitURIName = results[0].gameObject.name;
                }
            }

            // 3. 打印核心 Debug 日志
            Debug.Log($"<color=#FF00FF>[鼠标点击 Debug]</color> 屏幕坐标: ({mousePos.x:F1}, {mousePos.y:F1}) | 挡在最前面的 UI: <color=yellow>【{hitURIName}】</color> | 当前漫游状态: {isRoamingEnabled}");
        }
        // ⬆️

        // 1. 每帧优先检测右键点击，用来无条件切换漫游/UI模式
        HandleRightClickToggle();


        // 2. 如果当前不是漫游模式（即鼠标被呼出状态）
        if (!isRoamingEnabled)
        {
            // 按 Esc 时不仅关闭查看面板，还要自动恢复漫游模式
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (viewerPanel != null) viewerPanel.Hide();
                SetRoamingEnabled(true);
            }
            return; // 拦截后续的视角转动、移动和射线检测
        }

        // 3. 漫游模式下的正常每帧行为
        HandleMouseLook();
        HandleMovement();
        HandleInteraction();
    }

    /// <summary>
    /// 核心新增：处理右键切换鼠标释放与锁定
    /// </summary>
    void HandleRightClickToggle()
    {
        if (Input.GetMouseButtonDown(1)) // 检测鼠标右键按下
        {
            // 状态取反：如果是漫游就切到UI，如果是UI就切回漫游
            SetRoamingEnabled(!isRoamingEnabled);

            // 细节打磨：如果主动通过右键切回漫游，把可能开着的检视面板顺手关掉
            if (isRoamingEnabled && viewerPanel != null)
            {
                viewerPanel.Hide();
            }
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }


    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        controller.SimpleMove(move * walkSpeed);

        // ==========================================
        // 脚步声：按就播，松就强制停止
        // ==========================================
        bool isMoving = (Mathf.Abs(h) + Mathf.Abs(v)) > 0.1f;
        Debug.Log($"【脚步声Debug】是否移动 = {isMoving}");
        if (isMoving)
        {
            // 正在移动：播放脚步声
            if (!mainAudioSource.isPlaying)
            {
                mainAudioSource.clip = footstepClip;
                mainAudioSource.loop = false; // 你的是长音频，但不用循环
                mainAudioSource.volume = footstepVolume;
                mainAudioSource.Play();
                Debug.Log("<color=green>【脚步声Debug】▶️ 开始播放脚步声</color>");
            }
        }
        else
        {
            // 没按键：强制停止声音！！！
            if (mainAudioSource.isPlaying)
            {
                mainAudioSource.Stop();
                Debug.Log("<color=red>【脚步声Debug】⏹️ 松开按键，强制停止脚步声</color>");
            }
        }
    }

    /// <summary>短暂屏蔽左键检视，避免与对话点击、关面板同一帧冲突。</summary>
    public void SuppressInteraction(float seconds = 0.3f)
    {
        suppressInteractUntil = Time.unscaledTime + Mathf.Max(0.05f, seconds);
    }

    public bool IsInteractionSuppressed => Time.unscaledTime < suppressInteractUntil;

    void HandleInteraction()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
            return;

        if (IsInteractionSuppressed)
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            Debug.Log($"检测到物体: {hit.collider.name}，Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            var interactable = hit.collider.GetComponentInParent<InteractableObject>();
            if (interactable != null)
            {
                if (currentHoverTarget != interactable)
                {
                    if (currentHoverTarget != null)
                        currentHoverTarget.SetHighlight(false);
                    currentHoverTarget = interactable;
                    currentHoverTarget.SetHighlight(true);
                    viewerPanel.ShowHint(interactable.displayName);
                }

                if (Input.GetMouseButtonDown(0) && viewerPanel != null)
                {
                    currentHoverTarget.SetHighlight(false);
                    SetRoamingEnabled(false); // 👈 这一步会自动呼出真实的鼠标去点 UI
                    viewerPanel.Show(interactable);
                }
                return;
            }
        }

        if (currentHoverTarget != null)
        {
            currentHoverTarget.SetHighlight(false);
            currentHoverTarget = null;
            viewerPanel.HideHint();
        }
    }

    public void SetRoamingEnabled(bool enabled)
    {
        isRoamingEnabled = enabled;
        ApplyCursorForRoaming(enabled);

        // 核心修复：退出漫游（enabled == false）时，强制物理切断射线悬停状态，腾出屏幕图层空间
        if (!enabled)
        {
            if (currentHoverTarget != null)
            {
                currentHoverTarget.SetHighlight(false);
                currentHoverTarget = null;
            }
            viewerPanel?.HideHint();
        }
    }

    public void ApplyCursorForRoaming(bool roaming)
    {
        if (roaming)
        {
            // ==== 漫游模式下 ====
            GameplayCursor.HideForGameplay();

            Cursor.lockState = CursorLockMode.Locked; // 强行把鼠标锁在屏幕正中心
            Cursor.visible = false;                   // 隐藏系统自带指针
        }
        else
        {
            // ==== UI 点击模式下 ====
            GameplayCursor.ShowForUI();

            Cursor.lockState = CursorLockMode.None;   // 解除中心锁定，让鼠标可以在屏幕自由滑行
            Cursor.visible = true;                    // 显示真实的指针
        }
    }
}
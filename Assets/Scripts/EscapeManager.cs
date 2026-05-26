using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EscapeManager : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject escapePanel;          // 挣脱面板
    public Slider progressBar;              // 进度条
    public Text progressText;               // 进度文字
    public Text timeText;                   // 时间文字
    public Text tipText;                    // 提示文字

    [Header("默认参数")]
    public int defaultRequiredClicks = 8;   // 默认需要点击的次数
    public float defaultTimeLimit = 3f;     // 默认时间限制

    private static EscapeManager _instance;
    private bool isEscaping = false;
    private int requiredClicks;
    private int currentClicks;
    private float timeLimit;
    private float currentTime;
    private System.Action onSuccess;
    private System.Action onFail;

    public static EscapeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EscapeManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EscapeManager");
                    _instance = go.AddComponent<EscapeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 初始隐藏面板
        if (escapePanel != null)
            escapePanel.SetActive(false);
    }

    // 完整参数版本
    public void ShowEscapePanel(int clicks, float timeLimit, System.Action successCallback, System.Action failCallback)
    {
        requiredClicks = clicks;
        this.timeLimit = timeLimit;
        onSuccess = successCallback;
        onFail = failCallback;

        currentClicks = 0;
        currentTime = timeLimit;
        isEscaping = true;

        if (escapePanel != null)
        {
            escapePanel.SetActive(true);
            UpdateUI();
        }

        if (tipText != null)
        {
            tipText.text = $"快速点击鼠标左键！({requiredClicks}次)";
        }

        Debug.Log($"挣脱开始！需要点击 {requiredClicks} 次，时间 {timeLimit} 秒");
    }

    // 使用默认参数的简化版本
    public void ShowEscapePanel(System.Action successCallback, System.Action failCallback)
    {
        ShowEscapePanel(defaultRequiredClicks, defaultTimeLimit, successCallback, failCallback);
    }

    void Update()
    {
        if (!isEscaping) return;

        // 倒计时
        currentTime -= Time.deltaTime;
        UpdateUI();

        if (currentTime <= 0)
        {
            Fail();
            return;
        }

        // 检测鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            currentClicks++;
            UpdateUI();

            // 震动效果
            if (escapePanel != null)
            {
                escapePanel.transform.DOShakePosition(0.05f, 5f, 10);
            }

            // 可选：播放点击音效
            // AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

            Debug.Log($"挣脱点击: {currentClicks}/{requiredClicks}");

            if (currentClicks >= requiredClicks)
            {
                Success();
            }
        }
    }

    void UpdateUI()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = requiredClicks;
            progressBar.value = currentClicks;
        }

        if (progressText != null)
        {
            progressText.text = $"{currentClicks}/{requiredClicks}";
        }

        if (timeText != null)
        {
            timeText.text = $"{currentTime:F1}s";
        }
    }

    void Success()
    {
        isEscaping = false;
        onSuccess?.Invoke();

        if (escapePanel != null)
            escapePanel.SetActive(false);

        Debug.Log("挣脱成功！");
    }

    void Fail()
    {
        isEscaping = false;
        onFail?.Invoke();

        if (escapePanel != null)
            escapePanel.SetActive(false);

        Debug.Log("挣脱失败！");
    }

    public void StopEscape()
    {
        isEscaping = false;
        if (escapePanel != null)
            escapePanel.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class TipManager : MonoBehaviour
{
    [Header("UI组件")]
    public Text tipText;              // 提示文字
    public GameObject tipPanel;       // 提示面板
    public Image tipBackground;       // 背景（可选）

    [Header("动画参数")]
    public float fadeDuration = 0.3f;  // 淡入淡出时间
    public float displayDuration = 2f; // 显示持续时间
    public float slideDistance = 50f;  // 滑动距离

    private static TipManager _instance;
    private CanvasGroup canvasGroup;
    private Coroutine currentTipCoroutine;

    // 单例模式
    public static TipManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TipManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TipManager");
                    _instance = go.AddComponent<TipManager>();
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

        // 获取CanvasGroup组件
        if (tipPanel != null)
        {
            canvasGroup = tipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tipPanel.AddComponent<CanvasGroup>();
            }
            tipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示普通提示
    /// </summary>
    public void ShowTip(string message, float duration = -1)
    {
        if (currentTipCoroutine != null)
            StopCoroutine(currentTipCoroutine);

        currentTipCoroutine = StartCoroutine(ShowTipCoroutine(message, duration > 0 ? duration : displayDuration));
    }

    /// <summary>
    /// 显示警告提示（红色）
    /// </summary>
    public void ShowWarning(string message, float duration = -1)
    {
        ShowColoredTip(message, Color.red, duration);
    }

    /// <summary>
    /// 显示成功提示（绿色）
    /// </summary>
    public void ShowSuccess(string message, float duration = -1)
    {
        ShowColoredTip(message, Color.green, duration);
    }

    /// <summary>
    /// 显示彩色提示
    /// </summary>
    public void ShowColoredTip(string message, Color color, float duration = -1)
    {
        if (tipText != null)
            tipText.color = color;

        ShowTip(message, duration);

        // 重置颜色
        if (tipText != null)
            StartCoroutine(ResetColor());
    }

    IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(displayDuration + fadeDuration);
        if (tipText != null)
            tipText.color = Color.white;
    }

    IEnumerator ShowTipCoroutine(string message, float duration)
    {
        if (tipText != null)
            tipText.text = message;

        if (tipPanel != null)
        {
            tipPanel.SetActive(true);

            // 淡入动画
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, fadeDuration);
            }

            // 滑动动画（从上方滑入）
            RectTransform rect = tipPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 originalPos = rect.anchoredPosition;
                rect.anchoredPosition = new Vector2(originalPos.x, originalPos.y + slideDistance);
                rect.DOAnchorPos(originalPos, fadeDuration);
            }
        }

        // 显示时间
        yield return new WaitForSeconds(duration);

        // 淡出动画
        if (tipPanel != null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, fadeDuration);
            }

            yield return new WaitForSeconds(fadeDuration);
            tipPanel.SetActive(false);

            if (canvasGroup != null)
                canvasGroup.alpha = 1;
        }

        currentTipCoroutine = null;
    }

    /// <summary>
    /// 显示进度提示（用于挣脱等）
    /// </summary>
    public void ShowProgressTip(string message, int current, int total)
    {
        if (tipText != null)
        {
            tipText.text = $"{message} ({current}/{total})";
        }

        if (tipPanel != null && !tipPanel.activeSelf)
        {
            tipPanel.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
            }
        }
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void HideTip()
    {
        if (currentTipCoroutine != null)
        {
            StopCoroutine(currentTipCoroutine);
            currentTipCoroutine = null;
        }

        if (tipPanel != null)
        {
            tipPanel.SetActive(false);
        }
    }
}
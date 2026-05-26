using UnityEngine;

/// <summary>
/// 独立控制按钮组显示/隐藏
/// </summary>
public class ShowButton : MonoBehaviour
{
    [Header("把你的选择按钮拖进来")]
    public GameObject[] choiceButtons;

    void Awake()
    {
        // 一开始就隐藏
        Hide();
    }

    [ContextMenu("显示按钮")]
    public void Show()
    {
        Debug.Log("[独立控制器] 显示按钮");
        SetButtonsActive(true);
    }

    [ContextMenu("隐藏按钮")]
    public void Hide()
    {
        Debug.Log("[独立控制器] 隐藏按钮");
        SetButtonsActive(false);
    }

    public void SetButtonsActive(bool active)
    {
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.SetActive(active);
        }
    }
}

using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueUIController : MonoBehaviour
{
    public static ClueUIController Instance;
    public GameObject overlayPanel;
    public Image displayImage;
    public TextMeshProUGUI displayText;

    void Awake()
    {
        Instance = this;
        InitUI();
    }
    // 自动化寻找组件，确保你设计的排版能被正确识别
    void InitUI()
    {
        // 1. 找到母版面板
        overlayPanel = transform.Find("ClueOverlay").gameObject;

        // 2. 在母版里精准寻找负责展示的组件（根据你起的名字）
        // 这样即便你改了排版，只要名字对，就能自动更新
        displayImage = overlayPanel.transform.Find("Portrait").GetComponent<Image>();
        displayText = overlayPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>();

        overlayPanel.SetActive(false);
    }

    public void ShowClue(ClueData data)
    {
        if (data == null) return;

        // 自动变动内容：将线索的数据赋给 UI 模板
        displayImage.sprite = data.portrait; // 换图
        displayText.text = data.description; // 换字

        overlayPanel.SetActive(true);
        Time.timeScale = 0; // 暂停游戏以便阅读
    }

    public void HideClue()
    {
        overlayPanel.SetActive(false);
        Time.timeScale = 1;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EndingTrigger : MonoBehaviour
{
    [Header("玩家标签")]
    public string playerTag = "Player";

    [Header("UI引用")]
    public GameObject choicePanel;
    public Button buttonPrefab;
    public Transform buttonParent;
    public Image endCGImage;

    [Header("结局设置")]
    public List<EndingOption> endingOptions = new List<EndingOption>();

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;
        if (other.CompareTag(playerTag))
        {
            alreadyTriggered = true;
            ShowChoicePanel();
        }
    }

    void ShowChoicePanel()
    {
        // 清空旧按钮
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        // 生成新按钮
        foreach (var option in endingOptions)
        {
            Button btn = Instantiate(buttonPrefab, buttonParent);
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = option.buttonText;

            // 捕获当前选项，避免闭包问题
            var capturedOption = option;
            btn.onClick.AddListener(() =>
            {
                OnChoiceSelected(capturedOption);
            });
        }

        choicePanel.SetActive(true);
        Time.timeScale = 0; // 暂停游戏，避免玩家继续操作
    }

    void OnChoiceSelected(EndingOption option)
    {
        choicePanel.SetActive(false);
        Time.timeScale = 1;

        // 显示结局CG
        endCGImage.sprite = option.cgSprite;
        endCGImage.color = Color.white;
        endCGImage.gameObject.SetActive(true);

        // 可选：CG播放完后退出游戏/回到菜单
        Invoke(nameof(ExitToMenu), 3f); // 3秒后自动退出，可自行调整
    }

    void ExitToMenu()
    {
        // 这里写你回到菜单或退出游戏的逻辑
        // 例如：SceneManager.LoadScene("Menu");
        Debug.Log("游戏结束，回到菜单");
    }
}

[System.Serializable]
public class EndingOption
{
    public string buttonText;       // 按钮上显示的文字
    public Sprite cgSprite;         // 对应的结局CG图片
}

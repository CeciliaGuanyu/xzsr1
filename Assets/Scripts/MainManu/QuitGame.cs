using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    // 退出游戏按钮点击事件
    public void OnLoginButtonClick()
    {
        Application.Quit();
    }
}

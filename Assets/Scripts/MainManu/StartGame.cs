using System. Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartGame:MonoBehaviour
{
        public void OnLoginButtonClick()
    {
        // SceneManager.LoadScene(1);
        // 场景名必须和Build Settings里的场景名称一致
        SceneTransitionLoad.Instance.LoadScene("SampleScene");

    }
}

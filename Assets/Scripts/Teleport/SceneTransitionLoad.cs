using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionLoad : MonoBehaviour
{
    // 单例，方便其他脚本调用
    public static SceneTransitionLoad Instance;

    [Header("黑屏设置")]
    public Image blackScreen;       // 拖入你刚才做的全屏黑Image
    public float fadeSpeed = 3f;    // 淡入淡出速度，越大越快
    public float loadWaitTime = 0.3f;// 场景加载后黑屏停留一小段时间

    private void Awake()
    {
        // 单例初始化，防止重复创建
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换不销毁这个对象
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 对外暴露的调用方法，其他脚本直接调用这个
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    // 核心过场协程
    IEnumerator TransitionCoroutine(string sceneName)
    {
        // 1. 黑屏淡入（从透明→纯黑）
        blackScreen.enabled = true;
        Color screenColor = blackScreen.color;
        screenColor.a = 0;
        blackScreen.color = screenColor;

        while (screenColor.a < 1f)
        {
            screenColor.a += fadeSpeed * Time.deltaTime;
            blackScreen.color = screenColor;
            yield return null;
        }

        // 2. 加载目标场景（这一步会异步加载，不会卡顿）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. 场景加载完成后，黑屏短暂停留，再淡出
        yield return new WaitForSeconds(loadWaitTime);

        while (screenColor.a > 0f)
        {
            screenColor.a -= fadeSpeed * Time.deltaTime;
            blackScreen.color = screenColor;
            yield return null;
        }

        // 4. 隐藏黑屏，节省性能
        blackScreen.enabled = false;
    }
}

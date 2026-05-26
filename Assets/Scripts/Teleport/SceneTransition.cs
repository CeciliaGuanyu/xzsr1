using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    // 单例，方便其他脚本调用
    public static SceneTransition Instance;

    [Header("过渡设置")]
    public UnityEngine.UI.Image transitionImage;
    public float fadeDuration = 1f; // 淡入淡出时长（秒）

    private void Awake()
    {
        // 单例初始化，防止重复实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 外部调用：带过渡效果的场景加载
    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    // 核心协程：淡出 → 加载场景 → 淡入
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // 第一步：淡出到黑屏
        yield return StartCoroutine(FadeOut());

        // 第二步：加载目标场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 第三步：从黑屏淡入
        yield return StartCoroutine(FadeIn());
    }

    // 淡出：透明度从0→1（变黑）
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = transitionImage.color;
        color.a = 0f;
        transitionImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            transitionImage.color = color;
            yield return null;
        }
        color.a = 1f;
        transitionImage.color = color;
    }

    // 淡入：透明度从1→0（变透明）
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = transitionImage.color;
        color.a = 1f;
        transitionImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            transitionImage.color = color;
            yield return null;
        }
        color.a = 0f;
        transitionImage.color = color;
    }
}

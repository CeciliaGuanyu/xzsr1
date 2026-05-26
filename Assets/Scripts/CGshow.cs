using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CGFadeInNative : MonoBehaviour
{
    [Header("全局黑屏遮罩")]
    public CanvasGroup fadeCanvasGroup;

    [Header("CG与音效")]
    public AudioSource cgAudioSource;
    public AudioClip cgBgm;

    [Header("时间配置")]
    public float fadeDuration = 7f;    // 渐变时长
    public float cgStayTime = 7f;      // CG总停留时长

    [Header("结束设置")]
    public bool fadeOutAfter = true;   // 是否看完再渐黑
    public int targetSceneIndex = 0;   // 跳转场景序号(0=主菜单)

    void Start()
    {
        // 初始强制全屏全黑
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(CGFullSequence());
    }

    IEnumerator CGFullSequence()
    {
        // 1. 立刻播放CG背景音乐
        if (cgAudioSource != null && cgBgm != null)
        {
            cgAudioSource.clip = cgBgm;
            cgAudioSource.Play();
        }

        // 2. 黑屏渐变透明 → CG慢慢完整显示
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // 3. CG完整停留展示
        yield return new WaitForSeconds(cgStayTime);

        // 4. 可选：CG看完，再次渐变黑屏
        if (fadeOutAfter)
        {
            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 5. 黑屏后跳转指定场景
        SceneManager.LoadScene(targetSceneIndex);
    }
}
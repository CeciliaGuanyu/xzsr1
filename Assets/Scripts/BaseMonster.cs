using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BaseMonster : MonoBehaviour
{
    [Header("移动参数")]
    public float flyDuration = 0.5f;
    public Ease moveEase = Ease.OutBack;

    [Header("挣脱参数")]
    public int requiredClicks = 8;
    public float timeLimit = 3f;

    [Header("死亡结局 CG")]
    public GameObject deathCGCanvas;
    public AudioSource deathAudioSource;
    public AudioClip deathBgm;
    public float cgStayTime = 7f;

    [Header("渐变设置")]
    public CanvasGroup fadeCanvasGroup;    // 全局黑屏遮罩
    public CanvasGroup cgCanvasGroup;       // CG界面挂的CanvasGroup
    public float fadeSpeed = 1f;

    private Transform targetPoint;
    private MonsterTrigger trigger;
    private bool isAttacking = false;

    public virtual void Initialize(Transform target, MonsterTrigger trig)
    {
        targetPoint = target;
        trigger = trig;
        MoveToTarget();
    }

    protected virtual void MoveToTarget()
    {
        if (targetPoint == null) return;

        transform.DOLookAt(targetPoint.position, 0.3f);
        transform.DOMove(targetPoint.position, flyDuration)
            .SetEase(moveEase)
            .OnComplete(() => OnReachTarget());
    }

    protected virtual void OnReachTarget()
    {
        isAttacking = true;
        EscapeManager.Instance.ShowEscapePanel(
            requiredClicks,
            timeLimit,
            OnEscapeSuccess,
            OnEscapeFail
        );
        TipManager.Instance.ShowWarning($"💀 怪物抓住了你！快速点击鼠标左键 {requiredClicks} 次挣脱！", timeLimit);
    }

    protected virtual void OnEscapeSuccess()
    {
        TipManager.Instance.ShowSuccess("✓ 挣脱成功！", 1f);
        if (trigger != null) trigger.UnlockPlayer();
        transform.DOScale(0, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    protected virtual void OnEscapeFail()
    {
        TipManager.Instance.ShowWarning("✗ 挣脱失败...", 2f);

        // 1. 游戏画面渐变变黑
        fadeCanvasGroup.DOFade(1, fadeSpeed).OnComplete(() =>
        {
            // 激活CG界面，初始透明
            deathCGCanvas.SetActive(true);
            cgCanvasGroup.alpha = 0;

            // 2. CG渐变淡入
            cgCanvasGroup.DOFade(1, fadeSpeed).OnComplete(() =>
            {
                // 播放死亡音效
                if (deathAudioSource != null && deathBgm != null)
                    deathAudioSource.PlayOneShot(deathBgm);

                // 3. 停留7秒
                DOVirtual.DelayedCall(cgStayTime, () =>
                {
                    // 4. CG渐变淡出
                    cgCanvasGroup.DOFade(0, fadeSpeed).OnComplete(() =>
                    {
                        // 加载主菜单场景
                        SceneManager.LoadSceneAsync(0).completed += op =>
                        {
                            // 5. 场景加载完毕，黑屏渐变淡出，显示主菜单
                            fadeCanvasGroup.DOFade(0, fadeSpeed);
                        };
                    });
                });
            });
        });
    }
}
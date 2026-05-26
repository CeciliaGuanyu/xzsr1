using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger1 : MonoBehaviour
{
    [Header("场景设置")]
    public string sceneName = "GameOver";     // 要跳转的场景名称
    public float delay = 1f;                  // 延迟时间（秒）
    public string spawnPointTag = "OneToTwo";//增加1
    [Header("触发设置")]
    public bool oneTimeOnly = true;           // 是否只触发一次
    public bool destroyOnTrigger = false;     // 触发后是否销毁自身

    [Header("反馈")]
    public bool showTip = true;               // 是否显示提示
    public string tipMessage = "加载场景中..."; // 提示文字

    private bool hasTriggered = false;

   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {

            if (oneTimeOnly)
                hasTriggered = true;
            
            // 显示提示
            if (showTip && TipManager.Instance != null)
            {
                TipManager.Instance.ShowWarning(tipMessage, delay);
            }

            // 延迟加载场景
            Invoke("LoadScene", delay);

            // 可选：销毁自身
            if (destroyOnTrigger)
                Destroy(gameObject, delay + 0.1f);
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
           
            SceneManager.LoadScene(sceneName);
          
        }
        else
        {
            Debug.LogError("场景名称未设置！");
        }
    }

    // 可选：碰撞检测（用于非触发器）
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasTriggered)
        {
            if (oneTimeOnly)
                hasTriggered = true;
            
            if (showTip && TipManager.Instance != null)
            {
                TipManager.Instance.ShowWarning(tipMessage, delay);
            }

            Invoke("LoadScene", delay);

            if (destroyOnTrigger)
                Destroy(gameObject, delay + 0.1f);
        }
    }
   
}
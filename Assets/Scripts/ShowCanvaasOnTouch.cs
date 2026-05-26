using UnityEngine;

public class ShowCanvasOnTouch : MonoBehaviour
{
    public GameObject targetCanvas; // 拖入你的Canvas

    // 进入触发区域
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 玩家标签必须是Player
        {
            targetCanvas.SetActive(true);
        }
    }

    // 离开隐藏（不需要就删掉这段）
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetCanvas.SetActive(false);
        }
    }
}

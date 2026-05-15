
using UnityEngine;
using UnityEngine.AI;

public class Teleport : MonoBehaviour
{
    [Header("传送目标点")]
    public Transform targetPoint;

    // 防止多次连续传送
    private bool isTeleported = false;

    private void OnTriggerEnter(Collider other)
    {
        // 不是玩家 或者 已经传过了 直接返回
        if (isTeleported || !other.CompareTag("Player"))
            return;

        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // NavAgent角色专用瞬移，不会被导航拉回
            agent.Warp(targetPoint.position);
            // 同步旋转（可选）
            other.transform.rotation = targetPoint.rotation;
        }
        else
        {
            // 没有导航组件就普通瞬移
            other.transform.position = targetPoint.position;
            other.transform.rotation = targetPoint.rotation;
        }

        isTeleported = true;
        // 1秒后重置，可以再次传送
        Invoke(nameof(ResetTeleport), 1f);
    }

    void ResetTeleport()
    {
        isTeleported = false;
    }
}

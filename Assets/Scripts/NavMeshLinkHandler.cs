using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshLinkHandler : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 必须关闭自动穿越，否则你的自定义逻辑永远不会执行
        agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        // 持续检测是否在 NavMeshLink 上
        if (agent.isOnOffMeshLink)
        {
            // 获取当前 Link 的终点
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;

            // 传送逻辑（也可以换成跳跃动画）
            transform.position = linkData.endPos;

            // 关键！必须调用，否则代理会卡住，认为自己还在 Link 上
            agent.CompleteOffMeshLink();

            Debug.Log("已完成 NavMeshLink 传送");
        }
    }
}

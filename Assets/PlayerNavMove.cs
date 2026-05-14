using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMove : MonoBehaviour
{
    private NavMeshAgent agent;
    public float moveSpeed = 6f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        // 获取WASD输入
        float h = Input.GetAxis("Horizontal");  // A D
        float v = Input.GetAxis("Vertical");    // W S

        // 构建移动方向
        Vector3 dir = transform.right * h + transform.forward * v;

        if (dir.magnitude > 0.1f)
        {
            // 用NavMesh设置目标点，自动避障
            agent.SetDestination(transform.position + dir);
        }
        else
        {
            // 没有输入就停下
            agent.SetDestination(transform.position);
        }
    }
}

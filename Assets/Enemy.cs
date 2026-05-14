using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 敌人追踪玩家的速度
    public float moveSpeed = 5f;

    // 敌人停止追踪玩家的最大距离
    public float detectionDistance = 10f;

    // 敌人每次碰撞对玩家造成的伤害值
    public int damageToPlayer = 20;

    // 敌人碰撞玩家时播放的音效
    public AudioClip hitSound;

    // 音效的音量
    public float soundVolume = 1f;

    // 玩家对象的引用，用于追踪
    private Transform player;

    // 用于播放音效的AudioSource组件
    private AudioSource audioSource;

    void Start()
    {
        // 尝试获取玩家对象（这里假设玩家对象上有一个"Player"标签）
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // 如果玩家存在且距离小于等于检测距离，则追踪玩家
        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionDistance)
        {
            Vector3 moveDirection = player.position - transform.position;
            moveDirection.Normalize(); // 标准化方向向量
            transform.position += moveDirection * moveSpeed * Time.deltaTime; // 根据速度和时间更新位置

            // 也可以让敌人面向玩家（可选）
            // transform.LookAt(player);
        }
        // 如果玩家距离过远，则停止追踪（这里其实不需要额外代码，因为条件不满足时不会执行追踪逻辑）
    }

    void OnCollisionEnter(Collision collision)
    {
        // 检查碰撞到的对象是否有Player标签
        if (collision.collider.CompareTag("Player"))
        {
            // 播放碰撞音效
            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound, soundVolume);
            }

            // 尝试从碰撞的对象中获取PlayerHealth组件
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();

            // 如果找到了PlayerHealth组件，则调用其TakeDamage方法
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }
            else
            {
                // 如果玩家对象上没有PlayerHealth组件，则输出错误信息
                Debug.LogError("Player object collided with but does not have PlayerHealth component!");
            }
        }
    }
}

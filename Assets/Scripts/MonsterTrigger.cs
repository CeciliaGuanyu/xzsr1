using UnityEngine;

public class MonsterTrigger : MonoBehaviour
{
    [Header("玩家设置")]
    public Transform playerLockPoint;       // 玩家被锁定的位置（位置和角度）

    [Header("怪物设置")]
    public GameObject monster;              // 场景中的怪物
    public Transform monsterTargetPoint;    // 怪物要移动到的目标点

    [Header("触发设置")]
    public bool oneTimeOnly = true;         // 是否只触发一次
    public float delayBeforeActivate = 0.5f; // 延迟激活怪物

    private bool hasTriggered = false;
    private Transform player;
    private FPCharacter playerController;
    private FPMouseLook mouseLook;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerController = player.GetComponent<FPCharacter>();
            mouseLook = player.GetComponent<FPMouseLook>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && monster != null)
        {
            if (oneTimeOnly)
                hasTriggered = true;

            TipManager.Instance.ShowWarning("⚠️ 危险！怪物出现了！", 1f);
            Invoke("ActivateMonster", delayBeforeActivate);
        }
    }

    private void ActivateMonster()
    {
        // 1. 锁定玩家到指定位置和角度
        LockPlayer();

        // 2. 激活怪物
        monster.SetActive(true);

        // 3. 获取怪物脚本并初始化
        BaseMonster baseMonster = monster.GetComponent<BaseMonster>();
        if (baseMonster != null)
        {
            baseMonster.Initialize(monsterTargetPoint, this);
        }
    }

    private void LockPlayer()
    {
        if (player == null) return;

        // 保存原始位置（用于后续恢复）
        originalPlayerPos = player.position;
        originalPlayerRot = player.rotation;

        // 冻结玩家移动和视角
        if (playerController != null)
            playerController.SetGrabbed(true);

        if (mouseLook != null)
        {
            mouseLook.SetFrozen(true);
            mouseLook.ResetCameraAngle(); // 重置相机角度为0
        }

        // 强制设置玩家位置和角度
        if (playerLockPoint != null)
        {
            player.position = playerLockPoint.position;
            player.rotation = playerLockPoint.rotation;

            // 同时设置相机角度（如果是第一人称）
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        // 锁定光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("玩家已锁定到指定位置");
    }

    public void UnlockPlayer()
    {
        if (player == null) return;

        // 恢复玩家控制
        if (playerController != null)
            playerController.SetGrabbed(false);

        if (mouseLook != null)
            mouseLook.SetFrozen(false);

        // 恢复光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("玩家已解锁");
    }

    // 可视化
    private void OnDrawGizmos()
    {
        // 触发器范围
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            if (col is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider sphere)
                Gizmos.DrawSphere(sphere.center, sphere.radius);
        }

        // 玩家锁定点
        if (playerLockPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerLockPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, playerLockPoint.position);

            // 绘制方向箭头
            Vector3 forward = playerLockPoint.forward;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerLockPoint.position, forward * 1f);
        }

        // 怪物目标点
        if (monsterTargetPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(monsterTargetPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, monsterTargetPoint.position);
        }

        // 怪物
        if (monster != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, monster.transform.position);
        }
    }
}
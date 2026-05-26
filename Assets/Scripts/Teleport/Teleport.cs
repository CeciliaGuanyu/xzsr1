using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Teleport : MonoBehaviour
{
    [Header("传送目标点")]
    public Transform targetPoint;
    private CharacterController controller;
    private bool isTeleported = false;

    [Header("黑屏过渡设置")]
    public UnityEngine.UI.Image blackScreen;
    public float fadeSpeed = 2f;   // 统一淡入淡出速度
    public float blackStayTime = 1f; // 黑屏静止时长

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleported || !other.CompareTag("Player"))
            return;

        StartCoroutine(TeleportCoroutine(other));
    }

    IEnumerator TeleportCoroutine(Collider other)
    {
        isTeleported = true;

        // 1. 黑屏快速淡入（重写流畅渐变，替换原有分段时间）
        yield return StartCoroutine(FadeScreen(1));

        // ========== 你原本完整传送逻辑 一字未改 ==========
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        controller = other.GetComponent<CharacterController>();
        if (agent != null && controller != null)
        {
            controller.enabled = false;
            agent.Warp(targetPoint.position);
            other.transform.rotation = targetPoint.rotation;
            controller.enabled = true;
        }
        else
        {
            other.transform.position = targetPoint.position;
            other.transform.rotation = targetPoint.rotation;
        }
        // ================================================

        // 2. 黑屏停留
        yield return new WaitForSeconds(blackStayTime);

        // 3. 黑屏淡出
        yield return StartCoroutine(FadeScreen(0));

        // 重置传送冷却
        Invoke(nameof(ResetTeleport), 300f);
    }

    // 统一封装黑屏淡入淡出（只改这里黑屏逻辑，原版传送不动）
    IEnumerator FadeScreen(float targetAlpha)
    {
        if (blackScreen == null) yield break;

        blackScreen.enabled = true;
        Color screenColor = blackScreen.color;

        while (!Mathf.Approximately(screenColor.a, targetAlpha))
        {
            screenColor.a = Mathf.MoveTowards(screenColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            blackScreen.color = screenColor;
            yield return null;
        }

        // 透明度归0后关闭图片节省性能
        if (targetAlpha <= 0)
        {
            blackScreen.enabled = false;
        }
    }

    void ResetTeleport()
    {
        isTeleported = false;
    }
}

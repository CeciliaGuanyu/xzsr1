using System.Collections;
using UnityEngine;

/// <summary>
/// 统一管理第一人称游玩时的鼠标显示/隐藏。
/// </summary>
public static class GameplayCursor
{
    public static void HideForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void ShowForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>延迟隐藏，避免与 UI/对话切换同一帧冲突。</summary>
    public static IEnumerator HideAfterFrames(MonoBehaviour host, int frames = 2)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
        HideForGameplay();
    }
}

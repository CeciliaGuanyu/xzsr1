using UnityEngine;
using UnityEditor;

public static class DialogueSetupEditor
{
    [MenuItem("Tools/Setup Dialogue System")]
    static void Setup()
    {
        if (Object.FindObjectOfType<DialogueManager>() != null)
        {
            Debug.Log("[DialogueSetup] 场景中已有 DialogueManager。");
            Selection.activeObject = Object.FindObjectOfType<DialogueManager>();
            return;
        }

        var root = new GameObject("DialogueSystem");
        root.AddComponent<DialogueManager>();

        Undo.RegisterCreatedObjectUndo(root, "Setup Dialogue System");
        Selection.activeGameObject = root;

        Debug.Log("[DialogueSetup] 已创建 DialogueSystem。请添加 DialogueTrigger 组件并配置触发条件与对话内容。");
    }
}

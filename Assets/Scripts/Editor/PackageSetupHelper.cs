#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PackageSetupHelper
{
    // 需要自动添加的 Layer 名称
    private static readonly string[] requiredLayers = { "ObjectPreview" }; 

    static PackageSetupHelper()
    {
        SetupLayers();
    }

    private static void SetupLayers()
    {
        Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (asset == null || asset.Length == 0) return;

        SerializedObject tagManager = new SerializedObject(asset[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        bool isModified = false;

        foreach (string layer in requiredLayers)
        {
            bool found = false;

            // 检查 Layer 是否已经存在 (总共 32 个槽位)
            for (int i = 0; i <= 31; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp != null && sp.stringValue.Equals(layer))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // 自定义 Layer 从索引 8 开始，找到第一个空槽位填入
                for (int i = 8; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp != null && string.IsNullOrEmpty(sp.stringValue))
                    {
                        sp.stringValue = layer;
                        isModified = true;
                        Debug.Log($"自动添加了缺失的 Layer: {layer}");
                        break;
                    }
                }
            }
        }

        if (isModified)
        {
            tagManager.ApplyModifiedProperties();
            tagManager.Update();
        }
    }
}
#endif
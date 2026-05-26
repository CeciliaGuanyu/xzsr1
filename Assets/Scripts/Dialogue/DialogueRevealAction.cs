using System;
using UnityEngine;

[Serializable]
public class DialogueRevealAction
{
    [Tooltip("要显示的场景物体（SetActive = true）")]
    public GameObject[] showObjects;

    [Tooltip("要隐藏的场景物体（SetActive = false）")]
    public GameObject[] hideObjects;

    public bool HasContent()
    {
        return HasAny(showObjects) || HasAny(hideObjects);
    }

    static bool HasAny(GameObject[] array)
    {
        if (array == null || array.Length == 0) return false;
        foreach (var go in array)
        {
            if (go != null) return true;
        }
        return false;
    }
}

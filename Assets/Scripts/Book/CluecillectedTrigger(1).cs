using UnityEngine;

// 强行约束：挂载此脚本的物体上必须同时拥有这两个组件，防止配置遗漏
[RequireComponent(typeof(ClueItem))]
[RequireComponent(typeof(InteractableObject))]
public class ClueCollectOnInteract : MonoBehaviour
{
    private InteractableObject interactable;
    private ClueItem clueItem;

    void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        clueItem = GetComponent<ClueItem>();

        // 🎯 核心逻辑：利用运行时反射，把我们的收集方法偷偷塞进你原本的 onInteracted 事件中
        // 这样不用修改你原本的任何一行代码，点击时就会顺带触发这里的收集
        if (interactable != null && interactable.onInteracted != null)
        {
            interactable.onInteracted.AddListener(OnObjectClicked);
        }
    }

    /// <summary>
    /// 当物体被点击（进入检视）的瞬间，由 UnityEvent 自动带起该方法
    /// </summary>
    private void OnObjectClicked()
    {
        if (clueItem == null) return;

        // 寻找你的书本控制器
        BookTurnController bookController = FindObjectOfType<BookTurnController>();

        if (bookController != null)
        {
            // 扔给书本控制器去加入数组并打印日志
            bookController.CollectClue(clueItem);
        }
        else
        {
            Debug.LogError($"[线索联动] 场景中找不到 BookTurnController，无法将 【{clueItem.clueName}】 送入数组。");
        }
    }

    void OnDestroy()
    {
        // 销毁时记得顺手解绑，保持内存干净
        if (interactable != null && interactable.onInteracted != null)
        {
            interactable.onInteracted.RemoveListener(OnObjectClicked);
        }
    }
}
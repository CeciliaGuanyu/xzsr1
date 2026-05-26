using UnityEngine;
using System.Collections.Generic;
// 引入 TMP 命名空间
using TMPro;
using UnityEngine.UI;
using echo17.EndlessBook;

public class BookTurnController : MonoBehaviour
{
    protected EndlessBook book;

    public float stateAnimationTime = 1f;
    public EndlessBook.PageTurnTimeTypeEnum turnTimeType = EndlessBook.PageTurnTimeTypeEnum.TotalTurnTime;
    public float turnTime = 1f;

    [Header("页码物理 UI（TMP）")]
    public TextMeshProUGUI leftPageText;
    public TextMeshProUGUI rightPageText;

    // ────────────────────────────────────────────────────────
    // 🎯 新增：线索数据与动态渲染核心槽位
    // ────────────────────────────────────────────────────────
    [Header("动态收集的线索阵列 (无序收集，动态排队)")]
    private List<ClueItem> collectedClues = new List<ClueItem>();

    [Header("左右书页的 UI 渲染组件")]
    public Image leftClueImage;
    public TextMeshProUGUI leftClueText;
    public Image rightClueImage;
    public TextMeshProUGUI rightClueText;

    [Header("默认空白页表现")]
    public Sprite blankPageSprite;

    void Awake()
    {
        book = GetComponent<EndlessBook>();
        Debug.Log("书本初始化成功");
    }

    void Start()
    {
        RefreshPageNumber();
        // 初始化时根据当前物理页码刷一次内容
        int left = book.CurrentPageNumber;
        UpdateLeftPage(left);
        UpdateRightPage(left + 1);
    }

    // ────────────────────────────────────────────────────────
    // 🎯 新增：供外部调用的线索收集方法（完美内嵌你的需求）
    // ────────────────────────────────────────────────────────
    public void CollectClue(ClueItem clue)
    {
        if (clue == null) return;

        // 防止同组件重复收集
        if (collectedClues.Contains(clue))
        {
            Debug.Log($"[线索本] 物品 {clue.name} 已经收集过了，无需重复加入。");
            return;
        }

        // 顺序追加进数组，此时它的 Index 就是它的动态 ID
        collectedClues.Add(clue);
        int assignedID = collectedClues.Count - 1;

        // 🎯 满足你的需求：加入某某物品已收集，ID为几的 debug 日志
        Debug.Log($"<color=green>[线索收集系统]</color> 物品 <b>【{clue.name}】</b> 已收集！分配的线索ID为: <b>{assignedID}</b>");

        // 收集完毕后，实时刷新一下当前书页的内容，防止玩家正开着书时数据不更新
        RefreshCurrentPagesContent();
    }

    void Update()
    {
        // 开合书本 Z / X / C / V / B
        if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Get Z");
            book.SetState(EndlessBook.StateEnum.ClosedFront, stateAnimationTime, OnBookStateChanged);
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.X))
            book.SetState(EndlessBook.StateEnum.OpenFront, stateAnimationTime, OnBookStateChanged);
        if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            book.SetState(EndlessBook.StateEnum.OpenMiddle, stateAnimationTime, OnBookStateChanged);
        if (UnityEngine.Input.GetKeyDown(KeyCode.V))
            book.SetState(EndlessBook.StateEnum.OpenBack, stateAnimationTime, OnBookStateChanged);
        if (UnityEngine.Input.GetKeyDown(KeyCode.B))
            book.SetState(EndlessBook.StateEnum.ClosedBack, stateAnimationTime, OnBookStateChanged);

        // 左右箭头翻页
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
        {
            book.TurnBackward(turnTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
        {
            book.TurnForward(turnTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }

        // 数字键 1~0 跳页
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) { TurnToPage(1); }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) { TurnToPage(3); } // 跨双页跳跃
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) { TurnToPage(5); }
    }

    void TurnToPage(int page)
    {
        book.TurnToPage(
            page,
            turnTimeType,
            turnTime,
            stateAnimationTime,
            OnBookTurnToPageCompleted,
            OnPageTurnStart,
            OnPageTurnEnd
        );
    }

    public virtual void OnStateButtonClicked(int buttonIndex)
    {
        book.SetState((EndlessBook.StateEnum)buttonIndex, animationTime: stateAnimationTime, onCompleted: OnBookStateChanged);
    }

    public virtual void OnPageButtonClicked(int pageNumber)
    {
        book.TurnToPage(pageNumber == 999 ? book.LastPageNumber : pageNumber,
            turnTimeType,
            turnTime,
            openTime: stateAnimationTime,
            onCompleted: OnBookTurnToPageCompleted,
            onPageTurnStart: OnPageTurnStart,
            onPageTurnEnd: OnPageTurnEnd
        );
    }

    public virtual void OnTurnButtonClicked(int direction)
    {
        if (direction == -1)
        {
            book.TurnBackward(turnTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }
        else
        {
            book.TurnForward(turnTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }
    }

    protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
    {
        Debug.Log("State set to " + toState + ". Current Page Number = " + currentPageNumber);
        RefreshPageNumber();
        RefreshCurrentPagesContent();
    }

    protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
    {
        Debug.Log("OnBookTurnToPageCompleted: Current Page = " + currentPageNumber);
    }

    protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
    {
    }

    protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
    {
        Debug.Log("当前左页: " + pageNumberFirstVisible + "  右页: " + (pageNumberFirstVisible + 1));

        int leftPage = pageNumberFirstVisible;
        int rightPage = pageNumberFirstVisible + 1;

        // 🎯 翻页结束，利用物理页码去算数组下标对应更新内容
        UpdateLeftPage(leftPage);
        UpdateRightPage(rightPage);
        RefreshPageNumber(leftPage, rightPage);
    }

    // TMP 更新页码
    void RefreshPageNumber(int leftPage, int rightPage)
    {
        if (leftPageText != null)
            leftPageText.text = leftPage.ToString();

        if (rightPageText != null)
            rightPageText.text = rightPage.ToString();
    }

    void RefreshPageNumber()
    {
        int left = book.CurrentPageNumber;
        int right = left + 1;
        RefreshPageNumber(left, right);
    }

    void RefreshCurrentPagesContent()
    {
        if (book != null)
        {
            UpdateLeftPage(book.CurrentPageNumber);
            UpdateRightPage(book.CurrentPageNumber + 1);
        }
    }

    // ────────────────────────────────────────────────────────
    // 🎯 核心逻辑映射：用 pageIndex 对应计算 ID 并更新内容
    // ────────────────────────────────────────────────────────
    [Header("地图配置")]
    [Tooltip("固定显示在第1页和第2页的地图贴图（如果需要的话，不需要可以不赋值）")]
    public Sprite mapSprite;

    // ────────────────────────────────────────────────────────
    // 🎯 核心逻辑映射：支持前两页固定地图，第三页开始对齐 ID 0
    // ────────────────────────────────────────────────────────

    // 【全新的数学映射公式】：
    // 物理第 1 页 ➔ 固定显示地图 ➔ 跳过线索数组
    // 物理第 2 页 ➔ 固定显示地图 ➔ 跳过线索数组
    // 物理第 3 页 ➔ 对应线索 ID 0 (公式：3 - 3 = 0)
    // 物理第 4 页 ➔ 对应线索 ID 1 (公式：4 - 3 = 1)

    void UpdateLeftPage(int pageIndex)
    {
        if (pageIndex == 1 || pageIndex == 2)
        {
            // 如果是第 1 或 2 页，执行地图渲染
            RenderMapToPage(leftClueImage, leftClueText, pageIndex);
        }
        else
        {
            // 第 3 页及以后，减去 3 偏移量，让第 3 页精准对应数组 0 号元素
            int targetID = pageIndex - 3;
            RenderClueToPage(targetID, leftClueImage, leftClueText);
        }
    }

    void UpdateRightPage(int pageIndex)
    {
        if (pageIndex == 1 || pageIndex == 2)
        {
            // 如果是第 1 或 2 页，执行地图渲染
            RenderMapToPage(rightClueImage, rightClueText, pageIndex);
        }
        else
        {
            // 右页同样遵循：物理页码减 3 映射到数组下标
            int targetID = pageIndex - 3;
            RenderClueToPage(targetID, rightClueImage, rightClueText);
        }
    }

    /// <summary>
    /// 🎯 新增：专门渲染 1、2 页固定地图的函数
    /// </summary>
    private void RenderMapToPage(Image pageImage, TextMeshProUGUI pageText, int pageIndex)
    {
        if (pageImage != null)
        {
            pageImage.gameObject.SetActive(true);
            if (mapSprite != null)
            {
                pageImage.sprite = mapSprite;
            }
        }

        if (pageText != null)
        {
            pageText.text = $"<b>【全域地图 - 第 {pageIndex} 页】</b>\n\n这里是固定显示的地图内容，不受收集影响。";
        }
    }

    /// <summary>
    /// 核心渲染函数：根据计算出的线索 ID 决定是显示线索内容还是显示空白
    /// </summary>
    private void RenderClueToPage(int id, Image pageImage, TextMeshProUGUI pageText)
    {
        // 打印 Debug 帮我们在控制台肉眼追踪页码和 ID 的转换关系
        Debug.Log($"[本子渲染追踪] 收到渲染请求，计算出的虚拟线索 ID 为: {id}。当前数组内共有线索 {collectedClues.Count} 个。");

        if (id >= 0 && id < collectedClues.Count)
        {
            ClueItem activeClue = collectedClues[id];

            Debug.Log($"<color=cyan>[本子渲染成功]</color> 正在将 ID:{id} 的物品【{activeClue.clueName}】的数据刷到 UI 上！");

            if (pageImage != null)
            {
                pageImage.gameObject.SetActive(true);
                pageImage.sprite = activeClue.clueSprite != null ? activeClue.clueSprite : blankPageSprite;
            }

            if (pageText != null)
            {
                // 完美映射：把你的 ClueItem 数据卡的具体文本展现出来
                pageText.text = $"<b>线索卡 ID: {id} - {activeClue.clueName}</b>\n\n{activeClue.clueDescription}";
            }
        }
        else
        {
            // 超出当前已收集的数组长度，说明玩家还没点击该顺序的物品，显示空白页
            if (pageImage != null)
            {
                if (blankPageSprite != null)
                {
                    pageImage.gameObject.SetActive(true);
                    pageImage.sprite = blankPageSprite;
                }
                else
                {
                    pageImage.gameObject.SetActive(false);
                }
            }

            if (pageText != null)
            {
                pageText.text = "（这里空空如也，似乎还没有收集到相关线索...）";
            }
        }
    }
}
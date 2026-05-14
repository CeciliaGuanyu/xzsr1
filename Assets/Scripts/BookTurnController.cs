namespace echo17.EndlessBook.Demo01
{
    using UnityEngine;
    // 引入 TMP 命名空间
    using TMPro;
    using echo17.EndlessBook;

    public class BookTUrnController : MonoBehaviour
    {
        protected EndlessBook book;

        public float stateAnimationTime = 1f;
        public EndlessBook.PageTurnTimeTypeEnum turnTimeType = EndlessBook.PageTurnTimeTypeEnum.TotalTurnTime;
        public float turnTime = 1f;

        [Header("页码显示（TMP）")]
        public TextMeshProUGUI leftPageText;
        public TextMeshProUGUI rightPageText;

        void Awake()
        {
            book = GetComponent<EndlessBook>();
            Debug.Log("书本初始化成功");
        }

        void Start()
        {
            RefreshPageNumber();
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
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) { TurnToPage(2); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) { TurnToPage(3); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4)) { TurnToPage(4); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5)) { TurnToPage(5); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6)) { TurnToPage(6); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7)) { TurnToPage(7); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8)) { TurnToPage(8); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9)) { TurnToPage(9); }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0)) { TurnToPage(10); }
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

        void UpdateLeftPage(int pageIndex)
        {
            // 左页内容更新（自己扩展）
        }

        void UpdateRightPage(int pageIndex)
        {
            // 右页内容更新（自己扩展）
        }
    }
}
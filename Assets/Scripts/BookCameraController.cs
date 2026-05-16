using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class BookCameraToggle : MonoBehaviour
{
    [Header("绑定Book对应的Canvas界面")]
    public GameObject bookCanvas;

    private Camera _bookCam;
    private bool isOpen = false;

    void Awake()
    {
        // 获取自身相机
        _bookCam = GetComponent<Camera>();

        // 默认初始关闭
        CloseBook();
    }

    // ====== 给按钮OnClick绑定这个方法即可 ======
    public void ToggleBook()
    {
        if (isOpen)
            CloseBook();
        else
            OpenBook();
    }

    void OpenBook()
    {
        isOpen = true;
        _bookCam.enabled = true;
        if (bookCanvas != null)
            bookCanvas.SetActive(true);
    }

    void CloseBook()
    {
        isOpen = false;
        _bookCam.enabled = false;
        if (bookCanvas != null)
            bookCanvas.SetActive(false);
    }
}
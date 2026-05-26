using UnityEngine;

public class FPMouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    private float _rotationX = 0;
    private bool isUpdate = true;
    private bool isClimbing = false;
    private bool isFrozen = false;  // 新增：是否冻结视角
                                    // 在 FPMouseLook 类中添加这个方法
    public void ResetCameraAngle()
    {
        _rotationX = 0;
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

        // 如果有相机，也重置相机角度
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.identity;
        }
    }
    public void NoUpdat()
    {
        isUpdate = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetClimbing(bool climbing)
    {
        isClimbing = climbing;
    }

    // 新增：设置冻结状态
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        if (frozen)
        {
            // 可选：显示光标方便点击UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Start()
    {
        sensitivityHor = 9.0f;
        sensitivityVert = 9.0f;
        minimumVert = -45.0f;
        maximumVert = 45.0f;
        _rotationX = 0;
        isUpdate = true;
        isFrozen = false;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null) body.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 冻结时不处理视角旋转
        if (isFrozen) return;

        if (Input.GetKey(KeyCode.Escape))
        {
            NoUpdat();
        }
        else if (!isClimbing)
        {
            isUpdate = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!isUpdate) return;

        if (isClimbing && axes == RotationAxes.MouseXAndY)
        {
            float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityHor;
            transform.localEulerAngles = new Vector3(0, rotationY, 0);
            return;
        }

        if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
        }
        else if (axes == RotationAxes.MouseY)
        {
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
            transform.localEulerAngles = new Vector3(_rotationX, transform.localEulerAngles.y, 0);
        }
        else
        {
            float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityHor;
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        }
    }
}
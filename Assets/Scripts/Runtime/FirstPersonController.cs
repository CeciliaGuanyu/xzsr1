using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Interaction")]
    public float interactRange = 3f;
    public LayerMask interactLayer = -1;

    [Header("References")]
    public Camera playerCamera;
    public ObjectViewerPanel viewerPanel;

    private CharacterController controller;
    private float verticalRotation;
    private InteractableObject currentHoverTarget;
    private bool isRoamingEnabled = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        // Parent camera to player so it follows CharacterController movement
        if (playerCamera != null && playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
        }
    }

    void Start()
    {
        SetRoamingEnabled(true);
    }

    void Update()
    {
        if (!isRoamingEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                viewerPanel.Hide();
            return;
        }

        HandleMouseLook();
        HandleMovement();
        HandleInteraction();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        controller.SimpleMove(move * walkSpeed);
    }

    void HandleInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {

            // 【新增调试代码 2】：只要射中任何东西，就在控制台打印它的名字
            Debug.Log($"射线击中了物体: {hit.collider.name}，它所在的Layer是: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            var interactable = hit.collider.GetComponentInParent<InteractableObject>();
            if (interactable != null)
            {
                if (currentHoverTarget != interactable)
                {
                    if (currentHoverTarget != null)
                        currentHoverTarget.SetHighlight(false);
                    currentHoverTarget = interactable;
                    currentHoverTarget.SetHighlight(true);
                    viewerPanel.ShowHint(interactable.displayName);
                }

                if (Input.GetMouseButtonDown(0) && viewerPanel != null)
                {
                    currentHoverTarget.SetHighlight(false);
                    SetRoamingEnabled(false);
                    viewerPanel.Show(interactable);
                }
                return;
            }
        }

        if (currentHoverTarget != null)
        {
            currentHoverTarget.SetHighlight(false);
            currentHoverTarget = null;
            viewerPanel.HideHint();
        }
    }

    public void SetRoamingEnabled(bool enabled)
    {
        isRoamingEnabled = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;

        if (!enabled)
        {
            if (currentHoverTarget != null)
            {
                currentHoverTarget.SetHighlight(false);
                currentHoverTarget = null;
            }
            viewerPanel.HideHint();
        }
    }
}

using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class FPCharacter : MonoBehaviour
{
    public float speed = 2.0f;
    public float speedfast = 4.0f;
    public float gravity = -9.8f;
    float _speedfast;

    private CharacterController _charController;
    private Transform _cameraTransform;

    private bool isClimbing = false;
    private float climbSpeed = 3.0f;
    private Vector3 climbDirection = Vector3.up;

    private bool isGrabbed = false;  // 被抓住时不能移动

    void Start()
    {
        speed = 2.0f;
        speedfast = 4.0f;
        gravity = -9.8f;
        _charController = GetComponent<CharacterController>();

        Camera mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera != null)
        {
            _cameraTransform = mainCamera.transform;
        }
        else
        {
            _cameraTransform = Camera.main?.transform;
        }

        gameObject.tag = "Player";
    }

    public void SetGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;
        if (grabbed)
        {
            _speedfast = 0;
        }
    }

    public void SetClimbing(bool climbing, float speed)
    {
        isClimbing = climbing;
        climbSpeed = speed;
        if (!climbing)
        {
            _speedfast = 1;
        }
    }

    public void StopClimbing()
    {
        isClimbing = false;
    }

    public void StartClimbing(Transform targetPoint, float duration, Ease easeType, bool lookAtTarget)
    {
        if (isClimbing) return;

        isClimbing = true;
        _charController.enabled = false;

        if (lookAtTarget && targetPoint != null)
        {
            Vector3 directionToTarget = targetPoint.position - transform.position;
            directionToTarget.y = 0;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.DORotateQuaternion(targetRotation, 0.3f);
            }
        }

        transform.DOMove(targetPoint.position, duration)
            .SetEase(easeType)
            .OnComplete(() => {
                _charController.enabled = true;
                isClimbing = false;
                TipManager.Instance.ShowSuccess("攀爬完成！", 1f);
            });
    }

    void Update()
    {
        // 被抓住时不能移动和攀爬
        if (isGrabbed) return;

        if (isClimbing)
        {
            HandleClimbing();
            return;
        }

        HandleGroundMovement();
    }

    void HandleClimbing()
    {
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = Vector3.zero;

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            movement = climbDirection * verticalInput * climbSpeed;
        }

        movement *= Time.deltaTime;
        _charController.Move(movement);
    }

    void HandleGroundMovement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speedfast = speedfast;
        }
        else
        {
            _speedfast = 1;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = Vector3.zero;

        if (_cameraTransform != null)
        {
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            movement = (cameraForward * verticalInput + cameraRight * horizontalInput) * speed * _speedfast;
        }
        else
        {
            float deltaX = horizontalInput * speed * _speedfast;
            float deltaZ = verticalInput * speed * _speedfast;
            movement = new Vector3(deltaX, 0, deltaZ);
        }

        float maxSpeed = speed * _speedfast;
        if (movement.magnitude > maxSpeed)
        {
            movement = movement.normalized * maxSpeed;
        }

        movement.y = gravity;
        movement *= Time.deltaTime;

        movement = transform.TransformDirection(movement);
        _charController.Move(movement);
    }
}
using UnityEngine;

public class TestMove : MonoBehaviour
{
    Rigidbody rb;

    public float moveSpeed = 12f;
    public float rotateSpeed = 180f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // JK旋转（核心部分，单独抽出来）
        float rotateInput = 0;
        if (Input.GetKey(KeyCode.J)) rotateInput = -1;
        if (Input.GetKey(KeyCode.K)) rotateInput = 1;
        //Debug.Log("Rotate Input: " + rotateInput);
        // 直接修改transform，不受Rigidbody影响
        transform.Rotate(Vector3.up, rotateInput * rotateSpeed * Time.deltaTime);

        // WASD移动（和之前逻辑一致）
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;
        right.y = 0;

        Vector3 moveDir = forward * v + right * h;
        if (moveDir.magnitude > 1)
            moveDir.Normalize();

        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
    }
}
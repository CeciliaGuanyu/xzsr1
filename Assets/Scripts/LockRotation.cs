using UnityEngine;

public class LockUIRotation : MonoBehaviour
{
    // 你想要固定的旋转值，直接在这里设置好
    public Vector3 fixedEulerAngles = new Vector3(0, 0, 0);

    void LateUpdate()
    {
        // 强制覆盖旋转，让它永远保持你设置的值
        transform.rotation = Quaternion.Euler(fixedEulerAngles);
    }
}

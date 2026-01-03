using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 120f;

    [Header("References")]
    public Transform playerBody; // gắn Player (object có PlayerMotor)

    private float xRotation = 0f;

    void Start()
    {
        if (playerBody == null) playerBody = transform.parent; // camera là child
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // pitch (camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // yaw (player)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

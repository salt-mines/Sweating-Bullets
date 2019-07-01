using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float mouseSensitivity = 0.1f;

    public float Forward { get; private set; }
    public float Strafe { get; private set; }

    public float MouseX { get; private set; }
    public float MouseY { get; private set; }

    void Update()
    {
        Forward = Input.GetAxisRaw("Vertical");
        Strafe = Input.GetAxisRaw("Horizontal");

        MouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        MouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
    }
}

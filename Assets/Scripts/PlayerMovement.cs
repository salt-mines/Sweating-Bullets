using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Range(0, 100)]
    public float movementSpeed = 6.0f;

    public float mouseSensitivity = 2.0f;

    private Vector3 movement = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private CharacterController characterController;
    private Camera playerCamera;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        var x = Input.GetAxisRaw("Mouse X");
        var y = -Input.GetAxisRaw("Mouse Y");

        rotation.x += x * mouseSensitivity;
        rotation.y += y * mouseSensitivity;
        playerCamera.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);

        movement = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        movement.Normalize();

        transform.rotation = Quaternion.Euler(0, rotation.x, 0);

        characterController.SimpleMove(movement * movementSpeed);
    }
}

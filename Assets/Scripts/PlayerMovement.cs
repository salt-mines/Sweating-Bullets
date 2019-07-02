using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Range(0, 100)]
    public float movementSpeed = 6.0f;
    [Range(0, 100)]
    public float jumpSpeed = 12.0f;
    [Range(0, 100)]
    public float gravity = 6.0f;

    private Vector3 movement = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private PlayerInput playerInput;
    private CharacterController characterController;
    private Camera playerCamera;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        movement = transform.forward * playerInput.Forward + transform.right * playerInput.Strafe;
        movement.Normalize();

        transform.rotation = Quaternion.Euler(0, rotation.x, 0);
        characterController.SimpleMove(movement * movementSpeed);
    }

    private void LateUpdate()
    {
        rotation.x += playerInput.MouseX;
        rotation.y = Mathf.Clamp(rotation.y - playerInput.MouseY, -89.99f, 89.99f);

        playerCamera.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);   
    }
}

using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Range(0, 100)]
    public float movementSpeed = 6.0f;
    [Range(0, 100)]
    public float jumpSpeed = 8.0f;
    [Range(0, 100)]
    public float gravity = 20.0f;
    [Range(0, 100)]
    public float sprintSpeed = 11.0f;

    private float vSpeed = 0;

    private Vector3 movement = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private Vector3 lastPosition;

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
        if (characterController.isGrounded)
        {
            vSpeed = 0f;
            movement = new Vector3(playerInput.Strafe, 0, playerInput.Forward);
            movement.Normalize();
            movement = transform.TransformDirection(movement);

            if (playerInput.Sprint)
            {
                movement *= sprintSpeed;
            }
            else
            {
                movement *= movementSpeed;
            }
            if (playerInput.Jump)
            {
                vSpeed = jumpSpeed;
            }
        }
        vSpeed -= gravity * Time.deltaTime;
        movement.y = vSpeed;
        characterController.Move(movement * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, rotation.x, 0);

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        rotation.x += playerInput.MouseX;
        rotation.y = Mathf.Clamp(rotation.y - playerInput.MouseY, -89.99f, 89.99f);

        playerCamera.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);   
    }
}

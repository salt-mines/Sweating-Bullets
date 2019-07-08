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
    [Range(1, 5)]
    public float sprintSpeedMod = 1.5f;
    [Range(0, 1)]
    public float jumpMovementMod = 0.5f;
    [Range(0, 1)]
    public float crouchSpeedMod = 0.5f;


    private float vJumpSpeed = 0;
    private float movementSpeedLastFrame;

    private bool isCrouching;

    private Vector3 movement = Vector3.zero;
    private Vector3 groundMovement = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 airMovement = Vector3.zero;
    private Vector3 standingCameraPos;

    private Vector3 lastPosition;

    private PlayerInput playerInput;
    private CharacterController characterController;
    private Camera playerCamera;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        standingCameraPos = playerCamera.transform.position;
    }

    private void FixedUpdate()
    {
        if (characterController.isGrounded)
        {
            vJumpSpeed = 0f;
            movement = new Vector3(playerInput.Strafe, 0, playerInput.Forward);
            movement.Normalize();
            movement = transform.TransformDirection(movement);
            
            if (playerInput.Sprint && !isCrouching)
            {
                movement *= movementSpeed * sprintSpeedMod;
            }
            else
            {
                if (playerInput.Crouch)
                {
                    transform.localScale = new Vector3(1, 0.5f, 1);
                    movement *= movementSpeed * crouchSpeedMod;
                    isCrouching = true;
                }
                else if(!playerInput.Crouch)
                {
                    movement *= movementSpeed;
                    transform.localScale = new Vector3(1, 1, 1);
                    isCrouching = false;
                }
            }
            if (playerInput.Jump)
            {
                vJumpSpeed = jumpSpeed;
            }
            groundMovement = movement;
        }
        else
        {
            AirMove();
        }

        vJumpSpeed -= gravity * Time.deltaTime;
        movement.y = vJumpSpeed;

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

    private void AirMove()
    {
        airMovement = new Vector3(playerInput.Strafe, 0, playerInput.Forward);
        airMovement.Normalize();
        airMovement = transform.TransformDirection(airMovement);
        airMovement *= jumpMovementMod;

        movement = groundMovement + airMovement;
    }
}

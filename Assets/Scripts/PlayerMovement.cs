using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Unity properties

    [Range(0, 100)]
    public float movementSpeed = 6.0f;

    [Range(0, 100)]
    public float jumpSpeed = 8.0f;

    [Range(0, 100)]
    public float gravity = 20.0f;

    [Range(0, 1)]
    public float jumpMovementMod = 0.5f;

    #endregion

    #region Movement state variables

    private float vJumpSpeed;

    private Vector3 movement = Vector3.zero;
    private Vector3 groundMovement = Vector3.zero;
    private Vector3 airMovement = Vector3.zero;

    #endregion

    #region Components

    private PlayerInput playerInput;
    private CharacterController characterController;

    #endregion

    #region Unity events

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (characterController.isGrounded)
        {
            vJumpSpeed = 0f;
            movement.x = playerInput.Strafe;
            movement.z = playerInput.Forward;
            movement.Normalize();

            movement = transform.TransformDirection(movement) * movementSpeed;

            if (playerInput.Jump) vJumpSpeed = jumpSpeed;

            groundMovement = movement;
        }
        else
        {
            AirMove();
        }

        vJumpSpeed -= gravity * Time.deltaTime;
        movement.y = vJumpSpeed;

        characterController.Move(movement * Time.deltaTime);
    }

    #endregion

    #region Movement methods

    private void AirMove()
    {
        airMovement.x = playerInput.Strafe;
        airMovement.z = playerInput.Forward;

        airMovement.Normalize();
        airMovement = jumpMovementMod * transform.TransformDirection(airMovement);

        movement = groundMovement + airMovement;
    }

    public void ResetMovement()
    {
        vJumpSpeed = 0;
        movement = Vector3.zero;
        airMovement = Vector3.zero;
        groundMovement = Vector3.zero;
    }

    #endregion
}
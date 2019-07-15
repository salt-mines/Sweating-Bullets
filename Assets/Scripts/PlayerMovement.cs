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

    #endregion

    #region Movement state variables

    private float verticalSpeed;

    private Vector3 velocity = Vector3.zero;

    public Vector3 Velocity
    {
        get => velocity;
        private set => velocity = value;
    }

    private Vector3 targetMovement = Vector3.zero;

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
        targetMovement.x = playerInput.Strafe;
        targetMovement.z = playerInput.Forward;
        targetMovement.Normalize();
        targetMovement = transform.TransformDirection(targetMovement * movementSpeed);

        if (characterController.isGrounded && velocity.y <= 0)
        {
            velocity.x = targetMovement.x;
            velocity.z = targetMovement.z;

            if (playerInput.Jump)
                verticalSpeed = jumpSpeed;
        }

        verticalSpeed -= gravity * Time.deltaTime;
        velocity.y = verticalSpeed;

        characterController.Move(velocity * Time.deltaTime);
    }

    #endregion

    #region Movement methods

    public void ResetMovement()
    {
        verticalSpeed = 0;
        velocity = Vector3.zero;
        targetMovement = Vector3.zero;
    }

    #endregion
}
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
    [Range(0, 1)]
    public float jumpMovementMod = 0.5f;

    private float vJumpSpeed = 0;

    private Vector3 movement = Vector3.zero;
    private Vector3 groundMovement = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    private Vector3 airMovement = Vector3.zero;
    private Vector3 standingCameraPos;

    private PlayerInput playerInput;
    private PlayerMechanics playerMechanics;
    private CharacterController characterController;
    private Camera playerCamera;
    private GameManager gameManager;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMechanics = GetComponent<PlayerMechanics>();
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
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
            movement *= movementSpeed;
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

    }

    private void LateUpdate()
    {
        rotation.x += playerInput.MouseX;
        rotation.y = Mathf.Clamp(rotation.y - playerInput.MouseY, -89.99f, 89.99f);
        if (!gameManager.paused)
        {   
            playerCamera.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);
        }
    }
            

    private void AirMove()
    {
        airMovement = new Vector3(playerInput.Strafe, 0, playerInput.Forward);
        airMovement.Normalize();
        airMovement = transform.TransformDirection(airMovement);
        airMovement *= jumpMovementMod;

        movement = groundMovement + airMovement;
    }

    public void Reset(GameObject spawnPoint)
    {
        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;
        
        rotation.x = spawnPoint.transform.rotation.eulerAngles.y;
        rotation.y = 0;

        playerCamera.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);
    }
}

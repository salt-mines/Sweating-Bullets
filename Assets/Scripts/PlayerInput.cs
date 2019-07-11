using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float mouseSensitivity = 0.1f;

    public float Forward { get; private set; }
    public float Strafe { get; private set; }

    public float MouseX { get; private set; }
    public float MouseY { get; private set; }

    public bool Jump { get; private set; }

    public bool Sprint { get; private set; }

    public bool Crouch { get; private set; }

    public bool Cancel { get; private set; }

    public bool MouseLocked
    {
        get
        {
            return Cursor.lockState != CursorLockMode.None;
        }
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        MouseLocked = true;
    }

    private void Update()
    {
        if (!gameManager.paused)
        {
            Forward = Input.GetAxisRaw("Vertical");
            Strafe = Input.GetAxisRaw("Horizontal");

            MouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            MouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            Jump = Input.GetButton("Jump");
            Sprint = Input.GetButton("Sprint");
            Crouch = Input.GetButton("Crouch");
        }
        else
        {
            Forward = 0;
            Strafe = 0;
            Jump = false;
            Crouch = false;
            MouseX = 0;
            MouseY = 0;
        }

        Cancel = Input.GetButtonDown("Cancel");
    }
}

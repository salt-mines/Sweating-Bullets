using Game;
using UnityEngine;

public class ExtendedFlyCam : MonoBehaviour
{
    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
    LICENSE
        Free as in speech, and free as in beer.
 
    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
              Shift:    Move faster
            Control:    Move slower
                End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */

    public float cameraSensitivity = 16f;
    public float climbSpeed = 6;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    private float rotationX;
    private float rotationY;

    private void Start()
    {
        GameInput.MouseLocked = true;

        var localRotation = transform.localRotation;
        rotationX = localRotation.eulerAngles.y;
        rotationY = -localRotation.eulerAngles.x;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.End)) GameInput.MouseLocked = !GameInput.MouseLocked;

        if (!GameInput.MouseLocked) return;

        rotationX += Input.GetAxisRaw("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotationY += Input.GetAxisRaw("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        var tr = transform;

        tr.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up) * Quaternion.AngleAxis(rotationY, Vector3.left);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            tr.position += normalMoveSpeed * fastMoveFactor * Input.GetAxis("Vertical") * Time.deltaTime * tr.forward;
            tr.position += normalMoveSpeed * fastMoveFactor * Input.GetAxis("Horizontal") * Time.deltaTime * tr.right;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            tr.position += normalMoveSpeed * slowMoveFactor * Input.GetAxis("Vertical") * Time.deltaTime * tr.forward;
            tr.position += normalMoveSpeed * slowMoveFactor * Input.GetAxis("Horizontal") * Time.deltaTime * tr.right;
        }
        else
        {
            tr.position += normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime * tr.forward;
            tr.position += normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime * tr.right;
        }


        if (Input.GetKey(KeyCode.Q)) tr.position -= climbSpeed * Time.deltaTime * tr.up;
        if (Input.GetKey(KeyCode.E)) tr.position += climbSpeed * Time.deltaTime * tr.up;
    }
}
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public PlayerInput input;
    private Transform player;

    private Vector2 viewAngles = Vector2.zero;

    private void Start()
    {
        player = transform.parent;
    }

    private void FixedUpdate()
    {
        player.localRotation = Quaternion.AngleAxis(viewAngles.x, player.transform.up);
    }

    private void Update()
    {
        viewAngles.x += input.MouseX;
        viewAngles.y = Mathf.Clamp(viewAngles.y - input.MouseY, -89.99f, 89.99f);

        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }

    public void SetAngles(Vector2 angles)
    {
        viewAngles = angles;
        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }
}
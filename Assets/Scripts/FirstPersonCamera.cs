using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public PlayerInput input;
    private Transform player;

    private Vector2 viewAngles = Vector2.zero;

    private void Awake()
    {
        player = transform.parent;
    }

    private void Update()
    {
        viewAngles.x += input.MouseX;
        viewAngles.y = Mathf.Clamp(viewAngles.y - input.MouseY, -89.99f, 89.99f);

        player.localRotation = Quaternion.AngleAxis(viewAngles.x, player.transform.up);
        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }

    public void SetAngles(Vector2 angles)
    {
        viewAngles = angles;
        player.localRotation = Quaternion.AngleAxis(viewAngles.x, player.transform.up);
        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }
}
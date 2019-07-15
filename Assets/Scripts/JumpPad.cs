using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpStrength = 20f;

    public bool keepPlayerVelocity;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        var pm = other.gameObject.GetComponent<PlayerMovement>();
        if (!pm) return;

        var vel = transform.up * jumpStrength;

        if (keepPlayerVelocity)
        {
            vel.x += pm.Velocity.x;
            vel.z += pm.Velocity.z;
        }
        
        pm.Velocity = vel;
    }
}
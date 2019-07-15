using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpStrength = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerMovement>().Velocity = transform.up * jumpStrength;
    }
}
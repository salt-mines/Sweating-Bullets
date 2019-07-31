using UnityEngine;

namespace Game
{
    public class JumpPad : MonoBehaviour
    {
        private static readonly int Spring = Animator.StringToHash("Spring");

        public float jumpStrength = 20f;
        public bool keepPlayerVelocity;

        public Animator animator;
        public AudioSource audioSource;

        private void Awake()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            if (!audioSource)
                audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Tags.Player)) return;

            var pm = other.gameObject.GetComponent<PlayerMovement>();
            if (!pm) return;

            var vel = transform.up * jumpStrength;

            if (keepPlayerVelocity)
            {
                vel.x += pm.Velocity.x;
                vel.z += pm.Velocity.z;
            }

            pm.Velocity = vel;

            if (animator)
                animator.SetTrigger(Spring);

            if (audioSource)
                audioSource.Play();
        }
    }
}
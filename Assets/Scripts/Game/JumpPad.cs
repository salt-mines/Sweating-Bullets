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
            var go = other.gameObject;
            var isLocal = go.CompareTag(Tags.Player);
            var isNetPlayer = go.layer == (int) Layer.Players;

            if (isLocal || isNetPlayer)
            {
                if (audioSource)
                    audioSource.Play();

                if (animator)
                    animator.SetTrigger(Spring);
            }

            if (!isLocal) return;

            var pm = go.GetComponent<PlayerMovement>();
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
}
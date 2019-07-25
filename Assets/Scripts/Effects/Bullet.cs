using UnityEngine;

namespace Effects
{
    public class Bullet : MonoBehaviour
    {
        public ParticleSystem bloodSplatter;
        public ParticleSystem wallSplatter;

        public AudioSource audioSource;

        public AudioClip playerHitSound;
        public AudioClip wallHitSound;
    }
}
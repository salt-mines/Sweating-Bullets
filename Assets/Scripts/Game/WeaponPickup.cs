using UnityEngine;

namespace Game
{
    public class WeaponPickup : MonoBehaviour
    {
        public pickUps pickUp = pickUps.Shotgun;

        private AudioSource audioSource;

        private void Start()
        {
            if(!audioSource) audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Tags.Player)) return;

            var pm = other.gameObject.GetComponent<PlayerMechanics>();

            if (!pm) return;

            pm.SetWeapon(1);

            if (audioSource) audioSource.Play();
        }
    }

    public enum pickUps
    {
        Shotgun,
        Rifle
    }
}
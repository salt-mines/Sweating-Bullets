using UnityEngine;

namespace Game
{
    public class WeaponPickup : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Tags.Player)) return;

            var pm = other.gameObject.GetComponent<PlayerMechanics>();

            if (!pm) return;

            pm.SetWeapon(2);
        }
    }
}
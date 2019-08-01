using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    public class WeaponPickup : MonoBehaviour
    {
        public GameMode activeWhenGameModeIs;

        [Dropdown("weaponListValues")]
        public byte weapon;

        [Range(0, 300)]
        public float respawnTime = 15;

        public Transform pickupParent;

        private GameManager gameManager;
        private AudioSource audioSource;
        private readonly DropdownList<byte> weaponListValues = new DropdownList<byte>();

        private float timePickedUp;
        private bool pickedUp = false;

        private void Start()
        {
            if(!audioSource) audioSource = GetComponent<AudioSource>();

            gameManager = FindObjectOfType<GameManager>();

            Instantiate(activeWhenGameModeIs.weapons[weapon], pickupParent);
        }

        private void Update()
        {
            if (pickedUp && Time.time >= timePickedUp + respawnTime)
            {
                pickupParent.gameObject.SetActive(true);
                pickedUp = false;
            }

            if (gameManager.currentGameMode != null && gameManager.currentGameMode != activeWhenGameModeIs)
            {
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            if (!activeWhenGameModeIs) return;

            byte i = 0;
            foreach (var wep in activeWhenGameModeIs.weapons)
            {
                weaponListValues.Add(wep.name, i++);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (pickedUp && Time.time < timePickedUp + respawnTime) return;
            if (!other.gameObject.CompareTag(Tags.Player)) return;

            var pm = other.gameObject.GetComponent<PlayerMechanics>();

            if (!pm) return;

            pm.SetWeapon(weapon);
            pickupParent.gameObject.SetActive(false);
            timePickedUp = Time.time;
            pickedUp = true;

            if (audioSource) audioSource.Play();
        }
    }
}
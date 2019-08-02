using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class HealthPickup : MonoBehaviour
    {
        public GameMode activeWhenGameModeIs;

        public GameObject healthPack;

        [Range(0, 300)]
        public float respawnTime = 15;

        [Range(1, 200)]
        public byte healAmount = 50;

        public Transform pickupParent;

        private GameManager gameManager;
        private AudioSource audioSource;

        private float timePickedUp;
        private bool pickedUp = false;

        // Start is called before the first frame update
        void Start()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();

            gameManager = FindObjectOfType<GameManager>();

            Instantiate(healthPack, pickupParent);
        }

        // Update is called once per frame
        void Update()
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

            if (!pickedUp) pickupParent.transform.Rotate(new Vector3 (0, 1, 0), Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (pickedUp && Time.time < timePickedUp + respawnTime) return;

            var go = other.gameObject;
            var isLocal = go.CompareTag(Tags.Player);
            var isNetPlayer = go.layer == (int) Layer.Players;

            if (isLocal || isNetPlayer)
            {
                pickupParent.gameObject.SetActive(false);
                timePickedUp = Time.time;
                pickedUp = true;

                if (audioSource)
                    audioSource.Play();
            }

            if (!isLocal) return;

            var pm = other.gameObject.GetComponent<PlayerMechanics>();
            if (!pm) return;

            if (gameManager.currentGameMode.maxHealth <= pm.Health) return;

            pm.Health += healAmount;
            if(pm.Health > gameManager.currentGameMode.maxHealth)
            {
                pm.Health = gameManager.currentGameMode.maxHealth;
            }
        }
    }
}

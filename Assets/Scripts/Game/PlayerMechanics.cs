using System.Collections.Generic;
using NaughtyAttributes;
using UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMovement), typeof(NetworkPlayer))]
    public class PlayerMechanics : MonoBehaviour
    {
        public float spawnTime = 6f;

        [ReorderableList]
        public List<GameObject> disableOnDeath;

        public bool isAlive = true;

        private NetworkPlayer networkPlayer;
        private CharacterController characterController;
        private PlayerMovement playerMovement;
        private FirstPersonCamera playerCamera;
        private GameObject[] spawnPointList;

        private DeadOverlay uiDeadOverlay;
        private Transform spawnPoints;

        private float timeSpentDead;

        private void Start()
        {
            networkPlayer = GetComponent<NetworkPlayer>();
            characterController = GetComponent<CharacterController>();
            playerMovement = GetComponent<PlayerMovement>();
            playerCamera = GetComponentInChildren<FirstPersonCamera>();

            uiDeadOverlay = FindObjectOfType<GameManager>().deadOverlay;

            if (!spawnPoints)
                spawnPoints = FindObjectOfType<LevelInfo>().spawnPointParent.transform;

            spawnPointList = new GameObject[spawnPoints.childCount];

            for (var i = 0; i < spawnPoints.childCount; i++)
            {
                var spawn = spawnPoints.GetChild(i).gameObject;
                if (!spawn.CompareTag("Respawn")) continue;

                spawnPointList[i] = spawn;
            }

            RespawnPlayer();
        }

        private void Update()
        {
            if (!isAlive) timeSpentDead += Time.deltaTime;

            if (spawnTime < timeSpentDead)
            {
                timeSpentDead = 0;
                RespawnPlayer();
                isAlive = true;
            }
        }

        [Button]
        public void Kill()
        {
            isAlive = false;

            characterController.enabled = false;
            playerMovement.enabled = false;

            foreach (var go in disableOnDeath)
                go.SetActive(false);

            if (uiDeadOverlay)
            {
                uiDeadOverlay.respawnTime = spawnTime;
                uiDeadOverlay.gameObject.SetActive(true);
            }
        }

        public void RespawnPlayer()
        {
            var spawnPoint = spawnPointList[Random.Range(0, spawnPointList.Length)].transform;
            
            playerMovement.ResetMovement();
            networkPlayer.Teleport(spawnPoint.position, spawnPoint.rotation);

            foreach (var go in disableOnDeath)
                go.SetActive(true);

            playerMovement.enabled = true;
            characterController.enabled = true;

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(false);
        }
    }
}
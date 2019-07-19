using System.Collections.Generic;
using NaughtyAttributes;
using UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMovement))]
    public class PlayerMechanics : MonoBehaviour
    {
        private Transform spawnPoints;

        public float spawnTime = 10f;
        public float timeSpentDead;

        [ReorderableList]
        public List<GameObject> disableOnDeath;

        public bool isAlive = true;

        private CharacterController characterController;
        private PlayerMovement playerMovement;
        private FirstPersonCamera playerCamera;
        private GameObject[] spawnPointList;

        private DeadOverlay uiDeadOverlay;

        private void Start()
        {
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

        public void Kill()
        {
            isAlive = false;

            characterController.enabled = false;
            playerMovement.enabled = false;

            foreach (var go in disableOnDeath)
                go.SetActive(false);

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(true);
        }

        public void RespawnPlayer()
        {
            playerMovement.enabled = true;

            var spawnPoint = spawnPointList[Random.Range(0, spawnPointList.Length)].transform;
            playerMovement.ResetMovement();
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            playerCamera.SetAngles(new Vector2(spawnPoint.transform.rotation.eulerAngles.y, 0));

            foreach (var go in disableOnDeath)
                go.SetActive(true);

            characterController.enabled = true;

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using NaughtyAttributes;
using UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMovement), typeof(NetworkPlayer))]
    public class PlayerMechanics : MonoBehaviour
    {
        private readonly List<SpawnPoint> spawnPointList = new List<SpawnPoint>(8);
        private readonly List<SpawnPoint> freeSpawnPoints = new List<SpawnPoint>(8);

        public float spawnTime = 6f;

        [ReorderableList]
        public List<GameObject> disableOnDeath;

        private NetworkPlayer networkPlayer;
        private CharacterController characterController;
        private PlayerMovement playerMovement;

        private DeadOverlay uiDeadOverlay;
        private Transform spawnPointsParent;

        private float timeSpentDead;

        public bool IsAlive { get; set; } = true;

        private void Start()
        {
            networkPlayer = GetComponent<NetworkPlayer>();
            characterController = GetComponent<CharacterController>();
            playerMovement = GetComponent<PlayerMovement>();

            uiDeadOverlay = FindObjectOfType<GameManager>().deadOverlay;

            if (!spawnPointsParent)
                spawnPointsParent = FindObjectOfType<LevelInfo>().spawnPointParent.transform;

            freeSpawnPoints.Capacity = spawnPointList.Capacity = spawnPointsParent.childCount;

            for (var i = 0; i < spawnPointsParent.childCount; i++)
            {
                var spawn = spawnPointsParent.GetChild(i).gameObject.GetComponent<SpawnPoint>();
                if (!spawn) continue;

                spawnPointList.Add(spawn);
            }

            RespawnPlayer();
        }

        private void Update()
        {
            if (!IsAlive) timeSpentDead += Time.deltaTime;

            if (spawnTime < timeSpentDead)
            {
                timeSpentDead = 0;
                RespawnPlayer();
            }
        }

        [Button]
        public void Kill()
        {
            IsAlive = false;

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
            IsAlive = true;

            freeSpawnPoints.Clear();
            foreach (var spawn in spawnPointList)
                if (spawn.PlayersInSpawnZone == 0)
                    freeSpawnPoints.Add(spawn);

            var list = freeSpawnPoints.Count != 0 ? freeSpawnPoints : spawnPointList;
            var spawnTransform = list[Random.Range(0, list.Count)].playerSpawnPosition;

            playerMovement.ResetMovement();
            networkPlayer.Teleport(spawnTransform.position, spawnTransform.rotation);

            foreach (var go in disableOnDeath)
                go.SetActive(true);

            playerMovement.enabled = true;
            characterController.enabled = true;

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(false);
        }
    }
}
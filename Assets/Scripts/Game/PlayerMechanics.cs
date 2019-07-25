using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Game
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerMechanics : MonoBehaviour
    {
        private readonly List<SpawnPoint> spawnPointList = new List<SpawnPoint>(8);
        private readonly List<SpawnPoint> freeSpawnPoints = new List<SpawnPoint>(8);

        public GameSettings gameSettings;

        [ReorderableList]
        public List<GameObject> disableOnDeath;

        public Transform gunParent;
        public ParticleSystem deathEffect;

        private NetworkPlayer networkPlayer;
        private CharacterController characterController;
        private PlayerMovement playerMovement;

        private DeadOverlay uiDeadOverlay;
        private Transform spawnPointsParent;

        private float timeSpentDead;

        public byte CurrentWeaponId { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        public bool IsLocal => networkPlayer.IsLocalPlayer;
        public bool IsAlive { get; set; } = true;

        private void Start()
        {
            if (!gameSettings)
                throw new ArgumentException("gameSettings is required");

            networkPlayer = GetComponent<NetworkPlayer>();
            characterController = GetComponent<CharacterController>();
            playerMovement = GetComponent<PlayerMovement>();

            uiDeadOverlay = FindObjectOfType<GameManager>().deadOverlay;

            foreach (var wep in gameSettings.weapons)
            {
                var w = Instantiate(wep, gunParent);
                w.gameObject.SetActive(false);

                if (!IsLocal) continue;

                foreach (var mr in w.GetComponentsInChildren<MeshRenderer>())
                    mr.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            SetWeapon(0);

            if (!spawnPointsParent)
                spawnPointsParent = FindObjectOfType<LevelInfo>().spawnPointParent.transform;

            freeSpawnPoints.Capacity = spawnPointList.Capacity = spawnPointsParent.childCount;

            for (var i = 0; i < spawnPointsParent.childCount; i++)
            {
                var spawn = spawnPointsParent.GetChild(i).gameObject.GetComponent<SpawnPoint>();
                if (!spawn) continue;

                spawnPointList.Add(spawn);
            }

            Respawn();
        }

        private void Update()
        {
            if (!IsLocal) return;

            if (!IsAlive) timeSpentDead += Time.deltaTime;

            if (gameSettings.spawnTime < timeSpentDead)
            {
                timeSpentDead = 0;
                Respawn();
            }
        }

        public void SetWeapon(byte weaponId)
        {
            var wep = gunParent.GetChild(weaponId)?.GetComponent<Weapon>();

            if (wep != null)
                wep.gameObject.SetActive(true);

            if (CurrentWeapon != null)
                CurrentWeapon.gameObject.SetActive(false);

            CurrentWeapon = wep;
            CurrentWeaponId = weaponId;

            GetComponent<PlayerShooting>()?.SetWeapon(wep);
            GetComponent<PlayerAnimation>()?.SetWeapon(wep);
        }

        [Button]
        public void Kill()
        {
            IsAlive = false;

            if (deathEffect)
                deathEffect.Play();

            foreach (var go in disableOnDeath)
                go.SetActive(false);

            if (!IsLocal) return;

            characterController.enabled = false;
            playerMovement.enabled = false;

            if (uiDeadOverlay)
            {
                uiDeadOverlay.respawnTime = gameSettings.spawnTime;
                uiDeadOverlay.gameObject.SetActive(true);
            }
        }

        public void Respawn()
        {
            IsAlive = true;

            if (IsLocal)
            {
                freeSpawnPoints.Clear();
                foreach (var spawn in spawnPointList)
                    if (spawn.PlayersInSpawnZone == 0)
                        freeSpawnPoints.Add(spawn);

                var list = freeSpawnPoints.Count != 0 ? freeSpawnPoints : spawnPointList;
                var spawnTransform = list[Random.Range(0, list.Count)].playerSpawnPosition;

                playerMovement.ResetMovement();
                networkPlayer.Teleport(spawnTransform.position, spawnTransform.rotation);
            }

            foreach (var go in disableOnDeath)
                go.SetActive(true);

            if (!IsLocal) return;

            playerMovement.enabled = true;
            characterController.enabled = true;

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(false);
        }
    }
}
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Networking;
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

        [ReorderableList]
        public List<GameObject> disableOnDeath;

        public Transform gunParent;
        public Transform viewmodelParent;
        public ParticleSystem deathEffect;

        private NetworkPlayer networkPlayer;
        private CharacterController characterController;
        private PlayerMovement playerMovement;

        private GameMode gameMode;

        private DeadOverlay uiDeadOverlay;
        private Transform spawnPointsParent;

        private float timeSpentDead;

        public byte CurrentWeaponId { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        public bool IsLocal => networkPlayer.IsLocalPlayer;
        public bool IsAlive { get; set; }

        public byte Health { get; set; }

        private void Start()
        {
            gameMode = FindObjectOfType<GameManager>().currentGameMode;

            if (!gameMode)
                throw new ArgumentException("GameMode is required");

            Health = gameMode.maxHealth;

            networkPlayer = GetComponent<NetworkPlayer>();
            characterController = GetComponent<CharacterController>();
            playerMovement = GetComponent<PlayerMovement>();

            uiDeadOverlay = FindObjectOfType<GameManager>().deadOverlay;

            if (IsLocal)
                networkPlayer.Client.SelfHurt += TakeDamage;

            // Init weapons
            foreach (var wep in gameMode.weapons)
            {
                var w = Instantiate(wep, gunParent);
                w.gameObject.SetActive(false);

                if (!IsLocal) continue;

                Instantiate(wep.viewmodelPrefab, viewmodelParent).gameObject.SetActive(false);

                foreach (var mr in w.GetComponentsInChildren<MeshRenderer>())
                    mr.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            SetWeapon(0);

            // Init spawn points
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

        private void OnDestroy()
        {
            networkPlayer.Client.SelfHurt -= TakeDamage;
        }

        private void Update()
        {
            if (!IsLocal) return;

            if (!IsAlive) timeSpentDead += Time.deltaTime;

            if (gameMode.spawnTime < timeSpentDead)
            {
                timeSpentDead = 0;
                Respawn();
            }

            var wep = CurrentWeapon;

            if (wep != null && wep.maxAmmo > 0 && wep.Ammo == 0 && CurrentWeaponId > 0)
                SetWeapon(0);
        }

        public void SetWeapon(byte weaponId)
        {
            var wep = gunParent.GetChild(weaponId)?.GetComponent<Weapon>();

            if (CurrentWeapon != null)
                CurrentWeapon.gameObject.SetActive(false);

            if (wep != null)
                wep.gameObject.SetActive(true);

            if (IsLocal)
            {
                viewmodelParent.GetChild(CurrentWeaponId)?.gameObject.SetActive(false);
                viewmodelParent.GetChild(weaponId)?.gameObject.SetActive(true);
            }

            CurrentWeapon = wep;
            CurrentWeaponId = weaponId;

            if (CurrentWeapon != null)
                CurrentWeapon.Ammo = CurrentWeapon.maxAmmo;

            if (IsLocal)
                GetComponent<PlayerShooting>().viewmodel =
                    viewmodelParent.GetChild(weaponId)?.GetComponent<Viewmodel>();

            GetComponent<PlayerShooting>()?.SetWeapon(wep);
            networkPlayer.GetPlayerAnimation()?.SetWeapon(wep);
        }

        private void TakeDamage(object s, Client.DamageEventArgs dea)
        {
            if (!IsAlive) return;

            if (Health >= dea.Damage)
                Health -= dea.Damage;
            else
                Health = 0;

            if (Health != 0) return;

            Kill();
            networkPlayer.Client.OnDeath(dea.ShooterId);
        }

        [Button]
        public void Kill()
        {
            if (!IsAlive) return;

            IsAlive = false;

            if (deathEffect)
                deathEffect.Play();

            SetWeapon(0);

            foreach (var go in disableOnDeath)
                if (go)
                    go.SetActive(false);

            if (!IsLocal) return;

            if (!characterController) return;

            characterController.enabled = false;
            playerMovement.enabled = false;

            if (uiDeadOverlay)
            {
                uiDeadOverlay.respawnTime = gameMode.spawnTime;
                uiDeadOverlay.gameObject.SetActive(true);
            }

            SetWeapon(0);
        }

        public void Respawn()
        {
            IsAlive = true;
            Health = gameMode.maxHealth;

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
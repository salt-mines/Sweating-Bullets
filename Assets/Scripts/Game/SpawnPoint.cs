using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public class SpawnPoint : MonoBehaviour
    {
        private readonly HashSet<Collider> colliders = new HashSet<Collider>();

        public Transform playerSpawnPosition;

        public int PlayersInSpawnZone => colliders.Count;

        private void Awake()
        {
            if (!playerSpawnPosition)
                playerSpawnPosition = transform;
        }

        private void FixedUpdate()
        {
            colliders.Clear();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer != (int) Layer.Players) return;

            colliders.Add(other);
        }
    }
}
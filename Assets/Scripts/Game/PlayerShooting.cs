﻿using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerShooting : MonoBehaviour
    {
        public LayerMask hittableMask;

        public float range = 100f;
        public float rateOfFire = 1f;
        
        public LineRenderer linePrefab;
        public Weapon weapon;

        public float lineLifetime = 1f;

        private GameInput input;
        private NetworkPlayer player;
        private Camera fpsCamera;

        private float timeToFire;
        
        private readonly Vector3[] linePoints = new Vector3[2];

        // Start is called before the first frame update
        private void Start()
        {
            input = FindObjectOfType<GameInput>();
            player = GetComponent<NetworkPlayer>();
            fpsCamera = GetComponentInChildren<Camera>();

            timeToFire = rateOfFire;
        }

        // Update is called once per frame
        private void Update()
        {
            if (player.PlayerInfo.Alive && input.Fire && rateOfFire <= timeToFire)
            {
                timeToFire = 0;
                Shoot();
            }

            timeToFire += Time.deltaTime;
        }

        private void Shoot()
        {
            var cameraTransform = fpsCamera.transform;
            var from = cameraTransform.position;
            var to = cameraTransform.forward;

            if (!Physics.Raycast(from, to, out var hit, range,
                hittableMask))
            {
                SpawnLine(weapon.barrelPoint.position, to * range);
                return;
            }

            SpawnLine(weapon.barrelPoint.position, hit.point);

            if (hit.transform.gameObject.layer != 9) return;

            var targetNetPlayer = hit.transform.gameObject.GetComponent<NetworkPlayer>();
            if (targetNetPlayer)
                player.Shoot(targetNetPlayer);
        }

        private void SpawnLine(Vector3 from, Vector3 to)
        {
            var line = Instantiate(linePrefab);

            linePoints[0] = from;
            linePoints[1] = to;
            line.SetPositions(linePoints);
            Destroy(line.gameObject, lineLifetime);
        }
    }
}
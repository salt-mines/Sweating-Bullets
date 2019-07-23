using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game
{
    public class Pistol : Weapon
    {

        [Range(0.0f, 50.0f)]
        public float range = 100f;
        [Range(0.0f, 5.0f)]
        public float rateOfFire = 1f;

        public LineRenderer linePrefab;
        public float lineLifetime = 1f;

        private readonly Vector3[] linePoints = new Vector3[2];
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Shoot(Transform camera, NetworkPlayer player)
        {
            var cameraTransform = camera;
            var from = cameraTransform.position;
            var to = cameraTransform.forward;
            var barrel = barrelPoint.position;

            if (!Physics.Raycast(from, to, out var hit, range,
                Physics.AllLayers))
            {
                SpawnLine(barrel, to * range);
                player.Shoot(barrel, to * range);
                return;
            }

            SpawnLine(barrel, hit.point);
            player.Shoot(barrel, hit.point);

            //viewmodel.Shoot();

            if (hit.transform.gameObject.layer != (int)Layer.Players) return;

            var targetNetPlayer = hit.transform.gameObject.GetComponentInParent<NetworkPlayer>();
            if (targetNetPlayer)
                player.KillPlayer(targetNetPlayer);
        }

        protected override void SpawnLine(Vector3 from, Vector3 to)
        {
            var line = Instantiate(linePrefab);

            linePoints[0] = from;
            linePoints[1] = to;
            line.SetPositions(linePoints);
            Destroy(line.gameObject, lineLifetime);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Shotgun : Weapon
    {
        [Range(2,20)]
        public int numPellets = 6;
        [Range(0.0f, 20.0f)]
        public float horizontalSpread = 10.0f;
        [Range(0.0f, 20.0f)]
        public float verticalSpread = 10.0f;
        [Range(0.0f, 20.0f)]
        public float range;
        [Range(0.0f, 8.0f)]
        public float rateOfFire = 1f;

        public LineRenderer linePrefab;
        public float lineLifetime = 1f;

        private float timeToFire;
        private readonly Vector3[] linePoints = new Vector3[2];

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Shoot(Transform camera, NetworkPlayer player)
        {
            float spreadX;
            float spreadY;
            float rad;
            Vector3 from = camera.transform.position;

            for (int i = 0; i < numPellets; i++)
            {
                rad = Random.Range(0.0f, 360.0f) * Mathf.Rad2Deg;
                spreadX = Random.Range(0.0f, horizontalSpread / 2.0f) * Mathf.Cos(rad);
                spreadY = Random.Range(0.0f, verticalSpread / 2.0f) * Mathf.Sin(rad);

                Vector3 to = new Vector3(spreadX, spreadY, 0.0f) + camera.forward;

                if (Physics.Raycast(from, to, out var hit, range,
                Physics.AllLayers))
                {
                    SpawnLine(barrelPoint.position, hit.point);
                    //player.Shoot(weapon.barrelPoint.position, to * range);                    
                }
            }
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

using UnityEngine;

namespace Game
{
    public abstract class Weapon : MonoBehaviour
    {
        private readonly Vector3[] linePoints = new Vector3[2];

        [Header("Weapon")]
        [Tooltip("Rate of fire in rounds per second")]
        [Range(0.0f, 8.0f)]
        public float rateOfFire = 1f;

        [Tooltip("Which layers should bullets hit")]
        public LayerMask hittableMask = ~0;

        [Header("Positions")]
        public Transform barrelPoint;
        public Transform rightHandIKTarget;
        public Transform leftHandIKTarget;

        [Header("Trail")]
        public LineRenderer linePrefab;
        [Range(0f, 10f)]
        public float lineLifetime = 1f;

        protected float lastShot;

        public virtual bool CanShoot()
        {
            if (rateOfFire == 0f)
                return false;

            return Time.time > lastShot + 1f/rateOfFire;
        }

        public abstract void Shoot(Transform startPoint, NetworkPlayer player);

        public void SpawnLine(Vector3 from, Vector3 to)
        {
            var line = Instantiate(linePrefab);

            linePoints[0] = from;
            linePoints[1] = to;
            line.SetPositions(linePoints);
            Destroy(line.gameObject, lineLifetime);
        }
    }
}
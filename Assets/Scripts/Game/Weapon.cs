using UnityEngine;

namespace Game
{
    public abstract class Weapon : MonoBehaviour
    {
        public Transform barrelPoint;

        public Transform rightHandIKTarget;
        public Transform leftHandIKTarget;

        public abstract void Shoot(Transform camera, NetworkPlayer player);

        protected abstract void SpawnLine(Vector3 from, Vector3 to);
    }
}

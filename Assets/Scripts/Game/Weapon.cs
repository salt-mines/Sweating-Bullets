using DG.Tweening;
using UnityEngine;

namespace Game
{
    public abstract class Weapon : MonoBehaviour
    {
        protected static ObjectPool bulletPool = new ObjectPool();

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

        [Header("Visuals")]
        public Transform bulletParent;

        public TrailRenderer bulletEffect;
        public float bulletEffectTime = 0.1f;

        protected float lastShot;
        protected abstract int BulletReserve { get; }

        private void Start()
        {
            if (!bulletParent)
            {
                var li = FindObjectOfType<LevelInfo>();
                if (li && li.dynamicObjectParent)
                    bulletParent = li.dynamicObjectParent.transform;
            }

            bulletPool.Prefab = bulletEffect.gameObject;
            bulletPool.Parent = bulletParent;

            bulletPool.Capacity += BulletReserve;

            bulletPool.Fill();
        }

        public virtual bool CanShoot()
        {
            if (rateOfFire == 0f)
                return false;

            return Time.time > lastShot + 1f / rateOfFire;
        }

        public abstract void Shoot(NetworkPlayer player, Transform startPoint);

        public virtual void ShootVisual(NetworkPlayer player, Vector3 from, Vector3 to)
        {
            var bullet = bulletPool.GetOne().GetComponent<TrailRenderer>();
            bullet.transform.position = from;

            var cg = bullet.colorGradient;
            var cks = cg.colorKeys;

            for (var i = 0; i < cks.Length; i++)
            {
                cks[i].color = player.PlayerInfo.Color;
            }

            cg.SetKeys(cks, cg.alphaKeys);
            bullet.colorGradient = cg;

            bullet.gameObject.SetActive(true);

            var seq = DOTween.Sequence();
            seq.Append(bullet.transform.DOMove(to, bulletEffectTime))
                .AppendInterval(bullet.time)
                .AppendCallback(() => bullet.gameObject.SetActive(false));
        }
    }
}
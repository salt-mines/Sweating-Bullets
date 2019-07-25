using DG.Tweening;
using Effects;
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
        public float bulletEffectSpeed = 500f;

        [Header("Audio")]
        public AudioSource audioSource;

        protected float lastShot;
        protected abstract int BulletReserve { get; }

        private void Start()
        {
            if (!audioSource)
                audioSource = GetComponent<AudioSource>();

            if (!bulletParent)
            {
                var li = FindObjectOfType<LevelInfo>();
                if (li && li.dynamicObjectParent)
                    bulletParent = li.dynamicObjectParent.transform;
            }

            if (!bulletEffect) return;

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

        public virtual void ShootVisual(NetworkPlayer player, Vector3 from, Vector3 to, RaycastHit? hit = null)
        {
            if (!bulletEffect) return;

            var bulletTrail = bulletPool.GetOne().GetComponent<TrailRenderer>();
            bulletTrail.enabled = false;
            bulletTrail.Clear();
            bulletTrail.transform.position = from;

            // Set up player colored trail
            var cg = bulletTrail.colorGradient;
            var cks = cg.colorKeys;

            for (var i = 0; i < cks.Length; i++) cks[i].color = player.PlayerInfo.Color;

            cg.SetKeys(cks, cg.alphaKeys);
            bulletTrail.colorGradient = cg;

            bulletTrail.gameObject.SetActive(true);
            bulletTrail.enabled = true;

            var bullet = bulletTrail.GetComponent<Bullet>();

            // Hit effects
            var didHit = hit.HasValue && hit.Value.collider;
            var didHitPlayer = didHit && hit.Value.collider.gameObject.layer == (int) Layer.Players;

            // Set the emit rotation based on hit direction and normal
            if (didHit)
            {
                var fromDir = to - from;
                bullet.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(fromDir, hit.Value.normal));
            }

            bullet.transform.DOMove(to, bulletEffectSpeed).SetSpeedBased()
                .OnComplete(() =>
                {
                    if (didHitPlayer)
                        bullet.bloodSplatter.Play(false);
                    else if (didHit)
                        bullet.wallSplatter.Play(false);

                    DOTween.Sequence().PrependInterval(bulletTrail.time).AppendCallback(() =>
                    {
                        bullet.gameObject.SetActive(false);
                    });
                });
        }
    }
}
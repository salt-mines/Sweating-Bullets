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

        [Range(0, 100)]
        public byte damagePerBullet = 10;

        [Range(0, 255)]
        public byte maxAmmo;

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

        public byte Ammo { get; set; }

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

            Ammo = maxAmmo;
        }

        private void OnDestroy()
        {
            bulletPool.Clear();
        }

        public virtual bool CanShoot()
        {
            if (rateOfFire == 0f)
                return false;

            return (Ammo > 0 || maxAmmo == 0) && Time.time > lastShot + 1f / rateOfFire;
        }

        public virtual void Shoot(NetworkPlayer player, Transform startPoint)
        {
            if (maxAmmo > 0)
                Ammo--;

            lastShot = Time.time;
        }

        public void ShootEffect(NetworkPlayer player, Vector3 from, Vector3 to, RaycastHit? hit)
        {
            ShootEffect(player, from, to, new BulletInfo
            {
                hit = hit.HasValue && hit.Value.collider,
                hitPlayer = hit.HasValue && hit.Value.collider &&
                            hit.Value.collider.gameObject.layer == (int) Layer.Players,
                hitNormal = hit?.normal ?? Vector3.zero
            });
        }

        public virtual void ShootEffect(NetworkPlayer player, Vector3 from, Vector3 to, BulletInfo? hit = null)
        {
            if (audioSource) audioSource.Play();

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
            var didHit = hit.HasValue && hit.Value.hit;
            var didHitPlayer = didHit && hit.Value.hitPlayer;

            // Set the emit rotation based on hit direction and normal
            if (didHit)
            {
                var fromDir = to - from;
                bullet.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(fromDir, hit.Value.hitNormal));
            }

            bullet.transform.DOMove(to, bulletEffectSpeed).SetSpeedBased()
                .OnComplete(() =>
                {
                    if (didHitPlayer)
                    {
                        if (bullet.bloodSplatter)
                            bullet.bloodSplatter.Play(false);
                        if (bullet.audioSource && bullet.playerHitSound)
                            bullet.audioSource.PlayOneShot(bullet.playerHitSound);
                    }
                    else if (didHit)
                    {
                        if (bullet.wallSplatter)
                            bullet.wallSplatter.Play(false);
                        if (bullet.audioSource && bullet.wallHitSound)
                            bullet.audioSource.PlayOneShot(bullet.wallHitSound);
                    }

                    DOTween.Sequence().PrependInterval(bulletTrail.time).AppendCallback(() =>
                    {
                        bullet.gameObject.SetActive(false);
                    });
                });
        }
    }
}
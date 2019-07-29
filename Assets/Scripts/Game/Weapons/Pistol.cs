using UnityEngine;

namespace Game.Weapons
{
    public class Pistol : Weapon
    {
        [Header("Pistol")]
        [Range(0.0f, 100.0f)]
        public float range = 100f;

        protected override int BulletReserve => Mathf.CeilToInt(1 / rateOfFire) + 1;

        public override void Shoot(NetworkPlayer player, Transform startPoint)
        {
            lastShot = Time.time;

            var from = startPoint.position;
            var direction = startPoint.forward;

            var barrel = barrelPoint.position;

            var didHit = Physics.Raycast(from, direction, out var hit, range, hittableMask);
            var to = didHit ? hit.point : from + direction * range;

            player.Client.PlayerShootOne(barrel, to, damagePerBullet, hit);
            ShootEffect(player, barrel, to, hit);
        }
    }
}
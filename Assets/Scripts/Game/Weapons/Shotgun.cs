using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons
{
    public class Shotgun : Weapon
    {
        [Header("Shotgun")]
        [Range(2, 20)]
        public int numPellets = 6;

        [Range(0.0f, 2.0f)]
        public float accuracy = 0.5f;

        [Range(0.0f, 20.0f)]
        public float range = 8f;

        protected override int BulletReserve => Mathf.CeilToInt(numPellets * 1 / rateOfFire) + 1;

        private BulletInfo[] hits;

        private new void Start()
        {
            base.Start();
            hits = new BulletInfo[numPellets];
        }

        public override void Shoot(NetworkPlayer player, Transform startPoint)
        {
            base.Shoot(player, startPoint);
            var from = startPoint.position;

            for (var i = 0; i < numPellets; i++)
            {

                
                float randomOffset_x = UnityEngine.Random.Range(-(1 - accuracy), 1 - accuracy);
                float randomOffset_y = UnityEngine.Random.Range(-(1 - accuracy), 1 - accuracy);
                float randomOffset_z = UnityEngine.Random.Range(-(1 - accuracy), 1 - accuracy);

                Vector3 direction = transform.forward;

                direction.x += randomOffset_x;
                direction.y += randomOffset_y;
                direction.z += randomOffset_z;
                var endPoint = startPoint.position + direction * range;

                var didHit = Physics.Raycast(from, direction, out var hit, range, hittableMask);

                if (didHit)
                    endPoint = hit.point;

                ShootEffect(player, barrelPoint.position, endPoint, hit);

                hits[i] = BulletInfo.From(endPoint, damagePerBullet, hit);
            }

            player.Client.ShootMultiple(barrelPoint.position, hits);
        }
    }
}
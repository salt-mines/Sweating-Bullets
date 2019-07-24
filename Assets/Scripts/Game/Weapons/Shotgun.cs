using UnityEngine;

namespace Game.Weapons
{
    public class Shotgun : Weapon
    {
        [Header("Shotgun")]
        [Range(2, 20)]
        public int numPellets = 6;

        [Range(0.0f, 2.0f)]
        public float horizontalSpread = 1.0f;

        [Range(0.0f, 2.0f)]
        public float verticalSpread = 1.0f;

        [Range(0.0f, 20.0f)]
        public float range = 8f;

        protected override int BulletReserve => Mathf.CeilToInt(numPellets * 1 / rateOfFire) + 1;

        public override void Shoot(Transform startPoint, NetworkPlayer player)
        {
            lastShot = Time.time;

            float spreadX;
            float spreadY;
            float rad;
            var from = startPoint.position;

            for (var i = 0; i < numPellets; i++)
            {
                rad = Random.Range(0.0f, 360.0f) * Mathf.Rad2Deg;
                spreadX = Random.Range(0.0f, horizontalSpread / 2.0f) * Mathf.Cos(rad);
                spreadY = Random.Range(0.0f, verticalSpread / 2.0f) * Mathf.Sin(rad);

                var direction = new Vector3(spreadX, spreadY, 0.0f) + startPoint.forward;
                var endPoint = startPoint.position + direction * range;

                var didHit = Physics.Raycast(from, direction, out var hit, range, hittableMask);

                if (didHit)
                    endPoint = hit.point;

                ShootVisual(barrelPoint.position, endPoint);
                //player.Shoot(weapon.barrelPoint.position, to * range);
            }
        }
    }
}
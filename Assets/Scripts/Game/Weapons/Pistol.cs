using UnityEngine;

namespace Game.Weapons
{
    public class Pistol : Weapon
    {
        [Header("Pistol")]
        [Range(0.0f, 100.0f)]
        public float range = 100f;

        public override void Shoot(Transform startPoint, NetworkPlayer player)
        {
            lastShot = Time.time;

            var from = startPoint.position;
            var direction = startPoint.forward;
            var to = from + direction * range;

            var barrel = barrelPoint.position;

            var didHit = Physics.Raycast(from, direction, out var hit, range, Physics.AllLayers);

            if (didHit)
                to = hit.point;

            SpawnLine(barrel, to);
            player.Shoot(barrel, to);

            if (!didHit)
                return;

            if (hit.transform.gameObject.layer != (int) Layer.Players) return;

            var targetNetPlayer = hit.transform.gameObject.GetComponentInParent<NetworkPlayer>();
            if (targetNetPlayer)
                player.KillPlayer(targetNetPlayer);
        }
    }
}
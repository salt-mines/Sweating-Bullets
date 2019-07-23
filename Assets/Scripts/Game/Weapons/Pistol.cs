using UnityEngine;

namespace Game.Weapons
{
    public class Pistol : Weapon
    {
        [Header("Pistol")]
        [Range(0.0f, 100.0f)]
        public float range = 100f;

        [Header("Particles")]
        public ParticleSystem bulletParticleSystem;

        public override void Shoot(Transform startPoint, NetworkPlayer player)
        {
            lastShot = Time.time;

            var from = startPoint.position;
            var direction = startPoint.forward;

            var barrel = barrelPoint.position;

            var didHit = Physics.Raycast(from, direction, out var hit, range, hittableMask);
            var to = didHit ? hit.point : from + direction * range;

            ShootVisual(barrel, to);
            player.Shoot(barrel, to);

            if (!didHit)
                return;

            if (hit.transform.gameObject.layer != (int) Layer.Players) return;

            var targetNetPlayer = hit.transform.gameObject.GetComponentInParent<NetworkPlayer>();
            if (targetNetPlayer)
                player.KillPlayer(targetNetPlayer);
        }

        public override void ShootVisual(Vector3 from, Vector3 to)
        {
            var ps = bulletParticleSystem;

            ps.transform.position = from;
            ps.transform.LookAt(to);
            var main = ps.main;
            main.startLifetime = Mathf.Clamp(Vector3.Distance(from, to) / main.startSpeed.constant, 0.05f, 3f);

            ps.Emit(1);
        }
    }
}
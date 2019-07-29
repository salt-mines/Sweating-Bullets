using Lidgren.Network;
using Networking;
using UnityEngine;

namespace Game
{
    public struct BulletInfo
    {
        public Vector3 to;
        public bool hit;
        public bool hitPlayer;
        public byte victimId;
        public byte damage;
        public Vector3 hitNormal;

        public static BulletInfo Read(NetIncomingMessage msg)
        {
            return new BulletInfo
            {
                to = msg.ReadVector3(),
                hit = msg.ReadBoolean(),
                hitPlayer = msg.ReadBoolean(),
                victimId = msg.ReadByte(),
                damage = msg.ReadByte(),
                hitNormal = msg.ReadVector3()
            };
        }

        public static BulletInfo[] ReadAll(NetIncomingMessage msg)
        {
            var count = msg.ReadByte();
            var bullets = new BulletInfo[count];
            for (var i = 0; i < count; i++)
            {
                bullets[i] = Read(msg);
            }

            return bullets;
        }

        public void Write(NetOutgoingMessage msg)
        {
            msg.Write(to);
            msg.Write(hit);
            msg.Write(hitPlayer);
            msg.Write(victimId);
            msg.Write(damage);
            msg.Write(hitNormal);
        }

        public static BulletInfo From(byte playerId, Vector3 from, Vector3 to, byte damage, RaycastHit hit)
        {
            var didHit = hit.collider != null;
            var didHitPlayer = didHit && hit.collider.gameObject.layer == (int) Layer.Players;
            var victim = didHitPlayer ? hit.collider.gameObject.GetComponentInParent<NetworkPlayer>().Id : (byte)255;
            var normal = hit.normal;

            return new BulletInfo
            {
                to = to,
                hit = didHit,
                hitPlayer = didHitPlayer,
                victimId = victim,
                damage = damage,
                hitNormal = normal,
            };
        }
    }
}
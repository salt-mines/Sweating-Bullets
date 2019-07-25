using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    public struct PlayerState : IPacket
    {
        public PacketType Type => PacketType.PlayerMove;
        public int SequenceChannel => 2;

        public byte playerId;

        public Vector3 position;
        public Vector3 velocity;
        public Vector2 viewAngles;

        public byte health;
        public byte weapon;

        public bool teleported;
        public bool alive;

        public static PlayerState Lerp(PlayerState s0, PlayerState s1, float ratio)
        {
            if (s1.teleported)
                return s1;

            return new PlayerState
            {
                playerId = s0.playerId,
                position = Vector3.Lerp(s0.position, s1.position, ratio),
                velocity = Vector3.Lerp(s0.velocity, s1.velocity, ratio),
                viewAngles = Vector2.Lerp(s0.viewAngles, s1.viewAngles, ratio),
                health = s1.health,
                weapon = s1.weapon,
                alive = s0.alive && s1.alive
            };
        }

        public static PlayerState Read(NetIncomingMessage msg)
        {
            return new PlayerState
            {
                playerId = msg.ReadByte(),
                position = msg.ReadVector3(),
                velocity = msg.ReadVector3(),
                viewAngles = msg.ReadVector2(),
                health = msg.ReadByte(),
                weapon = msg.ReadByte(),
                teleported = msg.ReadBoolean(),
                alive = msg.ReadBoolean()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            msg.Write(playerId);
            msg.Write(position);
            msg.Write(velocity);
            msg.Write(viewAngles);
            msg.Write(health);
            msg.Write(weapon);
            msg.Write(teleported);
            msg.Write(alive);
        }
    }

    public struct TimedPlayerState
    {
        public float time;
        public PlayerState state;
    }
}
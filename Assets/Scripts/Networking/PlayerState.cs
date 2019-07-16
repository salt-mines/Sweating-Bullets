using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    public struct PlayerState : IPacket
    {
        public PacketType Type => PacketType.PlayerMove;

        public byte playerId;

        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;

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
                rotation = Quaternion.Lerp(s0.rotation, s1.rotation, ratio),
                alive = s0.alive && s1.alive
            };
        }

        public static PlayerState Read(NetIncomingMessage msg)
        {
            return new PlayerState
            {
                playerId = msg.ReadByte(),
                position = BufferUtils.ReadVector3(msg),
                velocity = BufferUtils.ReadVector3(msg),
                rotation = BufferUtils.ReadQuaternion(msg),
                teleported = msg.ReadBoolean(),
                alive = msg.ReadBoolean()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId, position, velocity, rotation, teleported, alive);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId, Vector3 position,
            Vector3 velocity, Quaternion rotation, bool teleported, bool alive)
        {
            msg.Write(playerId);
            BufferUtils.Write(msg, position);
            BufferUtils.Write(msg, velocity);
            BufferUtils.Write(msg, rotation);
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
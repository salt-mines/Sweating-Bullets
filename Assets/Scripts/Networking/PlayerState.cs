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
        public Vector2 viewAngles;

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
                viewAngles = BufferUtils.ReadVector2(msg),
                teleported = msg.ReadBoolean(),
                alive = msg.ReadBoolean()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            msg.Write(playerId);
            BufferUtils.Write(msg, position);
            BufferUtils.Write(msg, velocity);
            BufferUtils.Write(msg, viewAngles);
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
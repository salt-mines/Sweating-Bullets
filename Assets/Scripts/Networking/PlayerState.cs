using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    public struct PlayerState
    {
        public byte playerId;

        public Vector3 position;
        public Quaternion rotation;

        public static PlayerState Lerp(PlayerState s0, PlayerState s1, float ratio)
        {
            return new PlayerState
            {
                playerId = s0.playerId,
                position = Vector3.Lerp(s0.position, s1.position, ratio),
                rotation = Quaternion.Lerp(s0.rotation, s1.rotation, ratio)
            };
        }

        public static PlayerState Read(NetIncomingMessage msg)
        {
            return new PlayerState
            {
                playerId = msg.ReadByte(),
                position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
                rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat())
            };
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId, position, rotation);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId, Vector3 position,
            Quaternion rotation)
        {
            msg.Write(playerId);
            msg.Write(position.x);
            msg.Write(position.y);
            msg.Write(position.z);
            msg.Write(rotation.x);
            msg.Write(rotation.y);
            msg.Write(rotation.z);
            msg.Write(rotation.w);
            return msg;
        }
    }

    public struct TimedPlayerState
    {
        public float time;
        public PlayerState state;
    }
}
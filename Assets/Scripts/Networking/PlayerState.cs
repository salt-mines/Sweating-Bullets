using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    public struct PlayerState
    {
        public byte playerId;

        public Vector3 position;
        public Quaternion rotation;

        public bool alive;

        public static PlayerState Lerp(PlayerState s0, PlayerState s1, float ratio)
        {
            return new PlayerState
            {
                playerId = s0.playerId,
                position = Vector3.Lerp(s0.position, s1.position, ratio),
                rotation = Quaternion.Lerp(s0.rotation, s1.rotation, ratio),
                alive = s0.alive && s1.alive
            };
        }

        public static PlayerState Read(NetIncomingMessage msg)
        {
            return new PlayerState
            {
                playerId = msg.ReadByte(),
                position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
                rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
                alive = msg.ReadBoolean()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId, position, rotation, alive);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId, Vector3 position,
            Quaternion rotation, bool alive)
        {
            msg.Write(playerId);
            msg.Write(position.x);
            msg.Write(position.y);
            msg.Write(position.z);
            msg.Write(rotation.x);
            msg.Write(rotation.y);
            msg.Write(rotation.z);
            msg.Write(rotation.w);
            msg.Write(alive);
        }
    }

    public struct TimedPlayerState
    {
        public float time;
        public PlayerState state;
    }
}
using Lidgren.Network;
using UnityEngine;

namespace Networking.Packets
{
    public enum PacketType : byte
    {
        Connected = 0,
        PlayerConnected = 10,
        PlayerDisconnected = 11,
        PlayerMove = 12,
        PlayerShoot = 13,
        PlayerDeath = 14,
        WorldState = 20
    }

    public interface IPacket
    {
        PacketType Type { get; }

        void Write(NetOutgoingMessage msg);
    }

    public static class Packet
    {
        public static NetOutgoingMessage Write<T>(NetPeer peer, T packet) where T : IPacket
        {
            var msg = peer.CreateMessage();
            msg.Write((byte) packet.Type);
            packet.Write(msg);
            return msg;
        }
    }

    public struct Connected : IPacket
    {
        public PacketType Type => PacketType.Connected;

        public byte playerId;
        public byte maxPlayers;

        public string levelName;

        public static Connected Read(NetIncomingMessage msg)
        {
            return new Connected
            {
                playerId = msg.ReadByte(),
                maxPlayers = msg.ReadByte(),
                levelName = msg.ReadString()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId, maxPlayers, levelName);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId, byte maxPlayers, string levelName)
        {
            msg.Write(playerId);
            msg.Write(maxPlayers);
            msg.Write(levelName);
        }
    }

    public struct PlayerConnected : IPacket
    {
        public PacketType Type => PacketType.PlayerConnected;

        public byte playerId;

        public static PlayerConnected Read(NetIncomingMessage msg)
        {
            return new PlayerConnected
            {
                playerId = msg.ReadByte()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId)
        {
            msg.Write(playerId);
        }
    }

    public struct PlayerDisconnected : IPacket
    {
        public PacketType Type => PacketType.PlayerDisconnected;

        public byte playerId;

        public static PlayerDisconnected Read(NetIncomingMessage msg)
        {
            return new PlayerDisconnected
            {
                playerId = msg.ReadByte()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId)
        {
            msg.Write(playerId);
        }
    }

    public struct PlayerMove : IPacket
    {
        public PacketType Type => PacketType.PlayerMove;

        public byte playerId;

        public Vector3 position;
        public Quaternion rotation;

        public bool alive;

        public static PlayerMove Read(NetIncomingMessage msg)
        {
            return new PlayerMove
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

    public struct PlayerShoot : IPacket
    {
        public PacketType Type => PacketType.PlayerShoot;

        public byte shooterId;
        public byte targetId;

        public static PlayerShoot Read(NetIncomingMessage msg)
        {
            return new PlayerShoot
            {
                shooterId = msg.ReadByte(),
                targetId = msg.ReadByte()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, shooterId, targetId);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId, byte targetId)
        {
            msg.Write(playerId);
            msg.Write(targetId);
        }
    }

    public struct PlayerDeath : IPacket
    {
        public PacketType Type => PacketType.PlayerDeath;

        public byte playerId;
        public byte killerId;

        public static PlayerDeath Read(NetIncomingMessage msg)
        {
            return new PlayerDeath
            {
                playerId = msg.ReadByte(),
                killerId = msg.ReadByte()
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, playerId, killerId);
        }

        public static void Write(NetOutgoingMessage msg, byte playerId, byte killerId)
        {
            msg.Write(playerId);
            msg.Write(killerId);
        }
    }

    public struct WorldState : IPacket
    {
        public PacketType Type => PacketType.WorldState;

        public PlayerState?[] worldState;

        public static WorldState Read(NetIncomingMessage msg)
        {
            var length = msg.ReadByte();
            var worldState = new PlayerState?[length];

            for (var i = 0; i < length; i++)
            {
                if (!msg.ReadBoolean()) continue;

                var ps = PlayerState.Read(msg);
                worldState[ps.playerId] = ps;
            }

            return new WorldState
            {
                worldState = worldState
            };
        }

        public void Write(NetOutgoingMessage msg)
        {
            Write(msg, worldState);
        }

        public static void Write(NetOutgoingMessage msg, PlayerState?[] worldState)
        {
            msg.Write((byte) worldState.Length);
            foreach (var state in worldState)
                if (!state.HasValue)
                {
                    msg.Write(false);
                }
                else
                {
                    msg.Write(true);
                    state.Value.Write(msg);
                }
        }
    }
}
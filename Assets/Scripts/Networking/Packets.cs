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
            msg.Write(playerId);
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
            msg.Write(shooterId);
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
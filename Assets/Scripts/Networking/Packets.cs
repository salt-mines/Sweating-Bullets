using Lidgren.Network;
using System;
using System.Collections.Generic;
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
        WorldState = 20,
    }

    public interface IPacket
    {
        PacketType Type { get; }

        IPacket Read(NetIncomingMessage msg);
        NetOutgoingMessage Write(NetOutgoingMessage msg);
    }

    public static class Packet
    {
        public static IPacket GetPacketFromType(PacketType type)
        {
            switch(type)
            {
                case PacketType.Connected:
                    return new Connected();
                case PacketType.PlayerConnected:
                    return new PlayerConnected();
                case PacketType.PlayerDisconnected:
                    return new PlayerDisconnected();
                case PacketType.PlayerMove:
                    return new PlayerMove();
                case PacketType.PlayerShoot:
                    return new PlayerShoot();
                case PacketType.WorldState:
                    return new WorldState();
                default:
                    throw new NotImplementedException("Packet not implemented: " + type);
            }
        }
        
        public static NetOutgoingMessage Write<T>(NetPeer peer, T packet) where T : IPacket
        {
            var msg = peer.CreateMessage();
            msg.Write((byte)packet.Type);
            packet.Write(msg);
            return msg;
        }
    }

    public struct Connected : IPacket
    {
        public PacketType Type => PacketType.Connected;

        public byte playerId;
        public byte maxPlayers;

        public IPacket Read(NetIncomingMessage msg)
        {
            playerId = msg.ReadByte();
            maxPlayers = msg.ReadByte();
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId, maxPlayers);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId, byte maxPlayers)
        {
            msg.Write(playerId);
            msg.Write(maxPlayers);
            return msg;
        }
    }

    public struct PlayerConnected : IPacket
    {
        public PacketType Type => PacketType.PlayerConnected;

        public byte playerId;

        public IPacket Read(NetIncomingMessage msg)
        {
            playerId = msg.ReadByte();
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId)
        {
            msg.Write(playerId);
            return msg;
        }
    }

    public struct PlayerDisconnected : IPacket
    {
        public PacketType Type => PacketType.PlayerDisconnected;

        public byte playerId;

        public IPacket Read(NetIncomingMessage msg)
        {
            playerId = msg.ReadByte();
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId)
        {
            msg.Write(playerId);
            return msg;
        }
    }

    public struct PlayerMove : IPacket
    {
        public PacketType Type => PacketType.PlayerMove;

        public byte playerId;

        public Vector3 position;
        public Quaternion rotation;

        public IPacket Read(NetIncomingMessage msg)
        {
            playerId = msg.ReadByte();
            position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId, position, rotation);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId, Vector3 position, Quaternion rotation)
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

    public struct PlayerShoot : IPacket
    {
        public PacketType Type => PacketType.PlayerShoot;

        public byte playerId;
        public byte targetId;

        public IPacket Read(NetIncomingMessage msg)
        {
            playerId = msg.ReadByte();
            targetId = msg.ReadByte();
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, playerId, targetId);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte playerId, byte targetId)
        {
            msg.Write(playerId);
            msg.Write(targetId);
            return msg;
        }
    }

    public struct WorldState : IPacket
    {
        public PacketType Type => PacketType.WorldState;

        public PlayerState?[] worldState;
        
        public IPacket Read(NetIncomingMessage msg)
        {
            var length = msg.ReadByte();
            worldState = new PlayerState?[length];

            for (int i = 0; i < length; i++)
            {
                var exists = msg.ReadBoolean();
                if (exists)
                {
                    var ps = PlayerState.Read(msg);
                    worldState[ps.playerId] = ps;
                }
            }

            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, worldState);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, PlayerState?[] worldState)
        {
            msg.Write(worldState.Length);
            foreach (PlayerState? state in worldState)
            {
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
            return msg;
        }
    }
}

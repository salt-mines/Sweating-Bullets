﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Packets
{
    public enum PacketType : byte
    {
        Connected = 0,
        PlayerMove = 10,
        WorldState = 20,
        EntityState = 21,
    }

    public interface IPacket
    {
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
                case PacketType.PlayerMove:
                    return new PlayerMove();
                case PacketType.WorldState:
                    return new WorldState();
                case PacketType.EntityState:
                    return new PlayerState();
                default:
                    throw new NotImplementedException("Packet not implemented: " + type);
            }
        }
    }

    public struct Connected : IPacket
    {
        static readonly PacketType TYPE = PacketType.Connected;

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

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte hostId)
        {
            msg.Write((byte)TYPE);
            msg.Write(hostId);
            return msg;
        }
    }

    public struct PlayerMove : IPacket
    {
        static readonly PacketType TYPE = PacketType.PlayerMove;

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
            msg.Write((byte)TYPE);
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

    public struct WorldState : IPacket
    {
        static readonly PacketType TYPE = PacketType.WorldState;

        public List<PlayerState> worldState;
        
        public IPacket Read(NetIncomingMessage msg)
        {
            var length = msg.ReadInt32();
            worldState = new List<PlayerState>(length);

            for (int i = 0; i < length; i++)
            {
                msg.ReadByte(); // Read extra Packet ID
                worldState.Add((PlayerState)new PlayerState().Read(msg));
            }

            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, worldState);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, List<PlayerState> worldState)
        {
            msg.Write((byte)TYPE);
            msg.Write(worldState.Count);
            foreach (var state in worldState)
            {
                state.Write(msg);
            }
            return msg;
        }
    }
    
    public struct PlayerState : IPacket
    {
        static readonly PacketType TYPE = PacketType.EntityState;

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
            msg.Write((byte)TYPE);
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
}
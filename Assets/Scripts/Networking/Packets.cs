using Lidgren.Network;
using System;
using UnityEngine;

namespace Networking.Packets
{
    public enum PacketType : byte
    {
        Connected = 0,
        PlayerMove = 10,
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
                default:
                    throw new NotImplementedException("Packet not implemented: " + type);
            }
        }
    }

    public struct Connected : IPacket
    {
        static readonly PacketType TYPE = PacketType.Connected;

        public byte hostId;

        public IPacket Read(NetIncomingMessage msg)
        {
            hostId = msg.ReadByte();
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, hostId);
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

        public byte hostId;

        public Vector3 position;
        public Quaternion rotation;

        public IPacket Read(NetIncomingMessage msg)
        {
            hostId = msg.ReadByte();
            position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            return this;
        }

        public NetOutgoingMessage Write(NetOutgoingMessage msg)
        {
            return Write(msg, hostId, position, rotation);
        }

        public static NetOutgoingMessage Write(NetOutgoingMessage msg, byte hostId, Vector3 position, Quaternion rotation)
        {
            msg.Write((byte)TYPE);
            msg.Write(hostId);
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

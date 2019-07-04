using Lidgren.Network;

namespace Networking.Packets
{
    public enum PacketType : byte
    {
        Connected,
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
                default:
                    return null;
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


}

using UnityEngine;
using Lidgren.Network;
using Networking.Packets;

namespace Networking
{
    class Client : Peer
    {
        private NetClient client;

        public byte HostId { get; private set; }

        public Client()
        {
            peer = client = new NetClient(peerConfig);
        }

        public void Connect(string host, int port)
        {
            client.Connect(host, port);
        }

        protected override void OnDataMessage(NetIncomingMessage msg)
        {
            PacketType type = (PacketType)msg.ReadByte();
            var packet = Packet.GetPacketFromType(type);

            Debug.LogFormat("Packet [{0}]: {1}", this, type);

            switch(type)
            {
                case PacketType.Connected:
                    packet.Read(msg);
                    HostId = ((Connected)packet).hostId;
                    break;
            }
        }
    }
}

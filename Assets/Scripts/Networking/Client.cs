using UnityEngine;
using Lidgren.Network;
using Networking.Packets;
using System.Collections.Generic;

namespace Networking
{
    class Client : Peer
    {
        private NetClient client;

        public byte HostId { get; private set; }
        public GameObject LocalActor { get; set; }
        private PlayerMove movePacket = new PlayerMove();

        public NetworkActor NetworkPlayerPrefab { get; set; }

        private readonly Dictionary<byte, NetworkActor> networkActors = new Dictionary<byte, NetworkActor>();

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

            switch (type)
            {
                case PacketType.Connected:
                    packet.Read(msg);
                    HostId = ((Connected)packet).hostId;
                    movePacket.hostId = HostId;
                    break;
                case PacketType.PlayerMove:
                    UpdateNetworkActor((PlayerMove)packet.Read(msg));
                    break;
            }
        }

        public void Update()
        {
            if (!LocalActor) return;

            movePacket.position = LocalActor.transform.position;
            movePacket.rotation = LocalActor.transform.rotation;
        }

        private void UpdateNetworkActor(PlayerMove packet)
        {
            if (!networkActors.TryGetValue(packet.hostId, out var actor))
            {
                actor = Object.Instantiate(NetworkPlayerPrefab);
                networkActors.Add(packet.hostId, actor);
            }

            actor.transform.SetPositionAndRotation(packet.position, packet.rotation);
        }
    }
}

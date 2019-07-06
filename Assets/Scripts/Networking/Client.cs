using System.Collections.Generic;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    internal class Client : Peer
    {
        private readonly Dictionary<byte, NetworkActor> networkActors = new Dictionary<byte, NetworkActor>();
        private readonly NetClient client;

        public Client()
        {
            peer = client = new NetClient(peerConfig);
        }

        public byte PlayerId { get; private set; }
        private GameObject LocalActor { get; set; }

        public GameObject LocalPlayerPrefab { get; set; }

        public void Connect(string host, int port)
        {
            client.Connect(host, port);
        }

        protected override void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();
            var packet = Packet.GetPacketFromType(type).Read(msg);

            //Debug.LogFormat("Packet [{0}]: {1}", this, type);

            switch (type)
            {
                case PacketType.Connected:
                    OnConnected((Connected) packet);
                    break;
                case PacketType.WorldState:
                    GetWorldState((WorldState) packet);
                    break;
            }
        }

        public override void FixedUpdate()
        {
            if (!LocalActor) return;
            if (client.ConnectionStatus != NetConnectionStatus.Connected) return;

            client.SendMessage(PlayerMove.Write(
                client.CreateMessage(),
                PlayerId,
                LocalActor.transform.position,
                LocalActor.transform.rotation
            ), NetDeliveryMethod.UnreliableSequenced);
        }

        private void OnConnected(Connected packet)
        {
            PlayerId = packet.playerId;

            LocalActor = Object.Instantiate(LocalPlayerPrefab);
        }

        private void UpdatePlayerState(PlayerState state)
        {
            if (!networkActors.TryGetValue(state.playerId, out var actor))
            {
                Debug.LogFormat("Player {0} at {1}, rotated {2}", state.playerId, state.position, state.rotation);
                actor = Object.Instantiate(NetworkPlayerPrefab);
                networkActors.Add(state.playerId, actor);
            }

            actor.transform.SetPositionAndRotation(state.position, state.rotation);
        }

        private void GetWorldState(WorldState state)
        {
            foreach (var e in state.worldState) UpdatePlayerState(e);
        }
    }
}
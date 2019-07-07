using System;
using System.Collections.Generic;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Networking
{
    internal class ListenServer : Peer
    {
        private readonly List<ClientInfo> connectedClients = new List<ClientInfo>();
        private readonly Dictionary<byte, NetworkActor> networkActors = new Dictionary<byte, NetworkActor>();
        private readonly NetServer server;
        private readonly List<PlayerState> worldState = new List<PlayerState>();

        private byte nextHostId;

        public ListenServer()
        {
            peerConfig.Port = AppPort;
            peer = server = new NetServer(peerConfig);
            PlayerId = 0;
        }

        public byte PlayerId { get; }
        private GameObject LocalActor { get; set; }

        public GameObject LocalPlayerPrefab { get; set; }

        public void CreateLocalPlayer()
        {
            LocalActor = Object.Instantiate(LocalPlayerPrefab);
            var na = LocalActor.GetComponent<NetworkActor>();
            na.PlayerId = PlayerId;
            networkActors.Add(PlayerId, na);
        }

        public ClientInfo GetClientInfo(NetConnection connection)
        {
            if (connection.Tag == null) connection.Tag = new ClientInfo(++nextHostId, connection);

            return (ClientInfo) connection.Tag;
        }

        public ClientInfo GetClientInfo(NetIncomingMessage msg)
        {
            return GetClientInfo(msg.SenderConnection);
        }

        public void SendToOne(NetOutgoingMessage msg, NetConnection connection, NetDeliveryMethod method)
        {
            server.SendMessage(msg, connection, method);
        }

        public void SendToAll(NetOutgoingMessage msg, NetDeliveryMethod method)
        {
            foreach (var client in connectedClients) SendToOne(msg, client.Connection, method);
        }

        public void SendToAllButOne(NetOutgoingMessage msg, NetConnection exceptThis, NetDeliveryMethod method)
        {
            foreach (var client in connectedClients)
            {
                if (client.Connection == exceptThis) continue;

                SendToOne(msg, client.Connection, method);
            }
        }

        protected override void OnStatusMessage(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus) msg.ReadByte();
            var client = GetClientInfo(msg);
            switch (newStatus)
            {
                case NetConnectionStatus.Connected:
                    PlayerConnected(client);
                    break;
                case NetConnectionStatus.Disconnecting:
                case NetConnectionStatus.Disconnected:
                    PlayerDisconnected(client);
                    break;
            }
        }

        private void PlayerConnected(ClientInfo client)
        {
            Debug.LogFormat("Conn [{0}]: {1}", this, client.PlayerId);

            connectedClients.Add(client);
            var actor = Object.Instantiate(NetworkPlayerPrefab);
            actor.PlayerId = client.PlayerId;
            networkActors.Add(client.PlayerId, actor);

            SendToOne(Connected.Write(server.CreateMessage(), client.PlayerId), client.Connection,
                NetDeliveryMethod.ReliableSequenced);
        }

        private void PlayerDisconnected(ClientInfo client)
        {
            Debug.LogFormat("DC [{0}]: {1}", this, client.PlayerId);

            connectedClients.Remove(client);
            networkActors.Remove(client.PlayerId);
        }

        protected override void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();
            var packet = Packet.GetPacketFromType(type).Read(msg);
            var sender = GetClientInfo(msg).PlayerId;

            //Debug.LogFormat("Packet [{0}]: {1}", this, type);

            switch (type)
            {
                case PacketType.PlayerMove:
                    OnPlayerMove(sender, (PlayerMove) packet);
                    break;
            }
        }

        private void OnPlayerMove(byte sender, PlayerMove packet)
        {
            if (!networkActors.ContainsKey(sender)) throw new Exception("no player object for " + sender);

            networkActors[sender].transform.position = packet.position;
            networkActors[sender].transform.rotation = packet.rotation;
        }

        public override void FixedUpdate()
        {
            if (connectedClients.Count == 0) return;

            worldState.Clear();

            foreach (var player in networkActors.Values)
                worldState.Add(new PlayerState
                {
                    playerId = player.PlayerId,
                    position = player.transform.position,
                    rotation = player.transform.rotation
                });

            SendToAll(WorldState.Write(server.CreateMessage(), Time.time, worldState), NetDeliveryMethod.UnreliableSequenced);
        }
    }
}
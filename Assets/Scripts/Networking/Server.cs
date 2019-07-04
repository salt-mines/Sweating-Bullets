using UnityEngine;
using Lidgren.Network;
using System.Collections.Generic;

namespace Networking
{
    class Server : Peer
    {
        private NetServer server;

        private byte nextHostId;
        private List<ClientInfo> connectedClients = new List<ClientInfo>();

        public Server()
        {
            peerConfig.Port = APP_PORT;
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.DebugMessage, true);
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.WarningMessage, true);
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ErrorMessage, true);
            peer = server = new NetServer(peerConfig);
        }

        public ClientInfo GetClientInfo(NetConnection connection)
        {
            if (connection.Tag == null)
            {
                connection.Tag = new ClientInfo(++nextHostId, connection);
            }

            return (ClientInfo)connection.Tag;
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
            foreach (var client in connectedClients)
            {
                SendToOne(msg, client.Connection, method);
            }
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
            var newStatus = (NetConnectionStatus)msg.ReadByte();
            var client = GetClientInfo(msg);
            switch(newStatus)
            {
                case NetConnectionStatus.Connected:
                    Debug.LogFormat("Conn [{0}]: {1}", this, client.HostId);
                    connectedClients.Add(client);
                    SendToOne(Packets.Connected.Write(server.CreateMessage(), client.HostId), client.Connection, NetDeliveryMethod.ReliableSequenced);
                    SendToAll(Packets.PlayerMove.Write(server.CreateMessage(), client.HostId, Vector3.zero, Quaternion.identity), NetDeliveryMethod.ReliableSequenced);
                    break;
                case NetConnectionStatus.Disconnecting:
                case NetConnectionStatus.Disconnected:
                    Debug.LogFormat("DC [{0}]: {1}", this, client.HostId);
                    connectedClients.Remove(client);
                    break;
            }
        }

        public void Update()
        {

        }
    }
}

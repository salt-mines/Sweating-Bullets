using UnityEngine;
using Lidgren.Network;

namespace Networking
{
    class Server : Peer
    {
        private NetServer server;

        private byte nextHostId;

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

        protected override void OnStatusMessage(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus)msg.ReadByte();
            var client = GetClientInfo(msg);
            switch(newStatus)
            {
                case NetConnectionStatus.Connected:
                    Debug.LogFormat("Conn [{0}]: {1}", this, client.HostId);
                    server.SendMessage(Packets.Connected.Write(server.CreateMessage(), client.HostId), client.Connection, NetDeliveryMethod.ReliableSequenced);
                    break;
                case NetConnectionStatus.Disconnected:
                    Debug.LogFormat("DC [{0}]: {1}", this, client.HostId);
                    break;
            }
        }
    }
}

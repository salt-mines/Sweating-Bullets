using System;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    internal sealed class MenuClient : Client
    {
        private readonly NetClient client;

        public event EventHandler<ServerInfo> ServerDiscovered;

        internal MenuClient()
        {
            client = new NetClient(new NetPeerConfiguration(Constants.AppName));
            client.Configuration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            client.Start();
        }

        public override void Shutdown()
        {
            client.Shutdown("Bye");
            base.Shutdown();
        }

        public void DiscoverLocalServers(int port = Constants.AppPort)
        {
            client.DiscoverLocalPeers(port);
        }

        private void Send<T>(T packet, NetDeliveryMethod method) where T : IPacket
        {
            client.SendMessage(Packet.Write(client, packet), method);
        }

        protected override void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        OnDiscovery(msg);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        NetworkLog.HandleMessage("Client", msg);
                        break;
                }

                client.Recycle(msg);
            }
        }

        protected override void SendState()
        {
        }

        public override void PlayerShoot(byte targetId)
        {
        }

        private void OnDiscovery(NetIncomingMessage msg)
        {
            var ip = msg.SenderEndPoint;
            var playerCount = msg.ReadByte();
            var maxPlayers = msg.ReadByte();

            ServerDiscovered?.Invoke(this, new ServerInfo
            {
                IP = ip,
                PlayerCount = playerCount,
                MaxPlayerCount = maxPlayers
            });
        }
    }
}
using Lidgren.Network;
using Networking.Packets;

namespace Networking
{
    internal class NetworkClient : Client
    {
        private NetClient client;

        internal NetworkClient()
        {
            client = new NetClient(new NetPeerConfiguration(Constants.AppName));
            client.Start();
        }

        protected override void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        OnDataMessage(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        //OnStatusMessage(msg);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //OnLibraryMessage(msg);
                        break;
                    default:
                        //OnUnhandledMessage(msg);
                        break;
                }

                client.Recycle(msg);
            }
        }

        protected override void SendState()
        {

        }

        protected void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType)msg.ReadByte();
            var packet = Packet.GetPacketFromType(type).Read(msg);

            switch (type)
            {
                case PacketType.Connected:
                    var conn = (Connected)packet;
                    SetInfo(conn.playerId, conn.maxPlayers);
                    break;
                case PacketType.PlayerDisconnected:
                    //OnPlayerDisconnected((PlayerDisconnected)packet);
                    break;
                case PacketType.WorldState:
                    var ws = (WorldState)packet;
                    AddWorldState(ws.worldState);
                    break;
                default:
                    //base.OnDataMessage(msg);
                    break;
            }
        }
    }
}

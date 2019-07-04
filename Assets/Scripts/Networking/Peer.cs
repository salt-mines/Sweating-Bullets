using UnityEngine;
using Lidgren.Network;

namespace Networking
{
    public abstract class Peer
    {
        public const int APP_PORT = 13456;
        public const string APP_NAME = "saltfps";

        protected NetPeerConfiguration peerConfig = new NetPeerConfiguration(APP_NAME);
        protected NetPeer peer;

        public Peer()
        {
        }

        public void Start()
        {
            peer.Start();
        }

        public void ReadMessages()
        {
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        OnDataMessage(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        OnStatusMessage(msg);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        OnLibraryMessage(msg);
                        break;
                    default:
                        OnUnhandledMessage(msg);
                        break;
                }

                peer.Recycle(msg);
            }
        }

        protected virtual void OnDataMessage(NetIncomingMessage msg)
        {
            Debug.LogFormat("Data [{0}]: {1} bytes", this, msg.LengthBytes);
        }

        protected virtual void OnStatusMessage(NetIncomingMessage msg)
        {
            Debug.LogFormat("Status [{0}]: {1}", this, (NetConnectionStatus)msg.ReadByte());
        }

        protected virtual void OnLibraryMessage(NetIncomingMessage msg)
        {
            Debug.LogFormat("Network Debug [{0}]: {1}", this, msg.ReadString());
        }

        protected virtual void OnUnhandledMessage(NetIncomingMessage msg)
        {
            Debug.LogErrorFormat("Unhandled network message [{0}]: {1}", this, msg.MessageType);
        }
    }
}

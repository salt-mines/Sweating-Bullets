using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    internal static class NetworkLog
    {
        public static void HandleMessage(string tag, NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                    Log(tag, msg);
                    break;
                case NetIncomingMessageType.WarningMessage:
                    Warning(tag, msg);
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    Error(tag, msg);
                    break;
            }
        }

        internal static void Log(string tag, NetIncomingMessage msg)
        {
            Debug.LogFormat("Network Debug [{0}]: {1}", tag, msg.ReadString());
        }

        internal static void Warning(string tag, NetIncomingMessage msg)
        {
            Debug.LogWarningFormat("Network Warning [{0}]: {1}", tag, msg.ReadString());
        }

        internal static void Error(string tag, NetIncomingMessage msg)
        {
            Debug.LogErrorFormat("Network Error [{0}]: {1}", tag, msg.ReadString());
        }
    }
}
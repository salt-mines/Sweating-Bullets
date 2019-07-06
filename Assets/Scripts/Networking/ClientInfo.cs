using Lidgren.Network;

namespace Networking
{
    public class ClientInfo
    {
        public byte PlayerId { get; private set; }
        public bool HasSpawned { get; set; }
        public NetConnection Connection { get; private set; }

        public ClientInfo(byte id, NetConnection connection)
        {
            PlayerId = id;
            Connection = connection;
            connection.Tag = this;
        }
    }
}

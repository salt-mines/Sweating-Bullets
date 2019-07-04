using Lidgren.Network;

namespace Networking
{
    public class ClientInfo
    {
        public byte HostId { get; private set; }
        public bool HasSpawned { get; set; }
        public NetConnection Connection { get; private set; }

        public ClientInfo(byte id, NetConnection connection)
        {
            HostId = id;
            Connection = connection;
            connection.Tag = this;
        }
    }
}

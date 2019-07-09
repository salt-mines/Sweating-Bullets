using Lidgren.Network;

namespace Networking
{
    public class ClientInfo
    {
        public int PlayerId { get; private set; }
        public bool HasSpawned { get; set; }
        public NetConnection Connection { get; private set; }

        public ClientInfo(int id, NetConnection connection)
        {
            PlayerId = id;
            Connection = connection;
            connection.Tag = this;
        }
    }
}

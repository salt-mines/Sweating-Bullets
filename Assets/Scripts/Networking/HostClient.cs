using Networking.Packets;

namespace Networking
{
    internal class HostClient : Client
    {
        internal HostClient(Server server)
        {
            Server = server;

            SetInfo(Server.CreatePlayer().Id, Server.MaxPlayerCount);
        }

        private Server Server { get; }

        protected override void ProcessMessages()
        {
            AddWorldState(Server.WorldState);
        }

        protected override void SendState()
        {
            if (!PlayerId.HasValue) return;
            if (!LocalActor) return;

            Server.OnPlayerMove(PlayerId.Value, new PlayerMove
            {
                playerId = PlayerId.Value,
                position = LocalActor.transform.position,
                rotation = LocalActor.transform.rotation
            });
        }
    }
}
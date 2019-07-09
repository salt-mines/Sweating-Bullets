namespace Networking
{
    internal class HostClient : Client
    {
        private Server Server { get; }

        internal HostClient(Server server)
        {
            Server = server;

            SetInfo(Server.CreatePlayer().Id, Server.MaxPlayerCount);
        }

        protected override void ProcessMessages()
        {
            AddWorldState(Server.WorldState);
        }

        protected override void SendState()
        {
            if (!PlayerId.HasValue) return;
            if (!LocalActor) return;

            Server.OnPlayerMove(PlayerId.Value, new Packets.PlayerMove
            {
                playerId = PlayerId.Value,
                position = LocalActor.transform.position,
                rotation = LocalActor.transform.rotation,
            });
        }
    }
}

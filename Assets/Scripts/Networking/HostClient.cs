using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    internal sealed class HostClient : Client
    {
        internal HostClient(Server server)
        {
            Server = server;

            var ply = Server.CreatePlayer(true);
            InitializeFromServer(ply.Id, Server.MaxPlayerCount);
            ply.PlayerObject.PlayerInfo = CreatePlayer(ply.Id, true);
        }

        private Server Server { get; }

        protected override void ProcessMessages()
        {
            AddWorldState(Server.WorldState);
        }

        protected override void SendState()
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            var ply = Players[PlayerId.Value];

            if (ply == null) return;

            Server.OnPlayerMove(ply.Id, new PlayerMove
            {
                playerId = ply.Id,
                position = ply.Position,
                rotation = ply.Rotation
            });
        }

        internal override void OnGUI(float x, float y)
        {
            var origY = y;
            GUI.Box(new Rect(x, y += 20, 140, 90), "Host");
            if (PlayerId.HasValue && Players[PlayerId.Value] != null)
            {
                var ply = Players[PlayerId.Value];
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Pos: {ply.Position}");
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Rot: {ply.Rotation.eulerAngles}");
            }

            GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Interp: {Interpolation * 1000} ms");

            x += 140;
            y = origY;
            GUI.Box(new Rect(x + 5, y += 20, 100, 50), "Server");
            GUI.Label(new Rect(x + 10, y += 20, 100, 20), "Players: " + Server.PlayerCount);
        }
    }
}
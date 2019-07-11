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

        protected override PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            var ply = base.CreatePlayer(id, local);
            ply.PlayerObject = Server.Players[id].PlayerObject;
            ply.PlayerObject.PlayerInfo = ply;
            ply.PlayerObject.NetworkClient = this;
            return ply;
        }

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

        public override void PlayerShoot(byte targetId)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Server.OnPlayerShoot(PlayerId.Value, new PlayerShoot
            {
                shooterId = PlayerId.Value,
                targetId = targetId
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

        internal override void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (var ply in Server.Players)
            {
                if (ply == null) continue;

                Gizmos.DrawSphere(ply.Position, 0.1f);
            }
        }
    }
}
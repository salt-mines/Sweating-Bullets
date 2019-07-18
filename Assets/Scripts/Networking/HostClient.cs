using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    internal sealed class HostClient : Client
    {
        private bool wasAlive = true;

        internal HostClient(Server server)
        {
            Server = server;
            Server.Loader.LevelLoaded += (o, s) => LevelLoaded(s);
        }

        private Server Server { get; }

        private void LevelLoaded(string level)
        {
            var ply = Server.CreatePlayer(true);
            InitializeFromServer(ply.Id, Server.MaxPlayerCount, Server.Level, Server.BuildPlayerList(ply.Id));
            ply.PlayerObject.PlayerInfo = CreatePlayer(ply.Id, true);
            Server.ReceivePlayerInfo(ply.Id, Preferences.Name);
            Loaded = true;
        }

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

            if (!PlayerId.HasValue) return;

            if (!Server.Players[PlayerId.Value].Alive && wasAlive)
            {
                wasAlive = false;
                Players[PlayerId.Value].PlayerObject.Kill();
                UnityEngine.Debug.Log("Death!");
            }
            else if (Server.Players[PlayerId.Value].Alive)
            {
                wasAlive = true;
            }
        }

        protected override void SendState()
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            var ply = Players[PlayerId.Value];

            if (ply == null) return;

            Server.OnPlayerMove(ply.Id, ply.GetState());
        }

        public override void PlayerShoot(byte targetId)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            UnityEngine.Debug.LogFormat("Shooting Player {0}", targetId);
            
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
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Rot: {ply.ViewAngles}");
            }

            GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Interp: {Interpolation * 1000} ms");

            x += 140;
            y = origY;
            GUI.Box(new Rect(x + 5, y += 20, 100, 50), "Server");
            GUI.Label(new Rect(x + 10, y += 20, 100, 20), "Players: " + Server.PlayerCount);
        }

        internal override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Gizmos.color = Color.yellow;
            foreach (var ply in Server.Players)
            {
                if (ply == null) continue;

                Gizmos.DrawSphere(ply.Position, 0.1f);
            }
        }
    }
}
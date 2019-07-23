using Game;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    internal sealed class HostClient : Client
    {
        internal HostClient(Server server)
        {
            Server = server;
            Server.Loader.LevelLoaded += (o, s) => LevelLoaded(s);
            Server.PlayerJoined += OnPlayerJoined;
            Server.PlayerLeft += OnPlayerLeft;
            Server.PlayerSentInfo += OnPlayerSentInfo;
            Server.PlayerDied += OnPlayerDeath;
            Server.PlayerShot += OnPlayerShoot;
        }

        private Server Server { get; }

        private void OnPlayerJoined(object sender, PlayerInfo player)
        {
            var p = CreatePlayer(player.Id);
            Debug.Log($"Id: {p.Id} joined");
        }

        private void OnPlayerLeft(object sender, PlayerInfo player)
        {
            RemovePlayer(player.Id);
        }

        private void OnPlayerSentInfo(object sender, PlayerExtraInfo info)
        {
            OnPlayerSentInfo(info);
        }

        private void OnPlayerDeath(object sender, PlayerDeath death)
        {
            OnPlayerDeath(death);
        }

        private void OnPlayerShoot(object sender, PlayerShoot shot)
        {
            if (Players == null || LocalPlayer == null || shot.playerId == PlayerId) return;

            LocalPlayer.PlayerObject.GetComponent<PlayerShooting>().SpawnLine(shot.from, shot.to);
        }

        private void LevelLoaded(string level)
        {
            var ply = Server.CreatePlayer(true);

            InitializeFromServer(ply.Id, Server.MaxPlayerCount, Server.Level, Server.BuildPlayerList(ply.Id));
            ply.PlayerObject.PlayerInfo = CreatePlayer(ply.Id, true);
            var info = new PlayerExtraInfo {name = Preferences.Name};
            Server.ReceivePlayerInfo(ply.Id, info);
            OnPlayerSentInfo(info);
            Loaded = true;
        }

        protected override PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            var ply = base.CreatePlayer(id, local);
            ply.PlayerObject = Server.Players[id].PlayerObject;
            ply.PlayerObject.PlayerInfo = ply;
            ply.PlayerObject.Client = this;
            return ply;
        }

        protected override void ProcessMessages()
        {
            AddWorldState(Server.WorldState);
        }

        protected override void SendState()
        {
            System.Diagnostics.Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            var ply = Players[PlayerId.Value];

            if (ply == null) return;

            Server.OnPlayerMove(ply.Id, ply.GetState());

            if (ply.Teleported)
                ply.Teleported = false;
        }

        public override void PlayerShoot(Vector3 from, Vector3 to)
        {
            System.Diagnostics.Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Server.OnPlayerShoot(PlayerId.Value, new PlayerShoot
            {
                playerId = PlayerId.Value,
                from = from,
                to = to
            });
        }

        public override void PlayerKill(byte targetId)
        {
            base.PlayerKill(targetId);

            System.Diagnostics.Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Debug.LogFormat("Shooting Player {0}", targetId);

            Server.OnPlayerKill(PlayerId.Value, new PlayerKill
            {
                killerId = PlayerId.Value,
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
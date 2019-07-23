using System;
using System.Collections.Generic;
using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    public abstract class Client
    {
        private float nextSend;
        private float nextTick;

        public NetworkManager NetworkManager { get; internal set; }

        public PlayerInfo[] Players { get; private set; }
        public byte MaxPlayers { get; protected set; }

        public int TickRate { get; set; } = Constants.TickRate;
        public int SendRate { get; set; } = Constants.SendRate;

        public bool InterpolationEnabled { get; set; } = true;
        public float Interpolation { get; set; } = Constants.Interpolation;

        public byte? PlayerId { get; protected set; }
        public PlayerInfo LocalPlayer => PlayerId.HasValue ? Players[PlayerId.Value] : null;

        public bool Connected => PlayerId.HasValue;
        public bool Loaded { get; protected set; }

        public event EventHandler<PlayerInfo> PlayerJoined;
        public event EventHandler<PlayerInfo> PlayerLeft;
        public event EventHandler<PlayerPreferences> PlayerSentPreferences;
        public event EventHandler<PlayerExtraInfo> PlayerSentInfo;

        public event EventHandler<PlayerInfo> OwnKill;
        public event EventHandler<PlayerDeath> PlayerDeath;

        protected Preferences Preferences => NetworkManager.Loader.Preferences;

        public void Update()
        {
            var time = Time.time;
            if (time >= nextTick)
            {
                ProcessMessages();
                nextTick = time + 1f / TickRate;
            }

            if (!Connected || !Loaded) return;

            InterpolatePlayers();
        }

        public void LateUpdate()
        {
            if (!Connected || !Loaded) return;

            var time = Time.time;
            if (time < nextSend) return;

            SendState();
            nextSend = time + 1f / SendRate;
        }

        public virtual void Connect(string host, int port = Constants.AppPort)
        {
        }

        public virtual void Shutdown()
        {
            UnityEngine.Debug.Log($"{this} shutting down.");
            PlayerId = null;
            if (Players != null)
                Array.Clear(Players, 0, Players.Length);
        }

        protected abstract void ProcessMessages();

        protected abstract void SendState();

        protected void InitializeFromServer(Connected packet)
        {
            InitializeFromServer(packet.playerId, packet.maxPlayers, packet.levelName, packet.currentPlayers, packet.currentPlayersInfo);
        }

        protected virtual void InitializeFromServer(byte playerId, byte maxPlayers, string level,
            List<PlayerPreferences> currentPlayers, List<PlayerExtraInfo> currentPlayersInfo)
        {
            UnityEngine.Debug.Log($"InitializeFromServer: P#{playerId}; max {maxPlayers}; level {level}");

            MaxPlayers = maxPlayers;
            Players = new PlayerInfo[maxPlayers];
            PlayerId = playerId;

            // TODO: Do this when level is active scene so player objects go there
            UnityEngine.Debug.Log("Received player list");
            foreach (var pl in currentPlayers)
            {
                var p = CreatePlayer(pl.playerId);
                OnPlayerSentPreferences(pl);
                UnityEngine.Debug.Log($"Player on server: {p}");
            }

            foreach (var pl in currentPlayersInfo)
            {
                OnPlayerSentInfo(pl);
            }
        }

        protected virtual void OnPlayerJoined(PlayerInfo player)
        {
            PlayerJoined?.Invoke(this, player);
        }

        protected virtual void OnPlayerLeft(PlayerInfo player)
        {
            PlayerLeft?.Invoke(this, player);
        }

        protected virtual void OnPlayerSentPreferences(PlayerPreferences packet)
        {
            var pl = Players[packet.playerId];
            if (pl == null) return;
            pl.Name = packet.name;

            PlayerSentPreferences?.Invoke(this, packet);
        }

        protected virtual void OnPlayerSentInfo(PlayerExtraInfo packet)
        {
            var pl = Players[packet.playerId];
            if (pl == null) return;

            pl.Kills = packet.kills;
            pl.Deaths = packet.deaths;

            PlayerSentInfo?.Invoke(this, packet);
        }

        protected virtual PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            Players[id] = new PlayerInfo(id);
            OnPlayerJoined(Players[id]);
            return Players[id];
        }

        protected void RemovePlayer(PlayerInfo ply)
        {
            RemovePlayer(ply.Id);
        }

        protected virtual void RemovePlayer(byte id)
        {
            OnPlayerLeft(Players[id]);
            Players[id] = null;
        }

        public virtual void OnPlayerDeath(PlayerDeath packet)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            if (packet.playerId == PlayerId.Value)
            {
                Players[PlayerId.Value].PlayerObject.Kill();
                UnityEngine.Debug.Log("Death!");
            }

            Players[packet.playerId].Deaths = packet.playerDeaths;
            Players[packet.killerId].Kills = packet.killerKills;

            UnityEngine.Debug.LogFormat("Player {0} killed Player {1}", packet.killerId, packet.playerId);

            PlayerDeath?.Invoke(this, packet);
        }

        public abstract void PlayerShoot(Vector3 from, Vector3 to);

        public virtual void KillPlayer(byte targetId)
        {
            OwnKill?.Invoke(this, Players[targetId]);
        }

        internal virtual void OnGUI(float x, float y)
        {
        }

        internal virtual void OnDrawGizmos()
        {
            if (!Loaded) return;

            Gizmos.color = Color.green;
            foreach (var ply in Players)
            {
                if (ply == null) continue;
                var buf = ply.StateBuffer;

                foreach (var state in buf) Gizmos.DrawSphere(state.state.position, 0.1f);
            }
        }

        #region State updates and interpolation

        protected void AddWorldState(PlayerState?[] worldState)
        {
            if (Players == null || !Loaded) return;

            for (byte i = 0; i < worldState.Length; i++)
            {
                var ps = worldState[i];
                if (!ps.HasValue) continue;

                if (Players[i] == null) continue;

                // Don't update our own state
                if (PlayerId.HasValue && i == PlayerId.Value) continue;

                if (!InterpolationEnabled)
                {
                    // Without interpolation, just update pos & rot directly
                    Players[i].SetFromState(ps.Value);
                    continue;
                }

                Players[i].StateBuffer.AddLast(new TimedPlayerState
                {
                    time = Time.time,
                    state = ps.Value
                });
            }
        }

        private void InterpolatePlayers()
        {
            if (Players == null || !Loaded) return;

            var pastTime = Time.time - Interpolation;

            foreach (var ply in Players)
            {
                if (ply == null) continue;

                if (PlayerId.HasValue && ply.Id == PlayerId.Value) continue;

                var buf = ply.StateBuffer;

                while (buf.Count >= 2 && buf.First.Next.Value.time <= pastTime) buf.RemoveFirst();

                if (buf.Count < 2) continue;

                var from = buf.First.Value;
                var to = buf.First.Next.Value;

                if (!(from.time <= pastTime) || !(pastTime <= to.time)) continue;

                var ratio = (pastTime - from.time) / (to.time - from.time);

                ply.SetFromState(PlayerState.Lerp(from.state, to.state, ratio));
            }
        }

        #endregion
    }
}
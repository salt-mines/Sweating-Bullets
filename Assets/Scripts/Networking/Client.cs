using System;
using System.Collections.Generic;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    public abstract class Client
    {
        private float nextSendTime;
        private float nextTickTime;

        // List of players and their infos on the joined server. Held here to be
        // actually created client-side once we have loaded the level.
        private List<PlayerPreferences> serverPlayers;
        private List<PlayerExtraInfo> serverPlayerInfos;

        /// <summary>
        ///     Network manager object for client.
        /// </summary>
        public NetworkManager NetworkManager { get; internal set; }

        public byte GameModeId { get; internal set; }

        /// <summary>
        ///     List of players on current server.
        /// </summary>
        public PlayerInfo[] Players { get; private set; }

        /// <summary>
        ///     How many times per second client should check for new packets.
        /// </summary>
        public int TickRate { get; set; } = Constants.TickRate;

        /// <summary>
        ///     How many times per second client should send packets to server.
        /// </summary>
        public int SendRate { get; set; } = Constants.SendRate;

        /// <summary>
        ///     Whether client-side interpolation of other players should be enabled.
        /// </summary>
        public bool InterpolationEnabled { get; set; } = true;

        /// <summary>
        ///     How long should interpolation period be.
        /// </summary>
        public float Interpolation { get; set; } = Constants.Interpolation;

        /// <summary>
        ///     Player ID of local player, or null if client isn't connected yet.
        /// </summary>
        public byte? PlayerId { get; protected set; }

        /// <summary>
        ///     Local player object if one exists, or null.
        /// </summary>
        public PlayerInfo LocalPlayer => PlayerId.HasValue ? Players[PlayerId.Value] : null;

        /// <summary>
        ///     True if client is connected to a server and has its player id.
        /// </summary>
        public bool Connected => PlayerId.HasValue;

        /// <summary>
        ///     True if level is loaded.
        /// </summary>
        public bool Loaded { get; protected set; }

        protected Preferences Preferences => NetworkManager.Loader.Preferences;

        public event EventHandler<Connected> ServerInfoReceived;

        public event EventHandler<PlayerInfo> PlayerJoined;
        public event EventHandler<PlayerInfo> PlayerLeft;
        public event EventHandler<PlayerPreferences> PlayerSentPreferences;
        public event EventHandler<PlayerExtraInfo> PlayerSentInfo;

        public event EventHandler<PlayerInfo> OwnKill;
        public event EventHandler<byte> SelfHurt;
        public event EventHandler<PlayerDeath> PlayerDeath;
        public event EventHandler<PlayerInfo> PlayerRespawn;

        public void Update()
        {
            var time = Time.time;
            if (time >= nextTickTime)
            {
                ProcessMessages();
                nextTickTime = time + 1f / TickRate;
            }

            if (!Connected || !Loaded) return;

            InterpolatePlayers();
        }

        public void LateUpdate()
        {
            if (!Connected || !Loaded) return;

            var time = Time.time;
            if (time < nextSendTime) return;

            SendState();
            nextSendTime = time + 1f / SendRate;
        }

        public virtual void Connect(string host, int port = Constants.AppPort)
        {
        }

        public virtual void Shutdown()
        {
            Debug.Log($"{this} shutting down.");
            PlayerId = null;
            if (Players != null)
                Array.Clear(Players, 0, Players.Length);
        }

        protected abstract void ProcessMessages();

        protected abstract void SendState();

        protected virtual void InitializeFromServer(Connected packet)
        {
            Players = new PlayerInfo[packet.maxPlayers];
            PlayerId = packet.playerId;

            GameModeId = packet.modeId;

            // Store player info to be used later.
            serverPlayers = packet.currentPlayers;
            serverPlayerInfos = packet.currentPlayersInfo;

            ServerInfoReceived?.Invoke(this, packet);
        }

        protected void CreateServerPlayers()
        {
            Debug.Log("Received player list");
            foreach (var pl in serverPlayers)
            {
                var p = CreatePlayer(pl.playerId);
                OnPlayerSentPreferences(pl);
                Debug.Log($"Player on server: {p}");
            }

            foreach (var pl in serverPlayerInfos) OnPlayerSentInfo(pl);
        }

        protected virtual PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            Players[id] = new PlayerInfo(id);
            OnPlayerJoined(Players[id]);
            return Players[id];
        }

        protected virtual void RemovePlayer(byte id)
        {
            OnPlayerLeft(Players[id]);
            Players[id] = null;
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
            pl.Color = packet.color;

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

        protected void OnSelfHurt(byte damage)
        {
            SelfHurt?.Invoke(this, damage);
        }

        protected virtual void OnPlayerDeath(PlayerDeath packet)
        {
            System.Diagnostics.Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            if (packet.playerId == PlayerId.Value)
            {
                Players[PlayerId.Value].PlayerObject.Kill();
                Debug.Log("Death!");
            }

            Players[packet.playerId].Deaths = packet.playerDeaths;
            Players[packet.killerId].Kills = packet.killerKills;

            Debug.LogFormat("Player {0} killed Player {1}", packet.killerId, packet.playerId);

            PlayerDeath?.Invoke(this, packet);
        }

        internal void OnPlayerRespawn(PlayerInfo player)
        {
            PlayerRespawn?.Invoke(this, player);
        }

        /// <summary>
        ///     Called when local player has shot.
        /// </summary>
        /// <param name="from">Where the shot was shot from</param>
        /// <param name="to">Where the shot ended</param>
        /// <param name="hit">RaycastHit if shot hit something, null otherwise</param>
        public virtual void PlayerShootOne(Vector3 from, Vector3 to, byte damage, RaycastHit hit)
        {
        }

        public virtual void PlayerShootMultiple(Vector3 from, Vector3 to, byte damage, RaycastHit[] hits)
        {
        }

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

        /// <summary>
        ///     Add a world state packet into interpolation queue (or apply immediately if interpolation is disabled).
        /// </summary>
        /// <param name="worldState"></param>
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

        /// <summary>
        ///     Interpolate players' locations in between received packets.
        /// </summary>
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
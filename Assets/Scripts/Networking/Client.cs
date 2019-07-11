﻿using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    public abstract class Client
    {
        public NetworkManager NetworkManager { get; internal set; }

        public PlayerInfo[] Players { get; private set; }
        public byte MaxPlayers { get; protected set; }

        public bool InterpolationEnabled { get; set; } = true;
        public float Interpolation { get; set; } = 0.1f;

        public byte? PlayerId { get; protected set; }

        public bool Connected => PlayerId.HasValue;

        public void Update()
        {
            ProcessMessages();

            if (!Connected) return;

            InterpolatePlayers();

            SendState();
        }

        public virtual void Connect(string host, int port = Constants.AppPort)
        {
        }

        public virtual void Shutdown()
        {
        }

        protected abstract void ProcessMessages();

        protected abstract void SendState();

        protected void InitializeFromServer(byte playerId, byte maxPlayers)
        {
            MaxPlayers = maxPlayers;
            Players = new PlayerInfo[maxPlayers];

            PlayerId = playerId;
            CreatePlayer(playerId, true);
        }

        protected virtual PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            Players[id] = new PlayerInfo(id);
            return Players[id];
        }

        protected void RemovePlayer(PlayerInfo ply)
        {
            RemovePlayer(ply.Id);
        }

        protected virtual void RemovePlayer(byte id)
        {
            Players[id] = null;
        }

        public abstract void PlayerShoot(byte targetId);
        
        #region State updates and interpolation

        protected void AddWorldState(PlayerState?[] worldState)
        {
            if (Players == null) return;

            for (byte i = 0; i < worldState.Length; i++)
            {
                var ps = worldState[i];
                if (!ps.HasValue) continue;

                if (Players[i] == null) CreatePlayer(i);

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

        internal abstract void OnGUI(float x, float y);

        internal abstract void OnDrawGizmos();
    }
}
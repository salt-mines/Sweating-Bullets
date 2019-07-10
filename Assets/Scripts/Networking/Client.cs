using System.Collections.Generic;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    internal abstract class Client
    {
        public List<PlayerInfo> Players { get; } = new List<PlayerInfo>();
        public byte MaxPlayers { get; protected set; }

        public bool InterpolationEnabled { get; set; } = true;

        public float Interpolation { get; set; } = 0.1f;

        public byte? PlayerId { get; protected set; }

        protected GameObject LocalActor { get; set; }
        public GameObject LocalPlayerPrefab { get; set; }

        public bool Connected => PlayerId != null;

        public void Update()
        {
            ProcessMessages();

            if (!Connected) return;

            UpdateWorldState();

            SendState();
        }

        protected abstract void ProcessMessages();

        protected abstract void SendState();

        protected void SetInfo(byte playerId, byte maxPlayers)
        {
            PlayerId = playerId;
            MaxPlayers = maxPlayers;
            Players.Capacity = maxPlayers;
        }

        private void OnPlayerDisconnected(PlayerDisconnected packet)
        {
        }

        protected void AddWorldState(PlayerState?[] worldState)
        {
            for (byte i = 0; i < worldState.Length; i++)
            {
                var ps = worldState[i];
                if (ps.HasValue)
                {
                    if (Players.Count <= i || Players[i] == null)
                    {
                        var ply = new PlayerInfo(i);

                        if (Players.Count <= i)
                            Players.Add(ply);
                        else
                            Players[i] = ply;
                    }

                    Players[i].StateBuffer.AddLast(new TimedPlayerState
                    {
                        time = Time.time,
                        state = ps.Value
                    });
                }
            }
        }

        private void UpdatePlayerState(byte playerId, PlayerState state)
        {
            // Don't update ourselves
            if (playerId == PlayerId) return;

            Debug.LogFormat("P#{0}: pos {1}; rot {2}", playerId, state.position, state.rotation);

            //if (!networkActors.TryGetValue(playerId, out var actor))
            {
                //Debug.LogFormat("Player {0} at {1}, rotated {2}", playerId, state.position, state.rotation);
                //actor = Object.Instantiate(NetworkPlayerPrefab);
                //networkActors.Add(playerId, actor);
            }

            //actor.transform.SetPositionAndRotation(state.position, state.rotation);
        }

        private void UpdateWorldState()
        {
            foreach (var ply in Players)
            {
                foreach (var state in ply.StateBuffer) UpdatePlayerState(ply.Id, state.state);
                ply.StateBuffer.Clear();
            }

            //if (stateBuffer.Count == 0) return;

            //var interpTime = Time.time - Interpolation;

            //var from = stateBuffer.First;
            //var to = from.Next;

            //while (to != null && to.Value.time <= interpTime)
            //{
            //    from = to;
            //    to = from.Next;
            //    stateBuffer.RemoveFirst();
            //}

            //if (to != null)
            //{
            //    var ratio = (interpTime - from.Value.time) / (to.Value.time - from.Value.time);
            //    var idSet = new HashSet<byte>(from.Value.worldState.Keys);
            //    idSet.UnionWith(to.Value.worldState.Keys);

            //    foreach (var id in idSet)
            //    {
            //        var fromExists = from.Value.worldState.TryGetValue(id, out var playerFrom);
            //        var toExists = to.Value.worldState.TryGetValue(id, out var playerTo);

            //        if (fromExists && toExists)
            //        {
            //            UpdatePlayerState(id, PlayerState.Lerp(playerFrom, playerTo, ratio));
            //        }
            //        else
            //        {
            //            UpdatePlayerState(id, toExists ? playerTo : playerFrom);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var kv in from.Value.worldState)
            //    {
            //        UpdatePlayerState(kv.Key, kv.Value);
            //    }
            //}
        }
    }
}
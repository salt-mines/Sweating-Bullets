using UnityEngine;

namespace Networking
{
    internal abstract class Client
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

            UpdateWorldState();

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

        protected void AddWorldState(PlayerState?[] worldState)
        {
            for (byte i = 0; i < worldState.Length; i++)
            {
                var ps = worldState[i];
                if (!ps.HasValue) continue;

                if (Players[i] == null) CreatePlayer(i);

                Players[i].StateBuffer.AddLast(new TimedPlayerState
                {
                    time = Time.time,
                    state = ps.Value
                });
            }
        }

        private void UpdateWorldState()
        {
            foreach (var ply in Players)
            {
                if (ply == null) continue;
                
                // Update all players' positions except our own
                foreach (var state in ply.StateBuffer)
                    if (ply.Id != PlayerId) 
                        ply.SetFromState(state.state);
                
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

        internal abstract void OnGUI(float x, float y);
    }
}
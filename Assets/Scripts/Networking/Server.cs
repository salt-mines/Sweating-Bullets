using System;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    public sealed class Server
    {
        private readonly NetServer server;

        private float nextSend;
        private float nextTick;

        public Server(byte maxPlayers, Loader loader)
        {
            MaxPlayerCount = maxPlayers;
            Players = new PlayerInfo[maxPlayers];
            WorldState = new PlayerState?[maxPlayers];

            Loader = loader;
            LevelManager = loader.LevelManager;

            server = new NetServer(new NetPeerConfiguration(Constants.AppName)
            {
                Port = Constants.AppPort,
                MaximumConnections = maxPlayers,
#if UNITY_EDITOR
                ConnectionTimeout = 600
#endif
            });

            server.Configuration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            server.Start();

            LevelManager.ChangeToStartingLevel();
        }

        public Loader Loader { get; }
        public LevelManager LevelManager { get; }
        public NetworkManager NetworkManager { get; internal set; }

        public float SimulatedLag
        {
#if UNITY_EDITOR
            get => server.Configuration.SimulatedMinimumLatency;
            set => server.Configuration.SimulatedMinimumLatency = value;
#else
            get { return 0; }
            set {}
#endif
        }

        public int TickRate { get; set; } = 64;
        public int SendRate { get; set; } = 32;

        public byte MaxPlayerCount { get; }
        public byte PlayerCount { get; private set; }

        public PlayerInfo[] Players { get; }
        public PlayerState?[] WorldState { get; }

        public string Level => LevelManager.CurrentLevel;
        
        public void Update()
        {
            var time = Time.time;
            if (time >= nextTick)
            {
                ProcessMessages();
                nextTick = time + 1f / TickRate;
            }
        }

        public void LateUpdate()
        {
            var time = Time.time;
            if (time >= nextSend)
            {
                SendState();
                nextSend = time + 1f / SendRate;
            }
        }

        public void SendState()
        {
            Array.Clear(WorldState, 0, WorldState.Length);

            foreach (var player in Players)
            {
                if (player == null) continue;

                WorldState[player.Id] = player.GetState();
            }

            SendToAll(new WorldState {worldState = WorldState}, NetDeliveryMethod.UnreliableSequenced);
        }

        public void Shutdown()
        {
            server.Shutdown("Bye");
        }

        #region Gameplay methods

        #endregion

        #region Player methods

        public byte GetFreePlayerId()
        {
            for (byte i = 0; i < Players.Length; i++)
                if (Players[i] == null)
                    return i;

            throw new IndexOutOfRangeException("Too many players, no IDs left");
        }

        public PlayerInfo CreatePlayer(bool local = false, byte? id = null, NetConnection connection = null)
        {
            if (!id.HasValue) id = GetFreePlayerId();

            var ply = new PlayerInfo(id.Value, connection);
            if (connection != null) connection.Tag = ply;

            Players[ply.Id] = ply;
            PlayerCount++;

            if (NetworkManager)
                ply.PlayerObject = NetworkManager.CreatePlayer(ply, local);

            return ply;
        }

        public void RemovePlayer(PlayerInfo ply)
        {
            RemovePlayer(ply.Id);
        }

        public void RemovePlayer(byte id)
        {
            if (Players[id] == null) return;

            if (NetworkManager)
                NetworkManager.RemovePlayer(Players[id].PlayerObject);

            Players[id] = null;
            PlayerCount--;
        }

        private PlayerInfo GetPlayerInfo(NetConnection connection)
        {
            return connection.Tag as PlayerInfo;
        }

        private PlayerInfo GetPlayerInfo(NetIncomingMessage msg)
        {
            return GetPlayerInfo(msg.SenderConnection);
        }

        #endregion

        #region Packet sending

        private void SendToOne<T>(T packet, NetConnection connection, NetDeliveryMethod method)
            where T : IPacket
        {
            server.SendMessage(Packet.Write(server, packet), connection, method);
        }

        private void SendToAll<T>(T packet, NetDeliveryMethod method)
            where T : IPacket
        {
            if (server.ConnectionsCount == 0) return;

            server.SendMessage(Packet.Write(server, packet), server.Connections, method, 0);
        }

        #endregion

        #region Packet handling

        private void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        OnDataMessage(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        OnStatusMessage(msg);
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        OnDiscoveryRequest(msg);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        NetworkLog.HandleMessage("Server", msg);
                        break;
                }

                server.Recycle(msg);
            }
        }

        private void OnDiscoveryRequest(NetIncomingMessage msg)
        {
            var response = server.CreateMessage();
            response.Write(PlayerCount);
            response.Write(MaxPlayerCount);

            // Send the response to the sender of the request
            server.SendDiscoveryResponse(response, msg.SenderEndPoint);
        }

        private void OnStatusMessage(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus) msg.ReadByte();
            var client = GetPlayerInfo(msg);
            switch (newStatus)
            {
                case NetConnectionStatus.Connected:
                    OnPlayerConnected(msg.SenderConnection);
                    break;
                case NetConnectionStatus.Disconnecting:
                case NetConnectionStatus.Disconnected:
                    OnPlayerDisconnected(client);
                    break;
            }
        }

        private void OnPlayerConnected(NetConnection connection)
        {
            var player = CreatePlayer(false, null, connection);

            Debug.LogFormat("Conn [{0}]: {1}", "Server", player.Id);

            SendToOne(new Connected
            {
                playerId = player.Id,
                maxPlayers = MaxPlayerCount,
                levelName = Level
            }, player.Connection, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnPlayerDisconnected(PlayerInfo player)
        {
            Debug.LogFormat("DC [{0}]: {1}", "Server", player.Id);

            SendToAll(new PlayerDisconnected {playerId = player.Id}, NetDeliveryMethod.ReliableUnordered);

            RemovePlayer(player);
        }

        private void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();
            var sender = GetPlayerInfo(msg).Id;

            switch (type)
            {
                case PacketType.PlayerMove:
                    OnPlayerMove(sender, PlayerMove.Read(msg));
                    break;
                case PacketType.PlayerShoot:
                    OnPlayerShoot(sender, PlayerShoot.Read(msg));
                    break;
            }
        }

        internal void OnPlayerMove(byte sender, PlayerMove packet)
        {
            Players[sender]?.SetFromPacket(packet);
        }

        public void OnPlayerShoot(byte sender, PlayerShoot packet)
        {
            if (Players[sender] == null || Players[packet.targetId] == null)
                return;

            Players[packet.targetId].Alive = false;

            SendToAll(new PlayerDeath
            {
                playerId = packet.targetId,
                killerId = sender
            }, NetDeliveryMethod.ReliableUnordered);
        }

        #endregion
    }
}
using System;
using JetBrains.Annotations;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    internal sealed class Server
    {
        private readonly NetServer server;

        public Server(byte maxPlayers)
        {
            MaxPlayerCount = maxPlayers;
            Players = new PlayerInfo[maxPlayers];
            WorldState = new PlayerState?[maxPlayers];

            server = new NetServer(new NetPeerConfiguration(Constants.AppName)
            {
                Port = Constants.AppPort,
                MaximumConnections = maxPlayers
            });

            server.Start();
        }

        public NetworkManager NetworkManager { get; internal set; }

        public byte MaxPlayerCount { get; }
        public byte PlayerCount { get; private set; }

        public PlayerInfo[] Players { get; }
        public PlayerState?[] WorldState { get; }

        public void Update()
        {
            ProcessMessages();
        }

        public void LateUpdate()
        {
            Array.Clear(WorldState, 0, WorldState.Length);

            foreach (var player in Players)
            {
                if (player == null) continue;

                WorldState[player.Id] = new PlayerState
                {
                    playerId = player.Id,
                    position = player.Position,
                    rotation = player.Rotation
                };
            }

            SendToAll(Packet.Write(server, new WorldState {worldState = WorldState}),
                NetDeliveryMethod.UnreliableSequenced);
        }

        public void Shutdown()
        {
            server.Shutdown("Bye");
        }

        #region Player methods

        public byte GetFreePlayerId()
        {
            for (byte i = 0; i < Players.Length; i++)
                if (Players[i] == null)
                    return i;

            throw new IndexOutOfRangeException("Too many players, no IDs left");
        }

        public PlayerInfo CreatePlayer(bool local = false, byte? id = null, [CanBeNull] NetConnection connection = null)
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

        private void SendToOne(NetOutgoingMessage msg, NetConnection connection, NetDeliveryMethod method)
        {
            server.SendMessage(msg, connection, method);
        }

        private void SendToAll(NetOutgoingMessage msg, NetDeliveryMethod method)
        {
            if (server.ConnectionsCount == 0) return;

            server.SendMessage(msg, server.Connections, method, 0);
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

            SendToOne(Packet.Write(server, new Connected
            {
                playerId = player.Id,
                maxPlayers = MaxPlayerCount
            }), player.Connection, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnPlayerDisconnected(PlayerInfo player)
        {
            Debug.LogFormat("DC [{0}]: {1}", "Server", player.Id);

            SendToAll(Packet.Write(server, new PlayerDisconnected {playerId = player.Id}),
                NetDeliveryMethod.ReliableUnordered);

            RemovePlayer(player);
        }

        private void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();
            var packet = Packet.GetPacketFromType(type).Read(msg);
            var sender = GetPlayerInfo(msg).Id;

            switch (type)
            {
                case PacketType.PlayerMove:
                    OnPlayerMove(sender, (PlayerMove) packet);
                    break;
            }
        }

        internal void OnPlayerMove(byte sender, PlayerMove packet)
        {
            var ply = Players[sender];
            if (ply == null) return;

            ply.Position = packet.position;
            ply.Rotation = packet.rotation;
        }

        #endregion
    }
}
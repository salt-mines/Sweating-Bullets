using Lidgren.Network;
using Networking.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    internal class Server
    {
        private readonly NetServer server;

        public byte MaxPlayerCount { get; }
        public byte PlayerCount { get; private set; }

        public PlayerInfo[] Players { get; }
        public PlayerState?[] WorldState { get; }

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

        public byte GetFreePlayerId()
        {
            for (byte i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null) return i;
            }

            throw new IndexOutOfRangeException("Too many players, no IDs left");
        }

        public PlayerInfo CreatePlayer(byte? id = null, NetConnection connection = null)
        {
            if (!id.HasValue) id = GetFreePlayerId();

            var ply = new PlayerInfo(id.Value, connection);
            if (connection != null)
            {
                connection.Tag = ply;
            }

            Players[ply.Id] = ply;

            return ply;
        }

        public PlayerInfo GetPlayerInfo(NetConnection connection)
        {
            return connection.Tag as PlayerInfo;
        }

        public PlayerInfo GetPlayerInfo(NetIncomingMessage msg)
        {
            return GetPlayerInfo(msg.SenderConnection);
        }

        public void SendToOne(NetOutgoingMessage msg, NetConnection connection, NetDeliveryMethod method)
        {
            server.SendMessage(msg, connection, method);
        }

        public void SendToAll(NetOutgoingMessage msg, NetDeliveryMethod method)
        {
            if (server.ConnectionsCount == 0) return;

            server.SendMessage(msg, server.Connections, method, 0);
        }

        public void SendToAllButOne(NetOutgoingMessage msg, NetConnection exceptThis, NetDeliveryMethod method)
        {
            var clients = new List<NetConnection>(server.Connections);
            clients.Remove(exceptThis);

            server.SendMessage(msg, clients, method, 0);
        }

        protected void OnStatusMessage(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus)msg.ReadByte();
            var client = GetPlayerInfo(msg);
            switch (newStatus)
            {
                case NetConnectionStatus.Connected:
                    OnPlayerConnected(client);
                    break;
                case NetConnectionStatus.Disconnecting:
                case NetConnectionStatus.Disconnected:
                    OnPlayerDisconnected(client);
                    break;
            }
        }

        private void OnPlayerConnected(PlayerInfo player)
        {
            Debug.LogFormat("Conn [{0}]: {1}", this, player.Id);

            PlayerCount++;

            //players.

            SendToOne(Packet.Write(server, new Connected
            {
                playerId = player.Id,
                maxPlayers = MaxPlayerCount
            }), player.Connection, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnPlayerDisconnected(PlayerInfo player)
        {
            Debug.LogFormat("DC [{0}]: {1}", this, player.Id);

            PlayerCount--;

            //connectedClients.Remove(client);

            SendToAll(Packet.Write(server, new PlayerDisconnected { playerId = player.Id }), NetDeliveryMethod.ReliableUnordered);
        }

        protected void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType)msg.ReadByte();
            var packet = Packet.GetPacketFromType(type).Read(msg);
            var sender = GetPlayerInfo(msg).Id;

            switch (type)
            {
                case PacketType.PlayerMove:
                    OnPlayerMove(sender, (PlayerMove)packet);
                    break;
                default:
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

        private int tick;
        public void FixedUpdate()
        {
            tick++;

            if (tick % 4 != 0) return;

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

            SendToAll(Packet.Write(server, new WorldState { worldState = WorldState }), NetDeliveryMethod.UnreliableSequenced);
        }
    }
}
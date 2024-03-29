﻿using System;
using System.Collections.Generic;
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
        private float changeMapAt;

        public Server(NetworkManager nm, ServerConfig config, Loader loader)
        {
            if (config == null)
                config = new ServerConfig
                {
                    MaxPlayerCount = Constants.MaxPlayers,
                    StartingLevel = loader.LevelManager.StartingLevel,
                    GameMode = loader.availableGameModes[0],
                    KillsToWin = loader.availableGameModes[0].killsTarget
                };

            MaxPlayerCount = config.MaxPlayerCount;
            Players = new PlayerInfo[MaxPlayerCount];
            WorldState = new PlayerState?[MaxPlayerCount];

            Loader = loader;
            LevelManager = loader.LevelManager;
            LevelManager.StartingLevel = config.StartingLevel;

            NetworkManager = nm;

            ServerConfig = config;

            ServerConfig.GameMode.killsTarget = ServerConfig.KillsToWin;

            server = new NetServer(new NetPeerConfiguration(Constants.AppName)
            {
                Port = Constants.AppPort,
                MaximumConnections = MaxPlayerCount,
                EnableUPnP = true,
#if UNITY_EDITOR
                ConnectionTimeout = 600
#endif
            });

            server.Configuration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            server.Start();

            LevelManager.CurrentLevel = ServerConfig.StartingLevel;
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

        public ServerConfig ServerConfig { get; }

        public byte MaxPlayerCount { get; }
        public byte PlayerCount { get; private set; }

        public PlayerInfo[] Players { get; }
        public PlayerState?[] WorldState { get; }

        public bool GameOver { get; set; }

        public string Level => LevelManager.CurrentLevel;
        public bool ListenServer => NetworkManager && NetworkManager.Mode == NetworkManager.NetworkMode.ListenServer;

        public event EventHandler<PlayerInfo> PlayerJoined;
        public event EventHandler<PlayerInfo> PlayerLeft;
        public event EventHandler<PlayerPreferences> PlayerSentPreferences;

        public event EventHandler<PlayerDeath> PlayerDied;
        public event EventHandler<PlayerShoot> PlayerShot;

        public void Update()
        {
            var time = Time.time;
            if (time >= nextTick)
            {
                ProcessMessages();
                nextTick = time + 1f / TickRate;
            }

            if (GameOver && Time.time >= changeMapAt)
            {
                GameOver = false;
                ChangeLevel(LevelManager.GetNextLevel());
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

        public void ChangeLevel(string level)
        {
            if (!LevelManager.IsValidLevel(level)) return;

            foreach (var pl in Players)
            {
                if (pl == null) continue;

                pl.Kills = 0;
                pl.Deaths = 0;
            }

            if (!ListenServer)
                LevelManager.ChangeLevel(level);

            SendToAll(new ChangeLevel
            {
                nextLevel = level
            }, NetDeliveryMethod.ReliableUnordered);
        }

        public void EndGame(byte winner)
        {
            GameOver = true;
            changeMapAt = Time.time + ServerConfig.GameMode.mapChangeTime;

            SendToAll(new GameOver
            {
                winnerId = winner,
                mapChangeTime = ServerConfig.GameMode.mapChangeTime
            }, NetDeliveryMethod.ReliableUnordered);
        }

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

            return ply;
        }

        public void RemovePlayer(PlayerInfo ply)
        {
            RemovePlayer(ply.Id);
        }

        public void RemovePlayer(byte id)
        {
            if (Players[id] == null) return;

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

        internal List<PlayerPreferences> BuildPlayerList(byte? excludeId = null)
        {
            var list = new List<PlayerPreferences>(Players.Length);
            foreach (var pl in Players)
            {
                if (pl == null || excludeId == pl.Id) continue;

                list.Add(new PlayerPreferences
                {
                    playerId = pl.Id,
                    name = pl.Name,
                    color = pl.Color,
                    modelId = pl.Model
                });
            }

            return list;
        }

        internal List<PlayerExtraInfo> BuildPlayerInfoList(byte? excludeId = null)
        {
            var list = new List<PlayerExtraInfo>(Players.Length);
            foreach (var pl in Players)
            {
                if (pl == null || excludeId == pl.Id) continue;

                list.Add(new PlayerExtraInfo
                {
                    playerId = pl.Id,
                    kills = pl.Kills,
                    deaths = pl.Deaths
                });
            }

            return list;
        }

        private void OnPlayerConnected(NetConnection connection)
        {
            var player = CreatePlayer(false, null, connection);

            Debug.LogFormat("Conn [{0}]: {1}", "Server", player.Id);

            PlayerJoined?.Invoke(this, player);

            var currentModeId = (byte) Loader.availableGameModes.IndexOf(ServerConfig.GameMode);

            SendToOne(new Connected
            {
                playerId = player.Id,
                maxPlayers = MaxPlayerCount,
                modeId = currentModeId,
                levelName = Level,
                currentPlayers = BuildPlayerList(player.Id),
                currentPlayersInfo = BuildPlayerInfoList(player.Id)
            }, player.Connection, NetDeliveryMethod.ReliableUnordered);

            SendToAll(new PlayerConnected
            {
                playerId = player.Id
            }, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnPlayerDisconnected(PlayerInfo player)
        {
            Debug.LogFormat("DC [{0}]: {1}", "Server", player.Id);

            PlayerLeft?.Invoke(this, player);

            SendToAll(new PlayerDisconnected {playerId = player.Id}, NetDeliveryMethod.ReliableUnordered);

            RemovePlayer(player);
        }

        private void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();
            var sender = GetPlayerInfo(msg).Id;

            switch (type)
            {
                case PacketType.PlayerPreferences:
                    PacketReceived(sender, PlayerPreferences.Read(msg));
                    break;
                case PacketType.PlayerState:
                    PacketReceived(sender, PlayerState.Read(msg));
                    break;
                case PacketType.PlayerDeath:
                    PacketReceived(sender, PlayerDeath.Read(msg));
                    break;
                case PacketType.PlayerShoot:
                    PacketReceived(sender, PlayerShoot.Read(msg));
                    break;
            }
        }

        private void PacketReceived(byte sender, PlayerPreferences packet)
        {
            var ply = Players[sender];
            if (ply == null)
            {
                Debug.LogWarning("Received PlayerPreferences without player existing");
                return;
            }

            if (sender != packet.playerId)
                return;

            Debug.Log($"Setting player {sender}'s name to '{packet.name}'");
            ply.Name = packet.name;
            ply.Color = packet.color;
            ply.Model = packet.modelId;

            SendToAll(packet, NetDeliveryMethod.ReliableUnordered);

            PlayerSentPreferences?.Invoke(this, packet);
        }

        private void PacketReceived(byte sender, PlayerState packet)
        {
            Players[sender]?.SetFromState(packet);
        }

        private void PacketReceived(byte sender, PlayerShoot packet)
        {
            if (Players[sender] == null)
                return;

            if (sender != packet.playerId)
                return;

            SendToAll(packet, NetDeliveryMethod.ReliableUnordered);

            PlayerShot?.Invoke(this, packet);
        }

        private void PacketReceived(byte sender, PlayerDeath packet)
        {
            short deaths = 0;
            if (Players[sender] != null) deaths = ++Players[sender].Deaths;

            short kills = 0;
            if (packet.killerId == sender && Players[sender] != null)
                kills = Players[sender].Kills;
            else if (Players[packet.killerId] != null) kills = ++Players[packet.killerId].Kills;

            var death = new PlayerDeath
            {
                playerId = sender,
                killerId = packet.killerId,
                playerDeaths = deaths,
                killerKills = kills
            };
            SendToAll(death, NetDeliveryMethod.ReliableUnordered);

            PlayerDied?.Invoke(this, death);

            var killsTarget = ServerConfig.GameMode.killsTarget;
            if (killsTarget > 0 && kills >= ServerConfig.GameMode.killsTarget) EndGame(packet.killerId);
        }

        #endregion
    }
}
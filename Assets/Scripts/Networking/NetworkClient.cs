﻿using System;
using Game;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Networking
{
    public sealed class NetworkClient : Client
    {
        private readonly NetClient client;

        private bool preloadedLevel;

        internal NetworkClient(bool listenServer, Loader loader)
        {
            ListenServer = listenServer;

            Loader = loader;
            LevelManager = Loader.LevelManager;

            Loader.LevelLoaded += (o, s) => OnLevelLoaded(s);

            client = new NetClient(new NetPeerConfiguration(Constants.AppName)
            {
#if UNITY_EDITOR
                ConnectionTimeout = 600
#endif
            });

            client.Start();
        }

        private bool ListenServer { get; }

        private Loader Loader { get; }
        private LevelManager LevelManager { get; }

        public event EventHandler<StatusChangeEvent> StatusChanged;

        public override void Connect(string host, int port = Constants.AppPort)
        {
            client.Connect(host, port);
        }

        public override void Shutdown()
        {
            client.Shutdown("Bye");

            base.Shutdown();
        }

        protected override void InitializeFromServer(Connected packet)
        {
            base.InitializeFromServer(packet);

            if (preloadedLevel)
                OnLevelLoaded(packet.levelName);
            else
                LevelManager.ChangeLevel(packet.levelName);
        }

        protected override void OnLevelLoaded(string level)
        {
            if (!PlayerId.HasValue)
            {
                // If the scene was open in editor, this method gets called before we have actually joined the local
                // server. If this bool is set to true, LevelLoaded is called again once server has sent its
                // welcome packet.
                preloadedLevel = true;
                return;
            }

            Loaded = true;
            CreatePlayer(PlayerId.Value, true);

            if (Players != null)
                foreach (var pl in Players)
                {
                    if (pl == null || pl.Id == PlayerId.Value) continue;

                    pl.PlayerObject = NetworkManager.CreatePlayer(pl);
                }

            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");

            // Send our chosen player name and a random color to identify us.
            var info = new PlayerPreferences
            {
                playerId = PlayerId.Value,
                name = Preferences.Name,
                color = Random.ColorHSV(0, 1f, 0.3f, 1f, 0.5f, 1f),
                modelId = (byte) Random.Range(0, 3)
            };

            Send(info, NetDeliveryMethod.ReliableUnordered);
            OnPlayerSentPreferences(info);

            CreateServerPlayers();

            base.OnLevelLoaded(level);
        }

        protected override PlayerInfo CreatePlayer(byte id, bool local = false)
        {
            var ply = base.CreatePlayer(id, local);
            ply.PlayerObject = NetworkManager.CreatePlayer(ply, local);
            return ply;
        }

        protected override void RemovePlayer(byte id)
        {
            if (Players[id] != null)
                NetworkManager.RemovePlayer(Players[id].PlayerObject);

            base.RemovePlayer(id);
        }

        private void Send<T>(T packet, NetDeliveryMethod method) where T : IPacket
        {
            client.SendMessage(Packet.Write(client, packet), method);
        }

        protected override void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
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
                        NetworkLog.HandleMessage("Client", msg);
                        break;
                }

                client.Recycle(msg);
            }
        }

        protected override void SendState()
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            var ply = Players[PlayerId.Value];

            Send(ply.GetState(), NetDeliveryMethod.UnreliableSequenced);

            if (ply.Teleported)
                ply.Teleported = false;
        }

        /// <summary>
        ///     Called when local player has shot.
        /// </summary>
        /// <param name="from">Where the shot was shot from</param>
        /// <param name="to">Where the shot ended</param>
        /// <param name="damage">How much damage the shot does</param>
        /// <param name="hit">RaycastHit if shot hit something, null otherwise</param>
        public void ShootOne(Vector3 from, Vector3 to, byte damage, RaycastHit hit)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Send(new PlayerShoot
            {
                playerId = PlayerId.Value,
                from = from,
                bullets = new[] {BulletInfo.From(to, damage, hit)}
            }, NetDeliveryMethod.ReliableUnordered);
        }

        public void ShootMultiple(Vector3 from, BulletInfo[] bullets)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Send(new PlayerShoot
            {
                playerId = PlayerId.Value,
                from = from,
                bullets = bullets
            }, NetDeliveryMethod.ReliableUnordered);
        }

        public void OnDeath(byte shooter)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Send(new PlayerDeath
            {
                playerId = PlayerId.Value,
                killerId = shooter
            }, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnStatusMessage(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus) msg.ReadByte();
            var reason = msg.ReadString();

            StatusChanged?.Invoke(this, new StatusChangeEvent
            {
                Status = status,
                Reason = reason
            });
        }

        private void OnDataMessage(NetIncomingMessage msg)
        {
            var type = (PacketType) msg.ReadByte();

            if (type == PacketType.Connected)
                InitializeFromServer(Packets.Connected.Read(msg));

            if (!Loaded) return;

            switch (type)
            {
                case PacketType.PlayerPreferences:
                    PacketReceived(PlayerPreferences.Read(msg));
                    break;
                case PacketType.PlayerConnected:
                    PacketReceived(PlayerConnected.Read(msg));
                    break;
                case PacketType.PlayerDisconnected:
                    PacketReceived(PlayerDisconnected.Read(msg));
                    break;
                case PacketType.WorldState:
                    AddWorldState(WorldState.Read(msg).worldState);
                    break;
                case PacketType.PlayerDeath:
                    PacketReceived(Packets.PlayerDeath.Read(msg));
                    break;
                case PacketType.PlayerShoot:
                    PacketReceived(PlayerShoot.Read(msg));
                    break;
                case PacketType.ChangeLevel:
                    PacketReceived(ChangeLevel.Read(msg));
                    break;
                case PacketType.GameOver:
                    PacketReceived(Packets.GameOver.Read(msg));
                    break;
            }
        }

        private void PacketReceived(GameOver packet)
        {
            OnGameOver(packet);
        }

        private void PacketReceived(ChangeLevel packet)
        {
            Loaded = false;
            OnChangeLevel(packet);

            LevelManager.ChangeLevel(packet.nextLevel);
        }

        private void PacketReceived(PlayerConnected packet)
        {
            if (PlayerId == packet.playerId) return;

            var p = CreatePlayer(packet.playerId);
            UnityEngine.Debug.Log($"Id: {p.Id} joined");
        }

        private void PacketReceived(PlayerDisconnected packet)
        {
            RemovePlayer(packet.playerId);
        }

        private void PacketReceived(PlayerPreferences info)
        {
            OnPlayerSentPreferences(info);
        }

        private void PacketReceived(PlayerDeath packet)
        {
            OnPlayerDeath(packet);
        }

        private void PacketReceived(PlayerShoot packet)
        {
            if (Players == null || LocalPlayer == null || packet.playerId == PlayerId) return;

            var ply = Players[packet.playerId];

            if (!ply.PlayerObject.playerMechanics.CurrentWeapon) return;

            foreach (var b in packet.bullets)
            {
                var hit = new BulletInfo();
                if (b.hit)
                    hit = new BulletInfo
                    {
                        hit = true,
                        hitPlayer = b.hitPlayer,
                        hitNormal = b.hitNormal
                    };
                ply.PlayerObject.playerMechanics.CurrentWeapon.ShootEffect(ply.PlayerObject, packet.from, b.to, hit);

                Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
                if (b.hitPlayer && b.victimId == PlayerId.Value) OnSelfHurt(packet.playerId, b.damage);
            }
        }

        internal override void OnGUI(float x, float y)
        {
#if UNITY_EDITOR
            GUI.Box(new Rect(x, y += 20, 140, 100), "Client");
            var rtt = 0;
            if (client.ServerConnection != null)
                rtt = Mathf.RoundToInt(client.ServerConnection.AverageRoundtripTime * 1000);

            GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Lag: {rtt} ms");
            GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Interp: {Interpolation * 1000} ms");
            if (PlayerId.HasValue && Players[PlayerId.Value] != null)
            {
                var ply = Players[PlayerId.Value];
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Pos: {ply.Position}");
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Rot: {ply.ViewAngles}");
            }
#endif
        }

        public class StatusChangeEvent : EventArgs
        {
            public NetConnectionStatus Status { get; set; }
            public string Reason { get; set; }
        }
    }
}
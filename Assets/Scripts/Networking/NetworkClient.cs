using System;
using System.Collections.Generic;
using Game;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Networking
{
    internal sealed class NetworkClient : Client
    {
        private readonly NetClient client;

        internal NetworkClient(Loader loader)
        {
            Loader = loader;
            LevelManager = Loader.LevelManager;

            Loader.LevelLoaded += (o, s) => LevelLoaded(s);

            client = new NetClient(new NetPeerConfiguration(Constants.AppName)
            {
#if UNITY_EDITOR
                ConnectionTimeout = 600
#endif
            });

            client.Start();
        }

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

        protected override void InitializeFromServer(byte playerId, byte maxPlayers, string level,
            List<PlayerExtraInfo> currentPlayers)
        {
            base.InitializeFromServer(playerId, maxPlayers, level, currentPlayers);

            LevelManager.ChangeLevel(level);
        }

        private void LevelLoaded(string level)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");

            Loaded = true;
            CreatePlayer(PlayerId.Value, true);

            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            var info = new PlayerExtraInfo
            {
                playerId = PlayerId.Value,
                name = Preferences.Name
            };
            Send(info, NetDeliveryMethod.ReliableUnordered);
            OnPlayerSentInfo(info);
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
        }

        public override void PlayerShoot(Vector3 from, Vector3 to)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            Send(new PlayerShoot
            {
                playerId = PlayerId.Value,
                from = from,
                to = to
            }, NetDeliveryMethod.ReliableUnordered);
        }

        public override void PlayerKill(byte targetId)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            UnityEngine.Debug.LogFormat("Shooting Player {0}", targetId);

            Send(new PlayerKill
            {
                killerId = PlayerId.Value,
                targetId = targetId
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

            switch (type)
            {
                case PacketType.Connected:
                    InitializeFromServer(Packets.Connected.Read(msg));
                    break;
                case PacketType.PlayerExtraInfo:
                    ReceivedPlayerSentInfo(PlayerExtraInfo.Read(msg));
                    break;
                case PacketType.PlayerConnected:
                    ReceivedPlayerConnected(PlayerConnected.Read(msg));
                    break;
                case PacketType.PlayerDisconnected:
                    ReceivedPlayerDisconnected(PlayerDisconnected.Read(msg));
                    break;
                case PacketType.WorldState:
                    AddWorldState(WorldState.Read(msg).worldState);
                    break;
                case PacketType.PlayerDeath:
                    ReceivedPlayerDeath(PlayerDeath.Read(msg));
                    break;
                case PacketType.PlayerShoot:
                    ReceivedPlayerShoot(Packets.PlayerShoot.Read(msg));
                    break;
            }
        }
        
        private void ReceivedPlayerConnected(PlayerConnected packet)
        {
            if (PlayerId == packet.playerId) return;
            
            var p = CreatePlayer(packet.playerId);
            UnityEngine.Debug.Log($"Id: {p.Id} joined");
        }

        private void ReceivedPlayerDisconnected(PlayerDisconnected packet)
        {
            RemovePlayer(packet.playerId);
        }

        private void ReceivedPlayerSentInfo(PlayerExtraInfo info)
        {
            OnPlayerSentInfo(info);
        }

        private void ReceivedPlayerDeath(PlayerDeath packet)
        {
            Debug.Assert(PlayerId != null, nameof(PlayerId) + " != null");
            if (packet.playerId == PlayerId.Value)
            {
                Players[PlayerId.Value].PlayerObject.Kill();
                UnityEngine.Debug.Log("Death!");
            }

            UnityEngine.Debug.LogFormat("Player {0} killed Player {1}", packet.killerId, packet.playerId);
        }

        private void ReceivedPlayerShoot(PlayerShoot packet)
        {
            if (Players == null || LocalPlayer == null || packet.playerId == PlayerId) return;

            LocalPlayer.PlayerObject.GetComponent<PlayerShooting>().SpawnLine(packet.from, packet.to);
        }

        internal override void OnGUI(float x, float y)
        {
            GUI.Box(new Rect(x, y += 20, 140, 100), "Client");
            var rtt = 0;
            if (client.ServerConnection != null)
                rtt = Mathf.RoundToInt(client.ServerConnection.AverageRoundtripTime * 1000);

            GUI.Label(new Rect(x + 5, y += 20, 140, 20),
                $"Lag: {rtt} ms");
            GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Interp: {Interpolation * 1000} ms");
            if (PlayerId.HasValue && Players[PlayerId.Value] != null)
            {
                var ply = Players[PlayerId.Value];
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Pos: {ply.Position}");
                GUI.Label(new Rect(x + 5, y += 20, 140, 20), $"Rot: {ply.ViewAngles}");
            }
        }

        public class StatusChangeEvent : EventArgs
        {
            public NetConnectionStatus Status { get; set; }
            public string Reason { get; set; }
        }
    }
}
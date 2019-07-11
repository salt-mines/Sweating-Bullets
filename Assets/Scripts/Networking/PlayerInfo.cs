using System.Collections.Generic;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;

namespace Networking
{
    public class PlayerInfo
    {
        public PlayerInfo(byte id)
        {
            Id = id;
        }

        public PlayerInfo(byte id, NetConnection connection)
            : this(id)
        {
            Connection = connection;
        }

        public byte Id { get; }

        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public bool Alive { get; set; } = true;

        public NetworkPlayer PlayerObject { get; set; }
        public NetConnection Connection { get; }

        public LinkedList<TimedPlayerState> StateBuffer { get; } = new LinkedList<TimedPlayerState>();

        public PlayerState GetState()
        {
            return new PlayerState
            {
                playerId = Id,
                position = Position,
                rotation = Rotation,
                alive = Alive
            };
        }

        public void SetFromState(PlayerState state)
        {
            Position = state.position;
            Rotation = state.rotation;
            Alive = state.alive;
        }
        
        public PlayerMove CreateMove()
        {
            return new PlayerMove
            {
                playerId = Id,
                position = Position,
                rotation = Rotation,
                alive = Alive
            };
        }

        public void SetFromPacket(PlayerMove packet)
        {
            Position = packet.position;
            Rotation = packet.rotation;
            Alive = packet.alive;
        }
    }
}
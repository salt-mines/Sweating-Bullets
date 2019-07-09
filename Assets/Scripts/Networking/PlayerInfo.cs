using Lidgren.Network;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class PlayerInfo
    {
        public byte Id { get; }

        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public NetConnection Connection { get; }

        public LinkedList<TimedPlayerState> StateBuffer { get; } = new LinkedList<TimedPlayerState>();

        public PlayerInfo(byte id)
        {
            Id = id;
        }

        public PlayerInfo(byte id, NetConnection connection)
        {
            Id = id;
            Connection = connection;
        }
    }
}

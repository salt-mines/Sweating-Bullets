﻿using System.Collections.Generic;
using Lidgren.Network;
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

        public NetworkPlayer PlayerObject { get; set; }
        public NetConnection Connection { get; }

        public LinkedList<TimedPlayerState> StateBuffer { get; } = new LinkedList<TimedPlayerState>();

        public void SetFromState(PlayerState state)
        {
            Position = state.position;
            Rotation = state.rotation;
        }
    }
}
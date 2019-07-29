using System.Collections.Generic;
using System.Diagnostics;
using Lidgren.Network;
using Networking.Packets;
using UnityEngine;
using NetworkPlayer = Game.NetworkPlayer;

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

        public string Name { get; set; } = "Player";
        public Color Color { get; set; } = Color.white;

        public Vector3 Position { get; set; } = Vector3.zero;
        public Vector3 Velocity { get; set; } = Vector3.zero;
        public Vector2 ViewAngles { get; set; } = Vector2.zero;

        public byte Health { get; set; } = 100;
        public byte Weapon { get; set; } = 0;

        public short Kills { get; set; }
        public short Deaths { get; set; }

        public bool Grounded { get; set; }
        public bool Teleported { get; set; }
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
                velocity = Velocity,
                viewAngles = ViewAngles,
                health = Health,
                weapon = Weapon,
                grounded = Grounded,
                teleported = Teleported,
                alive = Alive
            };
        }

        public void SetFromState(PlayerState state)
        {
            Position = state.position;
            Velocity = state.velocity;
            ViewAngles = state.viewAngles;
            Health = state.health;
            Weapon = state.weapon;
            Grounded = state.grounded;
            Teleported = state.teleported;
            Alive = state.alive;
        }

        public override string ToString()
        {
            return $"Player[{Id}] {{ {Name}; {(Alive ? "Alive" : "Dead")} }}";
        }
    }
}
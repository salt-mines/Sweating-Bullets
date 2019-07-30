using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Game Mode", menuName = "Game Mode", order = 0)]
    public class GameMode : ScriptableObject
    {
        public string modeName;

        [ReorderableList]
        public List<Weapon> weapons;

        public bool itemsEnabled = true;

        [Range(0, 20)]
        public float spawnTime = 6f;

        [Range(0, 255)]
        public byte maxHealth = 100;

        [Range(0, short.MaxValue)]
        public short killsTarget = 30;

        private void Awake()
        {
            if (weapons.Count > 255)
                throw new IndexOutOfRangeException("too many weapons");
        }

        private void OnValidate()
        {
            if (weapons.Count > 255)
                throw new IndexOutOfRangeException("too many weapons");
        }
    }
}
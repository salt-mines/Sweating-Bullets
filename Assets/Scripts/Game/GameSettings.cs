using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [ReorderableList]
        public List<Weapon> weapons;

        [Range(0, 20)]
        public float spawnTime = 6f;

        [Range(1, 255)]
        public byte maxHealth = 100;
    }
}
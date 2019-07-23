using System;
using UnityEngine;

namespace Game
{
    public class Viewmodel : MonoBehaviour
    {
        public Transform barrelPoint;

        public Animator animator;
        private static readonly int ShootParam = Animator.StringToHash("Shoot");

        private void Start()
        {
            if (!animator)
                animator = GetComponent<Animator>();
        }

        public void Shoot()
        {
            animator.SetTrigger(ShootParam);
        }
    }
}
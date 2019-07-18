using System;
using Networking;
using UnityEngine;

namespace Game
{
    public class NetworkPlayer : MonoBehaviour
    {
        private static readonly int ParamSpeed = Animator.StringToHash("Speed");
        private static readonly int ParamForward = Animator.StringToHash("Forward");
        private static readonly int ParamRight = Animator.StringToHash("Right");

        [SerializeField]
        private bool isLocalPlayer;

        public Transform rootBone;
        public Transform upperBodyBone;

        public Animator animator;
        public PlayerMovement playerMovement;
        public FirstPersonCamera firstPersonCamera;

        public byte Id => PlayerInfo.Id;

        public PlayerInfo PlayerInfo { get; internal set; }

        public Client NetworkClient { get; set; }

        public bool IsLocalPlayer
        {
            get => isLocalPlayer;
            set => isLocalPlayer = value;
        }

        private void Update()
        {
            var tr = transform;
            if (IsLocalPlayer)
            {
                PlayerInfo.Position = tr.position;
                PlayerInfo.Velocity = playerMovement.Velocity;
                PlayerInfo.ViewAngles = firstPersonCamera.ViewAngles;
                PlayerInfo.Alive = GetComponent<PlayerMechanics>().isAlive;
            }
            else
            {
                tr.position = PlayerInfo.Position;
                tr.rotation = Quaternion.AngleAxis(PlayerInfo.ViewAngles.x, Vector3.up);
                tr.GetChild(0).gameObject.SetActive(PlayerInfo.Alive);
                tr.GetChild(1).gameObject.SetActive(PlayerInfo.Alive);
            }

            var groundVel = tr.InverseTransformVector(PlayerInfo.Velocity);
            groundVel.y = 0;
            animator.SetFloat(ParamSpeed, groundVel.magnitude);
            animator.SetFloat(ParamForward, groundVel.z);
            animator.SetFloat(ParamRight, groundVel.x);
        }

        private void LateUpdate()
        {
            var lowerBodyDirection = transform.InverseTransformVector(PlayerInfo.Velocity);
            lowerBodyDirection.y = 0;

            if (lowerBodyDirection.z < -1f)
            {
                lowerBodyDirection.z = -lowerBodyDirection.z;
                lowerBodyDirection.x = -lowerBodyDirection.x;
            }

            var localRot = upperBodyBone.localRotation;
            var upDown = Quaternion.Inverse(localRot) * Quaternion.AngleAxis(PlayerInfo.ViewAngles.y, Vector3.right);
            var upperBodyRotation = upperBodyBone.rotation * upDown * localRot;

            if (lowerBodyDirection.magnitude > 0f)
            {
                var rot = new Quaternion();
                rot.SetLookRotation(lowerBodyDirection);

                rootBone.rotation *= rot;
            }

            upperBodyBone.rotation = upperBodyRotation;
        }

        public void Kill()
        {
            GetComponent<PlayerMechanics>()?.Kill();
        }

        public void Shoot(NetworkPlayer target)
        {
            NetworkClient.PlayerShoot(target.Id);
        }
    }
}
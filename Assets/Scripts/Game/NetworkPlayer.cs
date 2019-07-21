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

        public Client Client { get; set; }

        public bool IsLocalPlayer
        {
            get => isLocalPlayer;
            set => isLocalPlayer = value;
        }

        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
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
                if (PlayerInfo.Teleported)
                    PlayerInfo.Teleported = false;
                    
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

        public void Teleport(Vector3 position)
        {
            var oldState = true;
            if (characterController)
            {
                oldState = characterController.enabled;
                characterController.enabled = false;
            }

            transform.position = position;
            PlayerInfo.Position = position;
            PlayerInfo.Teleported = true;

            if (characterController)
                characterController.enabled = oldState;
        }

        public void Teleport(Vector3 position, Vector2 viewAngles)
        {
            Teleport(position);

            transform.rotation = Quaternion.AngleAxis(viewAngles.x, Vector3.up);
            if (firstPersonCamera)
                firstPersonCamera.SetAngles(viewAngles);
            PlayerInfo.ViewAngles = viewAngles;
        }
        
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            Teleport(position);
            
            transform.rotation = rotation;
            if (firstPersonCamera)
                firstPersonCamera.SetAngles(new Vector2(rotation.eulerAngles.y, 0));
            PlayerInfo.ViewAngles = firstPersonCamera.ViewAngles;
        }

        public void Kill()
        {
            GetComponent<PlayerMechanics>()?.Kill();
        }

        public void Shoot(Vector3 from, Vector3 to)
        {
            Client.PlayerShoot(from, to);
        }

        public void Kill(NetworkPlayer target)
        {
            Client.PlayerKill(target.Id);
        }
    }
}
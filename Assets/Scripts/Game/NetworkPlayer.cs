using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Game
{
    public class NetworkPlayer : MonoBehaviour
    {
        private static readonly int ParamSpeed = Animator.StringToHash("Speed");
        private static readonly int ParamForward = Animator.StringToHash("Forward");
        private static readonly int ParamRight = Animator.StringToHash("Right");
        private static readonly int FootstepParam = Animator.StringToHash("Footstep");

        [SerializeField]
        private bool isLocalPlayer;

        public Transform eyePosition;
        public Transform modelParent;

        public Animator animator;
        public PlayerMechanics playerMechanics;
        public PlayerMovement playerMovement;
        public FirstPersonCamera firstPersonCamera;

        private CharacterController characterController;

        public AudioSource audioSource;
        public List<AudioClip> footstepClips = new List<AudioClip>(2);
        private int lastFootStepClip = 0;

        private byte modelId;
        private float lastStepValue;

        public byte Id => PlayerInfo.Id;

        public PlayerInfo PlayerInfo { get; internal set; }

        public NetworkClient Client { get; set; }

        public bool IsLocalPlayer
        {
            get => isLocalPlayer;
            set => isLocalPlayer = value;
        }

        private void Awake()
        {
            playerMechanics = GetComponent<PlayerMechanics>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (PlayerInfo == null) return;

            var tr = transform;
            if (IsLocalPlayer)
            {
                var respawned = !PlayerInfo.Alive && playerMechanics.IsAlive;
                PlayerInfo.Position = tr.position;
                PlayerInfo.Velocity = playerMovement.Velocity;
                PlayerInfo.ViewAngles = firstPersonCamera.ViewAngles;
                PlayerInfo.Alive = playerMechanics.IsAlive;
                PlayerInfo.Health = playerMechanics.Health;
                PlayerInfo.Weapon = playerMechanics.CurrentWeaponId;
                PlayerInfo.Grounded = playerMovement.IsGrounded;

                if (respawned) Client.OnPlayerRespawn(PlayerInfo);
            }
            else
            {
                if (PlayerInfo.Teleported)
                    PlayerInfo.Teleported = false;

                tr.position = PlayerInfo.Position;
                tr.rotation = Quaternion.AngleAxis(PlayerInfo.ViewAngles.x, Vector3.up);
                eyePosition.localRotation = Quaternion.AngleAxis(PlayerInfo.ViewAngles.y, Vector3.right);

                playerMechanics.Health = PlayerInfo.Health;

                if (playerMechanics.IsAlive && !PlayerInfo.Alive)
                {
                    Kill();
                }
                else if (!playerMechanics.IsAlive && PlayerInfo.Alive)
                {
                    playerMechanics.Respawn();
                    Client.OnPlayerRespawn(PlayerInfo);
                }

                if (playerMechanics.CurrentWeaponId != PlayerInfo.Weapon)
                    playerMechanics.SetWeapon(PlayerInfo.Weapon);
            }

            if (modelId != PlayerInfo.Model)
                SetModel(PlayerInfo.Model, modelId);

            var groundVel = tr.InverseTransformVector(PlayerInfo.Velocity);
            groundVel.y = 0;
            if (animator.isActiveAndEnabled)
            {
                animator.SetFloat(ParamSpeed, groundVel.magnitude);
                animator.SetFloat(ParamForward, groundVel.z);
                animator.SetFloat(ParamRight, groundVel.x);
            }

            var fsValue = animator.isActiveAndEnabled ? animator.GetFloat(FootstepParam) : 0;
            if (animator.isActiveAndEnabled && !Utils.SameSign(fsValue, lastStepValue) && PlayerInfo.Grounded)
            {
                var clip = footstepClips[lastFootStepClip];
                audioSource.PlayOneShot(clip, groundVel.magnitude / 7f);

                lastFootStepClip = lastFootStepClip == 0 ? 1 : 0;
            }

            lastStepValue = fsValue;
        }

        private void SetModel(byte id, byte oldModel)
        {
            var old = modelParent.GetChild(oldModel);
            var wep = old.GetComponent<PlayerAnimation>().weapon;
            old.gameObject.SetActive(false);

            var newModel = modelParent.GetChild(id);
            newModel.gameObject.SetActive(true);
            newModel.GetComponent<PlayerAnimation>().SetWeapon(wep);

            animator = newModel.GetComponent<Animator>();

            modelId = id;
        }

        public PlayerAnimation GetPlayerAnimation()
        {
            return modelParent.GetChild(modelId).GetComponent<PlayerAnimation>();
        }

        private void Teleport(Vector3 position)
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
            playerMechanics.Kill();
        }
    }
}
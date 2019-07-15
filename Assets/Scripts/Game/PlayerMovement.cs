using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        #region Unity properties

        [Range(0, 100)]
        public float movementSpeed = 6.0f;

        [Range(0, 100)]
        public float jumpSpeed = 8.0f;

        [Range(0, 100)]
        public float gravity = 20.0f;

        #endregion

        #region Movement state variables

        private Vector3 velocity = Vector3.zero;

        public Vector3 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        private Vector3 targetMovement = Vector3.zero;

        private bool hitCeiling;

        #endregion

        #region Components

        private GameInput gameInput;
        private CharacterController characterController;

        #endregion

        #region Unity events

        private void Start()
        {
            gameInput = FindObjectOfType<GameInput>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            targetMovement.x = gameInput.Strafe;
            targetMovement.z = gameInput.Forward;
            targetMovement.Normalize();
            targetMovement = transform.TransformDirection(targetMovement * movementSpeed);

            if (characterController.isGrounded && velocity.y <= 0)
            {
                hitCeiling = false;

                velocity.x = targetMovement.x;
                velocity.z = targetMovement.z;

                if (gameInput.Jump)
                    velocity.y = jumpSpeed;
            }

            var isColliding = (characterController.collisionFlags & CollisionFlags.Above) == CollisionFlags.Above;
            if (!hitCeiling && isColliding)
            {
                hitCeiling = true;
                velocity.y = 0;
            }

            if (!characterController.isGrounded) velocity.y -= gravity * Time.deltaTime;

            characterController.Move(velocity * Time.deltaTime);
        }

        #endregion

        #region Movement

        public void ResetMovement()
        {
            velocity = Vector3.zero;
            targetMovement = Vector3.zero;
        }

        #endregion
    }
}
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        #region Unity properties

        [Range(0, 50)]
        public float walkSpeed = 4.0f;

        [Range(0, 50)]
        public float runSpeed = 8.0f;

        [Range(0, 50)]
        public float jumpSpeed = 8.0f;

        [Range(0, 20)]
        public float gravityMultiplier = 2f;

        [Range(0, 20)]
        public float keepOnGroundForce = 2f;

        [Range(0, 1f)]
        public float groundDistanceTolerance = 0.1f;

        #endregion

        #region Movement state variables

        private Vector3 velocity = Vector3.zero;

        public Vector3 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public bool IsGrounded { get; private set; }

        private Vector2 input = Vector2.zero;

        private bool hitCeiling;
        private CollisionFlags collisionFlags;
        private bool isWalking;

        private Vector3 rayPositionOffset = Vector3.zero;

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
            var speed = GetInput();

            var tr = transform;
            var targetMovement = tr.forward * input.y + tr.right * input.x;

            rayPositionOffset.y = characterController.height / 2f;

            // Check if there's something solid close below us
            var didHit = Physics.SphereCast(tr.position + rayPositionOffset,
                characterController.radius * 0.8f, Vector3.down, out var hit,
                rayPositionOffset.y + groundDistanceTolerance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            var hitAngle = Vector3.Angle(Vector3.up, hit.normal);

            // Adjust direction based on ground slope, but only if the slope isn't too steep
            if (hitAngle < characterController.slopeLimit)
                targetMovement = Vector3.ProjectOnPlane(targetMovement, hit.normal).normalized;
            else
                targetMovement.Normalize();

            // Apply movement only when on (or near) ground and player isn't moving upwards
            if ((characterController.isGrounded || didHit) && velocity.y <= 0f)
            {
                IsGrounded = true;
                velocity.x = targetMovement.x * speed;
                velocity.z = targetMovement.z * speed;

                // Apply a bit of force to help keep us grounded
                if (characterController.isGrounded) velocity.y = -keepOnGroundForce;

                if (gameInput.Jump)
                {
                    velocity.y = jumpSpeed;
                }
            }
            else
            {
                IsGrounded = false;

                if (velocity.sqrMagnitude < runSpeed * runSpeed)
                {
                    velocity.x += (targetMovement.x * speed) * Time.deltaTime;
                    velocity.z += (targetMovement.z * speed) * Time.deltaTime;
                }
            }

            // Apply constant gravity to also help keep us grounded
            velocity += gravityMultiplier * Time.deltaTime * Physics.gravity;

            collisionFlags = characterController.Move(velocity * Time.deltaTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if ((collisionFlags & CollisionFlags.Below) == CollisionFlags.Below)
            {
                hitCeiling = false;
                return;
            }

            // Stop upwards velocity if we hit something above, and reset when we return to ground
            if ((collisionFlags & CollisionFlags.Above) != CollisionFlags.Above || hitCeiling ||
                velocity.y <= 0) return;

            hitCeiling = true;
            velocity.y = 0;
        }

        #endregion

        #region Movement

        private float GetInput()
        {
            input.x = gameInput.Strafe;
            input.y = gameInput.Forward;

            if (input.sqrMagnitude > 1)
                input.Normalize();

            isWalking = gameInput.Walk;

            return isWalking ? walkSpeed : runSpeed;
        }

        public void ResetMovement()
        {
            velocity = Vector3.zero;
            input = Vector2.zero;
        }

        #endregion
    }
}
using UnityEngine;
using UnityEngine.Animations;

namespace Game
{
    public class PlayerAnimation : MonoBehaviour
    {
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Land = Animator.StringToHash("Land");
        private static readonly int Jump = Animator.StringToHash("Jump");
        public bool ikEnabled = true;

        public Weapon weapon;

        public Transform weaponPivot;
        public Transform eyePosition;

        public Transform rightHandTarget;
        public Transform leftHandTarget;

        public NetworkPlayer networkPlayer;
        public Animator animator;

        private bool oldGrounded;

        private void Awake()
        {
            if (!networkPlayer)
                networkPlayer = GetComponent<NetworkPlayer>();

            if (!animator)
                animator = GetComponent<Animator>();

            if (!weapon) return;

            if (!rightHandTarget)
                rightHandTarget = weapon.rightHandIKTarget;

            if (!leftHandTarget)
                leftHandTarget = weapon.leftHandIKTarget;
        }

        private void OnEnable()
        {
            weaponPivot.GetComponent<PositionConstraint>().SetSource(0,
                new ConstraintSource
                {
                    sourceTransform = animator.GetBoneTransform(HumanBodyBones.Chest),
                    weight = 1
                });
        }

        public void SetWeapon(Weapon wep)
        {
            weapon = wep;

            if (!weapon) return;

            rightHandTarget = weapon.rightHandIKTarget;
            leftHandTarget = weapon.leftHandIKTarget;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!ikEnabled) return;

            if (networkPlayer.PlayerInfo == null) return;
            var grounded = networkPlayer.PlayerInfo.Grounded;

            animator.SetBool(Grounded, grounded);

            if (oldGrounded && !grounded)
            {
                animator.ResetTrigger(Land);
                animator.SetTrigger(Jump);
            }

            if (!oldGrounded && grounded)
            {
                animator.ResetTrigger(Jump);
                animator.SetTrigger(Land);
            }

            oldGrounded = grounded;

            var vel = networkPlayer.PlayerInfo.Velocity;

            var lowerBodyDirection = networkPlayer.transform.InverseTransformVector(vel);
            lowerBodyDirection.y = 0;

            if (lowerBodyDirection.z < -1f)
            {
                lowerBodyDirection.z = -lowerBodyDirection.z;
                lowerBodyDirection.x = -lowerBodyDirection.x;
            }

            if (layerIndex == 0 && lowerBodyDirection.magnitude > 0f)
            {
                var rot = new Quaternion();
                rot.SetLookRotation(lowerBodyDirection);

                animator.bodyRotation *= rot;

                return;
            }

            if (lowerBodyDirection.magnitude > 0f)
            {
                var rot = new Quaternion();
                rot.SetLookRotation(lowerBodyDirection);
                var q = animator.GetBoneTransform(HumanBodyBones.Spine).localRotation * Quaternion.Inverse(rot);
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, q);
            }

            var from = eyePosition.position;
            var dir = eyePosition.forward;

            var mask = LayerMask.GetMask("Default");
            var didHit = Physics.Raycast(from, dir, out var hit, float.PositiveInfinity, mask);
            var to = didHit ? hit.point : from + dir * 100;

            animator.SetLookAtPosition(to);
            animator.SetLookAtWeight(1);

            if (weaponPivot)
            {
                weaponPivot.LookAt(to);

                var angles = weaponPivot.localRotation.eulerAngles;
                if (angles.x > 180 && angles.x < 272)
                    angles.x = 272;
                if (angles.x <= 180 && angles.x > 87)
                    angles.x = 87;
                angles.y = 0;
                var q = Quaternion.Euler(angles);
                weaponPivot.localRotation = q;
            }

            if (rightHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            if (!leftHandTarget) return;

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }
}
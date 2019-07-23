using UnityEngine;

namespace Game
{
    public class GameInput : MonoBehaviour
    {
        public float mouseSensitivity = 6f;

        public float Forward { get; private set; }
        public float Strafe { get; private set; }

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }

        public bool Fire { get; private set; }

        public bool Jump { get; private set; }
        public bool Walk { get; private set; }

        public bool Cancel { get; private set; }

        public bool BlockInput { get; set; }

        public static bool MouseLocked
        {
            get => Cursor.lockState != CursorLockMode.None;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
            }
        }

        private void Update()
        {
            Cancel = Input.GetButtonDown("Cancel");

            if (BlockInput)
            {
                Forward = Strafe = MouseX = MouseY = 0;
                Fire = Jump = false;

                return;
            }

            Forward = Input.GetAxisRaw("Vertical");
            Strafe = Input.GetAxisRaw("Horizontal");

            MouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            MouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            Fire = Input.GetButton("Fire1");

            Jump = Input.GetButtonDown("Jump");
            Walk = Input.GetButton("Walk");
        }
    }
}
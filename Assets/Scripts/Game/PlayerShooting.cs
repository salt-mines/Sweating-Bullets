using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerShooting : MonoBehaviour
    {
        public Camera fpsCamera;
        public Weapon weapon;
        public Viewmodel viewmodel;

        private GameInput input;
        private NetworkPlayer player;

        // Start is called before the first frame update
        private void Start()
        {
            input = FindObjectOfType<GameInput>();
            player = GetComponent<NetworkPlayer>();

            if (!fpsCamera)
                fpsCamera = GetComponentInChildren<Camera>();

            if (!weapon)
                weapon = GetComponentInChildren<Weapon>();

            if (viewmodel && weapon && viewmodel.barrelPoint)
                weapon.barrelPoint = viewmodel.barrelPoint;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!input.Fire || !player.PlayerInfo.Alive || !weapon.CanShoot()) return;

            weapon.Shoot(fpsCamera.transform, player);

            if (viewmodel)
                viewmodel.Shoot();
        }
    }
}
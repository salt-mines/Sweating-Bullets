using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerShooting : MonoBehaviour
    {
        private GameInput input;
        private NetworkPlayer player;
        private Camera fpsCamera;

        public LayerMask hittableMask;

        public float range = 100f;
        public float rateOfFire = 1f;

        private float timeToFire;

        // Start is called before the first frame update
        private void Start()
        {
            input = FindObjectOfType<GameInput>();
            player = GetComponent<NetworkPlayer>();
            fpsCamera = GetComponentInChildren<Camera>();

            timeToFire = rateOfFire;
        }

        // Update is called once per frame
        private void Update()
        {
            if (player.PlayerInfo.Alive && input.Fire && rateOfFire <= timeToFire)
            {
                timeToFire = 0;
                Shoot();
            }

            timeToFire += Time.deltaTime;
        }

        private void Shoot()
        {
            var cameraTransform = fpsCamera.transform;

            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, range,
                hittableMask)) return;

            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * hit.distance, Color.yellow, 2,
                false);

            if (hit.transform.gameObject.layer != 9) return;

            var targetNetPlayer = hit.transform.gameObject.GetComponent<NetworkPlayer>();
            if (targetNetPlayer)
                player.Shoot(targetNetPlayer);
        }
    }
}
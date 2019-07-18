using UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMovement))]
    public class PlayerMechanics : MonoBehaviour
    {
        private Transform spawnPoints;

        public float spawnTime = 10f;
        public float timeSpentDead;

        public bool isAlive = true;

        private CharacterController characterController;
        private PlayerMovement playerMovement;
        private FirstPersonCamera playerCamera;
        private GameObject[] spawnPointList;

        private DeadOverlay uiDeadOverlay;

        // Start is called before the first frame update
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerMovement = GetComponent<PlayerMovement>();
            playerCamera = GetComponentInChildren<FirstPersonCamera>();

            uiDeadOverlay = FindObjectOfType<GameManager>().deadOverlay;

            if (!spawnPoints)
                spawnPoints = FindObjectOfType<LevelInfo>().spawnPointParent.transform;

            spawnPointList = new GameObject[spawnPoints.childCount];

            for (var i = 0; i < spawnPoints.childCount; i++)
            {
                var spawn = spawnPoints.GetChild(i).gameObject;
                if (!spawn.CompareTag("Respawn")) continue;

                spawnPointList[i] = spawn;
            }

            RespawnPlayer();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!isAlive) timeSpentDead += Time.deltaTime;

            if (spawnTime < timeSpentDead)
            {
                timeSpentDead = 0;
                RespawnPlayer();
                isAlive = true;
            }
        }

        public void Kill()
        {
            isAlive = false;

            characterController.enabled = false;
            playerMovement.enabled = false;
            foreach (Transform child in transform)
                if (child.gameObject.CompareTag("MainCamera"))
                    foreach (Transform cameraChild in child.transform)
                        cameraChild.gameObject.SetActive(false);
                else
                    child.gameObject.SetActive(false);

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(true);
        }

        public void RespawnPlayer()
        {
            playerMovement.enabled = true;

            var spawnPoint = spawnPointList[Random.Range(0, spawnPointList.Length)].transform;
            playerMovement.ResetMovement();
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            playerCamera.SetAngles(new Vector2(spawnPoint.transform.rotation.eulerAngles.y, 0));

            foreach (Transform child in transform)
            {
                foreach (Transform cameraChild in child.transform) cameraChild.gameObject.SetActive(true);

                child.gameObject.SetActive(true);
            }

            characterController.enabled = true;

            if (uiDeadOverlay)
                uiDeadOverlay.gameObject.SetActive(false);
        }
    }
}
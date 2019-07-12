using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerMovement))]
public class PlayerMechanics : MonoBehaviour
{
    private GameObject spawnPoints;

    public float spawnTime = 10f;
    public float timeSpentDead = 0f;

    public bool isAlive = true;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private FirstPersonCamera playerCamera;
    private GameObject[] spawnPointList;

    public GameObject uiDeadOverlayPrefab;

    private GameManager gameManager;
    private ScoreManager scoreManager;
    private GameObject uiDeadOverlay;

    public int points;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCamera = GetComponentInChildren<FirstPersonCamera>();

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        scoreManager = GameObject.Find("PointsPanel").GetComponent<ScoreManager>();

        if (!spawnPoints)
            spawnPoints = GameObject.Find("Spawnpoints");

        spawnPointList = new GameObject[spawnPoints.transform.childCount];

        for (int i = 0; i < spawnPoints.transform.childCount; i++)
        {
            spawnPointList[i] = spawnPoints.transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            timeSpentDead += Time.deltaTime;
        }

        if (spawnTime < timeSpentDead)
        {
            timeSpentDead = 0;
            RespawnPlayer();
            isAlive = true;
        }

        scoreManager.PlayerJoin();
    }

    public void Kill()
    {
        isAlive = false;

        characterController.enabled = false;
        playerMovement.enabled = false;
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("MainCamera"))
            {
                foreach (Transform cameraChild in child.transform)
                {
                    cameraChild.gameObject.SetActive(false);
                }
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        if (uiDeadOverlayPrefab && !uiDeadOverlay)
            uiDeadOverlay = Instantiate(uiDeadOverlayPrefab, GameObject.FindWithTag("Canvas").transform);
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
            foreach (Transform cameraChild in child.transform)
            {
                cameraChild.gameObject.SetActive(true);
            }

            child.gameObject.SetActive(true);
        }

        characterController.enabled = true;

        if (uiDeadOverlay)
            Destroy(uiDeadOverlay);
    }
}
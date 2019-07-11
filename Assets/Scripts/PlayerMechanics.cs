using UnityEngine;


public class PlayerMechanics : MonoBehaviour
{
    private GameObject spawnPoints;

    public float spawnTime = 10f;
    public float timeSpentDead = 0f;

    public bool isAlive = true;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private GameObject[] spawnPointList;
    
    private GameManager gameManager;
    private ScoreManager scoreManager;

    public int points;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        scoreManager = GameObject.Find("PointsPanel").GetComponent<ScoreManager>();
        
        if (!spawnPoints)
            spawnPoints = GameObject.Find("Spawnpoints");

        spawnPointList = new GameObject[spawnPoints.transform.childCount];

        for(int i = 0; i<spawnPoints.transform.childCount; i++)
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if(hit.transform.gameObject.layer == 9 && isAlive)
        {
            characterController.enabled = false;
            playerMovement.enabled = false;
            foreach (Transform child in transform)
            {
                if(child.gameObject.CompareTag("MainCamera"))
                {
                    foreach(Transform cameraChild in child.transform)
                    {
                        cameraChild.gameObject.SetActive(false);
                    }
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
            isAlive = false;
            Debug.Log("Death!");
        }
    }

    public void RespawnPlayer()
    {
        playerMovement.enabled = true;
        
        var spawnPoint = spawnPointList[Random.Range(0, spawnPointList.Length)];
        playerMovement.Reset(spawnPoint);

        foreach (Transform child in transform)
        {
            foreach (Transform cameraChild in child.transform)
            {
                cameraChild.gameObject.SetActive(true);
            }
            child.gameObject.SetActive(true);
        }
        characterController.enabled = true;
    }
}

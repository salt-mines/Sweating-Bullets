using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMechanics : MonoBehaviour
{
    public GameObject spawnPoints;

    public float spawnTime = 10f;
    public float timeSpentDead = 0f;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private GameObject[] spawnPointList;

    private bool isAlive = true;

    private int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

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
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if(hit.transform.gameObject.layer == 9 && isAlive)
        {
            playerMovement.enabled = false;
            characterController.enabled = false;
            foreach (Transform child in transform)
            {
                if(child.gameObject.tag == "MainCamera")
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
        gameObject.transform.position = spawnPointList[Random.Range(0, spawnPointList.Length - 1)].transform.position;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        characterController.enabled = true;
        playerMovement.enabled = true;
    }
}

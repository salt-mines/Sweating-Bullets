using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMechanics : MonoBehaviour
{
    public GameObject spawnPoint;
    public GameObject playerPrefab;

    private CharacterController characterController;
    private PlayerMovement playerMovement;

    public float spawnTime = 10f;
    public float timeSpentDead = 0f; 
    private bool isAlive = true;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
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
        gameObject.transform.position = spawnPoint.transform.position;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        characterController.enabled = true;
        playerMovement.enabled = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] playerList;
    public GameObject pausePanel;

    private PlayerInput playerInput;
    public bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.Cancel && !paused)
        {
            OnPause();
            paused = true;
        }else if(paused && playerInput.Cancel)
        {
            OnResume();
            paused = false;
        }
    }

    public void OnPause()
    {
        pausePanel.SetActive(true);
        //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = false;
        playerInput.MouseLocked = false;
    }

    public void OnResume()
    {
        pausePanel.SetActive(false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = true;
        playerInput.MouseLocked = true;
        paused = false;
    }

    public void OnMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

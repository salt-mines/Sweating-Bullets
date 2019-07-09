using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] playerList;

    private PlayerInput playerInput;
    private bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.Escape && !paused)
        {
            OnPause();
            paused = true;
        }
    }

    public void OnPause()
    {
        
    }
}

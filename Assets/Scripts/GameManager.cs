using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool paused;
    public GameObject pausePanel;

    private PlayerInput playerInput;
    public GameObject[] playerList;

    // Update is called once per frame
    private void Update()
    {
        if (!playerInput)
            playerInput = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInput>();

        if (!playerInput)
            return;

        if (playerInput.Cancel && !paused)
            OnPause();
        else if (paused && playerInput.Cancel) OnResume();
    }

    public void OnPause()
    {
        pausePanel.SetActive(true);
        playerInput.MouseLocked = false;
        paused = true;
    }

    public void OnResume()
    {
        pausePanel.SetActive(false);
        playerInput.MouseLocked = true;
        paused = false;
    }

    public void OnMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
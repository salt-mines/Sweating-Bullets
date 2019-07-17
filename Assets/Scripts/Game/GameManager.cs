using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public bool paused;
        public GameObject pausePanel;

        private GameInput gameInput;

        private void Start()
        {
            gameInput = FindObjectOfType<GameInput>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (gameInput.Cancel && !paused)
                OnPause();
            else if (paused && gameInput.Cancel) OnResume();
        }

        public void OnPause()
        {
            pausePanel.SetActive(true);
            GameInput.MouseLocked = false;
            paused = true;
            gameInput.BlockInput = true;
        }

        public void OnResume()
        {
            pausePanel.SetActive(false);
            GameInput.MouseLocked = true;
            paused = false;
            gameInput.BlockInput = false;
        }

        public void OnMainMenu()
        {
            FindObjectOfType<Loader>().LoadMainMenu();
        }
    }
}
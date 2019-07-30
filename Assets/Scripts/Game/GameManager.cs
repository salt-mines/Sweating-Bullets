using Networking.Packets;
using UI;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public bool paused;
        public GameObject pausePanel;
        public DeadOverlay deadOverlay;

        public GameMode currentGameMode;

        private GameInput gameInput;

        private void Start()
        {
            gameInput = FindObjectOfType<GameInput>();

            GetComponent<NetworkManager>().Client.ServerInfoReceived += OnServerInfo;
        }

        // Update is called once per frame
        private void Update()
        {
            if (gameInput.Cancel && !paused)
                OnPause();
            else if (paused && gameInput.Cancel) OnResume();
        }

        private void OnServerInfo(object sender, Connected packet)
        {
            currentGameMode = FindObjectOfType<Loader>().availableGameModes[packet.modeId];
            Debug.Log($"Loading mode {currentGameMode.modeName}");
        }

        private void OnPause()
        {
            pausePanel.SetActive(true);
            GameInput.MouseLocked = false;
            paused = true;
            gameInput.BlockInput = true;
        }

        private void OnResume()
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
using Networking;
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
        public GameOverOverlay gameOverOverlay;

        public GameMode currentGameMode;

        private GameInput gameInput;
        private Client client;

        private void Start()
        {
            gameInput = FindObjectOfType<GameInput>();

            client = GetComponent<NetworkManager>().Client;

            client.ServerInfoReceived += OnServerInfo;
            client.GameOver += OnGameEnd;
            client.LevelChanging += OnLevelChange;
        }

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

        private void OnGameEnd(object sender, GameOver packet)
        {
            gameOverOverlay.changeTime = packet.mapChangeTime;
            gameOverOverlay.SetWinner(client.Players[packet.winnerId]?.Name ?? "???");
            gameOverOverlay.gameObject.SetActive(true);

            deadOverlay.block = true;

            if (deadOverlay.gameObject.activeSelf)
            {
                deadOverlay.gameObject.SetActive(false);
            }
        }

        private void OnLevelChange(object sender, string e)
        {
            gameOverOverlay.gameObject.SetActive(false);

            deadOverlay.block = false;
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
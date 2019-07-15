using Game;
using Networking;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenu;
        public ServerSelection serverMenu;

        private Loader loader;
        private NetworkManager networkManager;

        private void Start()
        {
            loader = FindObjectOfType<Loader>();
            networkManager = FindObjectOfType<NetworkManager>();
            networkManager.StartNet(loader, NetworkManager.NetworkMode.MenuClient);
            
            serverMenu.MenuClient = (MenuClient) networkManager.Client;
            serverMenu.onJoin.AddListener(StartClient);

            GameInput.MouseLocked = false;
        }

        public void OnJoin()
        {
            serverMenu.gameObject.SetActive(true);
        }

        private void StartClient(ServerInfo info)
        {
            loader.ServerAddress = info.IP;
            loader.NetworkMode = NetworkManager.NetworkMode.Client;
            loader.StartGame();
        }

        public void OnHost()
        {
            loader.NetworkMode = NetworkManager.NetworkMode.ListenServer;
            loader.StartGame(loader.LevelManager.AvailableLevels[0]);
        }

        public void OnOptions()
        {
            optionsMenu.gameObject.SetActive(true);
        }

        public void OnQuit()
        {
            Application.Quit();
        }
    }
}
using System;
using Game;
using Networking;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenu;
        public ServerSelection serverMenu;
        public HostGameDialog hostGameDialog;

        private Loader loader;
        private NetworkManager networkManager;

        private void Awake()
        {
            loader = FindObjectOfType<Loader>();
            networkManager = FindObjectOfType<NetworkManager>();

            hostGameDialog.LevelManager = loader.LevelManager;
        }

        private void Start()
        {
            networkManager.StartNet(loader, NetworkManager.NetworkMode.MenuClient);

            serverMenu.MenuClient = (MenuClient) networkManager.Client;

            hostGameDialog.onHostGame.AddListener(StartHost);
            serverMenu.onJoin.AddListener(StartClient);

            GameInput.MouseLocked = false;
        }

        private void StartClient(ServerInfo info)
        {
            loader.ServerAddress = info.IP;
            loader.NetworkMode = NetworkManager.NetworkMode.Client;
            loader.StartGame();
        }

        private void StartHost(ServerConfig config)
        {
            loader.NetworkMode = NetworkManager.NetworkMode.ListenServer;
            loader.StartGame(config);
        }

        public void OnClickJoin()
        {
            serverMenu.gameObject.SetActive(true);
        }

        public void OnClickHost()
        {
            hostGameDialog.gameObject.SetActive(true);
        }

        public void OnClickOptions()
        {
            optionsMenu.gameObject.SetActive(true);
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }
    }
}
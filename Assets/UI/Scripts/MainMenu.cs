using System.Net;
using Networking;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenuPrefab;
        public ServerSelection serverMenuPrefab;

        private Loader loader;
        private NetworkManager networkManager;

        private IPEndPoint server;

        private void Start()
        {
            loader = FindObjectOfType<Loader>();
            networkManager = FindObjectOfType<NetworkManager>();
        }

        public void OnJoin()
        {
            if (!serverMenuPrefab) return;

            var menu = Instantiate(serverMenuPrefab, transform.parent);
            menu.MenuClient = (MenuClient) networkManager.Client;
            menu.onJoin.AddListener(StartClient);
        }

        private void StartClient(string host)
        {
            server = new IPEndPoint(string.IsNullOrEmpty(host) ? IPAddress.Loopback : IPAddress.Parse(host),
                Constants.AppPort);

            loader.ServerAddress = server;
            loader.NetworkMode = NetworkManager.NetworkMode.Client;
            loader.ChangeLevel("Test");
        }

        public void OnHost()
        {
            loader.NetworkMode = NetworkManager.NetworkMode.ListenServer;
            loader.ChangeLevel("Test");
        }

        public void OnOptions()
        {
            if (optionsMenuPrefab) Instantiate(optionsMenuPrefab, transform.parent);
        }

        public void OnQuit()
        {
            Application.Quit();
        }
    }
}
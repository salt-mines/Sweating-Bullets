using System;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenuPrefab;
        public ServerSelection serverMenuPrefab;

        private Loader loader;
        private NetworkManager networkManager;
        
        private NetworkManager.NetworkMode mode;
        private string host;

        private void Start()
        {
            loader = FindObjectOfType<Loader>();
            networkManager = FindObjectOfType<NetworkManager>();
            
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public void OnJoin()
        {
            if (!serverMenuPrefab) return;

            var menu = Instantiate(serverMenuPrefab, transform.parent);
            menu.MenuClient = (MenuClient)networkManager.Client;
            menu.onJoin.AddListener(StartClient);
        }

        private void StartClient(string host)
        {
            this.host = host;
            if (string.IsNullOrEmpty(this.host))
            {
                this.host = "127.0.0.1";
            }

            mode = NetworkManager.NetworkMode.Client;
            loader.ChangeLevel("Test");
        }

        public void OnHost()
        {
            mode = NetworkManager.NetworkMode.ListenServer;
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

        private void SceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            var nm = FindObjectOfType<NetworkManager>();
            if (nm == null) return;

            nm.Mode = mode;

            if (nm.Mode == NetworkManager.NetworkMode.Client)
            {
                nm.Connect(host);
            }
        }
    }
}
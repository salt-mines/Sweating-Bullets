using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenuPrefab;
        public ServerSelection serverMenuPrefab;

        private Loader loader;
        private NetworkManager.NetworkMode mode;
        private string host;

        private void Start()
        {
            loader = FindObjectOfType<Loader>();
            
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public void OnJoin()
        {
            if (!serverMenuPrefab) return;

            var menu = Instantiate(serverMenuPrefab, transform.parent);
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

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var nmo = GameObject.Find("NetworkManager");
            if (nmo == null) return;

            var nm = nmo.GetComponent<NetworkManager>();
            if (nm == null) return;

            nm.Mode = this.mode;

            if (nm.Mode == NetworkManager.NetworkMode.Client)
            {
                nm.Connect(host);
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenuPrefab;

        private NetworkManager.NetworkMode mode;

        private void Start()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public void OnJoin()
        {
            mode = NetworkManager.NetworkMode.Client;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void OnHost()
        {
            mode = NetworkManager.NetworkMode.ListenServer;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
        }
    }
}
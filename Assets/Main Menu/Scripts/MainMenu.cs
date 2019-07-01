using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public OptionsMenu optionsMenuPrefab;

        public void OnStart()
        {
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
    }
}

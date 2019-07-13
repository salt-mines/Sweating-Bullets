using System.Collections;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public Canvas loadingScreenCanvas;
    public SceneReference mainMenuScene;
    public ProgressBar progressBar;

    private void Start()
    {
        if (SceneManager.sceneCount == 1)
            // If this is the only loaded scene, load main menu
            LoadScene(mainMenuScene.ScenePath);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadScene(string name)
    {
        if (loadingScreenCanvas)
            loadingScreenCanvas.gameObject.SetActive(true);
        
        StartCoroutine(LoadSceneAsync(name));
    }

    private IEnumerator LoadSceneAsync(string name)
    {
        var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar)
                progressBar.Progress = op.progress;

            yield return null;
        }

        op.allowSceneActivation = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadingScreenCanvas)
            loadingScreenCanvas.gameObject.SetActive(false);
        
        SceneManager.SetActiveScene(scene);
    }
}
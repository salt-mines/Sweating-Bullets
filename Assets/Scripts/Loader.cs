using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public Canvas loadingScreenCanvas;
    public ProgressBar progressBar;

    public SceneReference mainMenuScene;

    [Tooltip("Scene containing common gameplay objects.")]
    public SceneReference gameScene;

    private string currentLevel;
    private bool isCommonLoaded;

    private void Start()
    {
        if (SceneManager.sceneCount == 1)
            // If this is the only loaded scene, load main menu
            ChangeLevel(mainMenuScene, false);
        else
        {
            currentLevel = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadMainMenu()
    {
        ChangeLevel(mainMenuScene, false);
    }

    public void ChangeLevel(string name, bool loadCommon = true)
    {
        if (loadingScreenCanvas)
            loadingScreenCanvas.gameObject.SetActive(true);

        if (currentLevel != null) UnloadCurrentLevel();
        if (!isCommonLoaded && loadCommon)
        {
            LoadScene(gameScene, false);
            isCommonLoaded = true;
        }

        currentLevel = name;
        LoadScene(currentLevel);
    }

    private void LoadScene(string name, bool loadAsync = true)
    {
        if (loadingScreenCanvas)
            loadingScreenCanvas.gameObject.SetActive(true);

        if (loadAsync)
            StartCoroutine(LoadSceneAsync(name));
        else
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    private void UnloadCurrentLevel()
    {
        StartCoroutine(UnloadSceneAsync(currentLevel));
        if (isCommonLoaded)
            StartCoroutine(UnloadSceneAsync(gameScene));
        currentLevel = null;
        isCommonLoaded = false;
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

    private IEnumerator UnloadSceneAsync(string name)
    {
        var op = SceneManager.UnloadSceneAsync(name);

        while (!op.isDone) yield return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadingScreenCanvas)
            loadingScreenCanvas.gameObject.SetActive(false);

        SceneManager.SetActiveScene(scene);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [SerializeField]
    private LoadingScreen loadingScreen;

    public SceneReference mainMenuScene;

    [Tooltip("Scene containing common gameplay objects.")]
    public SceneReference gameScene;

    [SerializeField]
    private List<SceneReference> availableLevels = new List<SceneReference>();

    private bool awoken;
    private bool isCommonLoaded;
    private Scene preloadedScene;

    public LevelManager LevelManager { get; private set; }

    public IPEndPoint ServerAddress { get; set; }
    public NetworkManager.NetworkMode NetworkMode { get; set; } = NetworkManager.NetworkMode.ListenServer;

    public event EventHandler<string> LevelLoaded;

    private void Awake()
    {
        if (awoken) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        awoken = true;

        LevelManager = new LevelManager(availableLevels);
        LevelManager.LevelChanging += LevelChanging;

        preloadedScene = SceneManager.GetActiveScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // If this is the only loaded scene, load main menu
        if (SceneManager.sceneCount == 1) StartCoroutine(LoadSceneAsync(mainMenuScene));

        // At the start, the active scene gets loaded possibly before our Boot scene,
        // so call OnSceneLoaded manually to ensure that stuff gets initialized properly.
        OnSceneLoaded(preloadedScene, LoadSceneMode.Additive);
    }

    private void LevelChanging(object sender, string newLevel)
    {
        StartCoroutine(UnloadAndLoadAsync(LevelManager.CurrentLevel, newLevel));
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(gameObject.scene.buildIndex);
    }

    /// <summary>
    ///     Load common game scene, optionally with the given starting level.
    /// </summary>
    /// <param name="startingLevel">optional starting level</param>
    public void StartGame(string startingLevel = null)
    {
        if (isCommonLoaded) return;

        if (startingLevel != null)
            LevelManager.StartingLevel = startingLevel;

        StartCoroutine(UnloadAndLoadAsync(mainMenuScene, gameScene));
        isCommonLoaded = true;
    }

    private IEnumerator UnloadAndLoadAsync(string unload, string load)
    {
        yield return UnloadSceneAsync(unload);
        yield return LoadSceneAsync(load);
    }

    private IEnumerator LoadSceneAsync(string scene)
    {
        loadingScreen.Show(true, true);

        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            loadingScreen.Progress = op.progress;

            yield return null;
        }

        op.allowSceneActivation = true;
    }

    private IEnumerator UnloadSceneAsync(string scene)
    {
        AsyncOperation op;
        try
        {
            op = SceneManager.UnloadSceneAsync(scene);
        }
        catch (ArgumentException)
        {
            yield break;
        }

        while (!op.isDone) yield return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == gameObject.scene) return;
        if (!scene.IsValid()) return;

        var isCommon = scene.path == gameScene.ScenePath;
        var isMainMenu = scene.path == mainMenuScene.ScenePath;

        if (!isCommon)
            loadingScreen.Show(false, true);

        if (isCommon)
        {
            var nm = FindObjectOfType<NetworkManager>();

            nm.Level = LevelManager.StartingLevel;
            nm.StartNet(this, NetworkMode, ServerAddress);

            if (nm.Client is NetworkClient nc) nc.StatusChanged += OnNetworkStatus;

            return;
        }

        if (isMainMenu) return;

        // Set the actual level as active
        SceneManager.SetActiveScene(scene);

        LevelLoaded?.Invoke(this, scene.name);
    }

    private void OnNetworkStatus(object sender, NetworkClient.StatusChangeEvent statusChangeEvent)
    {
        Debug.Log(statusChangeEvent.Status);

        loadingScreen.ConnectingStatus = statusChangeEvent.Status.ToString();

        if (statusChangeEvent.Status == NetConnectionStatus.Disconnected)
            LoadMainMenu();
    }
}
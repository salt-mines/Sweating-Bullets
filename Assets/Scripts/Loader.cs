using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using NaughtyAttributes;
using Networking;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [Tooltip("Loading screen prefab.")]
    public LoadingScreen loadingScreen;

    [Tooltip("Main menu scene, loaded first if other scenes aren't open.")]
    public SceneReference mainMenuScene;

    [Tooltip("Scene containing common gameplay objects.")]
    public SceneReference gameScene;

    [Tooltip("List of available levels in the game. Displayed in given order.")]
    [ReorderableList]
    public List<SceneReference> availableLevels = new List<SceneReference>();

    private bool isCommonLoaded;
    private Scene preloadedScene;
    private ServerConfig serverConfig;

    public Preferences Preferences { get; } = new Preferences();
    public LevelManager LevelManager { get; private set; }

    public IPEndPoint ServerAddress { get; set; }
    public NetworkManager.NetworkMode NetworkMode { get; set; } = NetworkManager.NetworkMode.ListenServer;

    public event EventHandler<string> LevelLoaded;

    private void Awake()
    {
        Preferences.Load();

        GetComponent<PreferencesSetter>().Preferences = Preferences;

        SceneManager.sceneLoaded += OnSceneLoaded;

        LevelManager = new LevelManager(availableLevels);
        LevelManager.LevelChanging += LevelChanging;

        preloadedScene = SceneManager.GetActiveScene();

        // Check open scenes for any levels, so we can correctly load them.
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var sc = SceneManager.GetSceneAt(i);

            if (sc == gameObject.scene ||
                sc.path == gameScene.ScenePath ||
                sc.path == mainMenuScene.ScenePath)
                continue;

            LevelManager.StartingLevel = sc.name;
            LevelManager.StartingLevelLoaded = true;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // If started in batch mode, load straight into game
        if (Application.isBatchMode)
        {
            if (Utils.GetArgument("-listlevels") != null)
            {
                Debug.Log("Available levels:\n");
                foreach (var lvl in LevelManager.AvailableLevels)
                    Debug.Log($"Level name: {lvl}");

                Application.Quit();
            }

            var maxPl = Constants.MaxPlayers;
            var maxPlStr = Utils.GetArgument("-maxplayers");
            if (maxPlStr != null)
                byte.TryParse(maxPlStr, out maxPl);

            var clLevel = Utils.GetArgument("-level") ?? LevelManager.StartingLevel;

            if (!LevelManager.IsValidLevel(clLevel))
                throw new ArgumentException("level is invalid");

            StartGame(new ServerConfig
            {
                MaxPlayerCount = maxPl,
                StartingLevel = clLevel
            });
            return;
        }

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

    /// <summary>
    ///     Load main menu by unloading everything and starting from boot.
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(gameObject.scene.buildIndex);
    }

    /// <summary>
    ///     Load common game scene, optionally with the given starting level.
    /// </summary>
    /// <param name="config">server configuration</param>
    public void StartGame(ServerConfig config = null)
    {
        if (isCommonLoaded) return;

        serverConfig = config;

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

        if (op == null) yield break;

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

        // Start network things when game scene is loaded.
        if (isCommon)
        {
            var nm = FindObjectOfType<NetworkManager>();

            nm.ServerConfig = serverConfig;
            nm.StartNet(this, NetworkMode, ServerAddress);

            if (nm.Client is NetworkClient nc) nc.StatusChanged += OnNetworkStatus;

            return;
        }

        // Set last loaded level as active scene.
        SceneManager.SetActiveScene(scene);

        if (isMainMenu)
        {
            FindObjectOfType<MainMenu>().Preferences = Preferences;
            return;
        }

        LevelLoaded?.Invoke(this, scene.name);
    }

    private void OnNetworkStatus(object sender, NetworkClient.StatusChangeEvent statusChangeEvent)
    {
        Debug.Log($"Connection status changed: {statusChangeEvent.Status}");

        loadingScreen.ConnectingStatus = statusChangeEvent.Status.ToString();

        if (statusChangeEvent.Status == NetConnectionStatus.Disconnected)
            LoadMainMenu();
    }
}
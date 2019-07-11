using JetBrains.Annotations;
using Networking;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public enum NetworkMode
    {
        Server,
        ListenServer,
        Client
    }

    [CanBeNull] private Client client;
    [CanBeNull] private Server server;

    [CanBeNull] private string startupHost;
    private int startupPort;

    public NetworkMode Mode { get; set; } = NetworkMode.ListenServer;

    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected");
            Mode = NetworkMode.Server;
        }

        Debug.Log("Starting in mode: " + Mode);

        if (Mode == NetworkMode.Server || Mode == NetworkMode.ListenServer)
        {
            server = new Server(Constants.MaxPlayers) {NetworkManager = this};
            server.SimulatedLag = simulatedLag;
        }

        if (Mode == NetworkMode.ListenServer) client = new HostClient(server) {NetworkManager = this};

        if (Mode == NetworkMode.Client) client = new NetworkClient {NetworkManager = this};

        if (client != null)
        {
            client.InterpolationEnabled = interpolationEnabled;
            client.Interpolation = interpolation;
        }

        if (startupHost != null)
            Connect(startupHost, startupPort);
    }

    private void OnValidate()
    {
        if (server != null) server.SimulatedLag = simulatedLag;

        if (client != null)
        {
            client.InterpolationEnabled = interpolationEnabled;
            client.Interpolation = interpolation;
        }
    }

    public NetworkPlayer CreatePlayer(PlayerInfo info, bool local = false)
    {
        var ply = Instantiate(local ? localPlayerPrefab : networkPlayerPrefab);
        ply.PlayerInfo = info;
        ply.IsLocalPlayer = local;

        return ply;
    }

    public void RemovePlayer(NetworkPlayer player)
    {
        Destroy(player.gameObject);
    }

    public void Connect(string host, int port = Constants.AppPort)
    {
        if (client == null)
        {
            startupHost = host;
            startupPort = port;
            return;
        }

        client.Connect(host, port);
    }

    private void Update()
    {
        client?.Update();
        server?.Update();
    }

    private void LateUpdate()
    {
        server?.LateUpdate();
    }

    private void OnDestroy()
    {
        client?.Shutdown();
        server?.Shutdown();
    }

    private void OnGUI()
    {
        client?.OnGUI(5, 20);
    }

    private void OnDrawGizmos()
    {
        client?.OnDrawGizmos();
    }

    #region Unity fields

    [Header("Shared")] public NetworkPlayer networkPlayerPrefab;

    [Header("Server")]
    [Range(0, 1)]
    [Tooltip("Simulated latency in seconds")]
    public float simulatedLag;

    [Header("Client")] public NetworkPlayer localPlayerPrefab;

    public bool interpolationEnabled = true;

    [Range(0, 1)]
    [Tooltip("Interpolation time in seconds")]
    public float interpolation = 0.1f;

    #endregion
}
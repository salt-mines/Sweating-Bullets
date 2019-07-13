using JetBrains.Annotations;
using Networking;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public enum NetworkMode
    {
        MenuClient,
        Client,
        Server,
        ListenServer
    }

    #region Unity fields

    [Header("Shared")] public NetworkPlayer networkPlayerPrefab;

    public NetworkMode mode = NetworkMode.ListenServer;

    [Range(1, 240)]
    [Tooltip("How many updates per second to process")]
    public int tickRate = Constants.TickRate;

    [Range(1, 240)]
    [Tooltip("How many updates per second to send")]
    public int sendRate = Constants.SendRate;

    [Header("Server")]
    [Range(0, 1)]
    [Tooltip("Simulated latency in seconds")]
    public float simulatedLag;

    [Header("Client")] public NetworkPlayer localPlayerPrefab;

    public bool interpolationEnabled = true;

    [Range(0, 1)]
    [Tooltip("Interpolation time in seconds")]
    public float interpolation = Constants.Interpolation;

    #endregion

    #region Class variables

    [CanBeNull] private Client client;
    [CanBeNull] private Server server;

    [CanBeNull] private string startupHost;
    private int startupPort;

    public NetworkMode Mode
    {
        get => mode;
        set => mode = value;
    }

    #endregion

    #region Unity events

    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected");
            Mode = NetworkMode.Server;
        }

        Debug.Log("Starting in mode: " + Mode);

        if (Mode == NetworkMode.Server || Mode == NetworkMode.ListenServer)
            server = new Server(Constants.MaxPlayers)
            {
                NetworkManager = this,
                TickRate = tickRate,
                SendRate = sendRate,
                SimulatedLag = simulatedLag
            };

        if (Mode == NetworkMode.ListenServer) client = new HostClient(server);

        if (Mode == NetworkMode.Client) client = new NetworkClient();
        
        if (Mode == NetworkMode.MenuClient) client = new MenuClient();

        if (client != null)
        {
            client.NetworkManager = this;
            client.TickRate = tickRate;
            client.SendRate = sendRate;
            client.InterpolationEnabled = interpolationEnabled;
            client.Interpolation = interpolation;
        }

        if (startupHost != null)
            Connect(startupHost, startupPort);

        if (Mode == NetworkMode.MenuClient)
        {
            var mc = (MenuClient) client;
            mc.ServerDiscovered += OnDiscovery;
            mc.DiscoverLocalServers();
        }
    }

    private void OnDiscovery(object sender, ServerInfo info)
    {
        Debug.LogFormat("LAN server found: {0}", info);
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

    #endregion

    #region Network methods

    public NetworkPlayer CreatePlayer(PlayerInfo info, bool local = false)
    {
        var ply = Instantiate(local ? localPlayerPrefab : networkPlayerPrefab);
        ply.PlayerInfo = info;
        ply.IsLocalPlayer = local;
        ply.NetworkClient = client;

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

    #endregion
}
using System.Net;
using Networking;
using UnityEngine;
using UnityEngine.Serialization;
using NetworkPlayer = Game.NetworkPlayer;

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

    [Header("Shared")]
    public NetworkPlayer networkPlayerPrefab;

    [FormerlySerializedAs("mode")]
    public NetworkMode networkMode = NetworkMode.ListenServer;

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

    [Header("Client")]
    public NetworkPlayer localPlayerPrefab;

    public bool interpolationEnabled = true;

    [Range(0, 1)]
    [Tooltip("Interpolation time in seconds")]
    public float interpolation = Constants.Interpolation;

    #endregion

    #region Class variables

    internal Loader Loader { get; set; }
    public Client Client { get; private set; }
    public Server Server { get; private set; }

    public ServerConfig ServerConfig { get; set; }

    public NetworkMode Mode
    {
        get => networkMode;
        set => networkMode = value;
    }

    #endregion

    #region Unity events

    public void StartNet(Loader loader, NetworkMode netMode, IPEndPoint host = null)
    {
        if (Client != null || Server != null) return;

        Mode = netMode;

        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected");
            Mode = NetworkMode.Server;
        }

        Loader = loader;

        Debug.Log("Starting in mode: " + Mode);

        if (Mode == NetworkMode.Server || Mode == NetworkMode.ListenServer)
            Server = new Server(ServerConfig, Loader)
            {
                NetworkManager = this,
                TickRate = tickRate,
                SendRate = sendRate,
                SimulatedLag = simulatedLag
            };

        if (Mode == NetworkMode.ListenServer) Client = new HostClient(Server);

        if (Mode == NetworkMode.Client) Client = new NetworkClient(Loader);

        if (Mode == NetworkMode.MenuClient) Client = new MenuClient();

        if (Client != null)
        {
            Client.NetworkManager = this;
            Client.TickRate = tickRate;
            Client.SendRate = sendRate;
            Client.InterpolationEnabled = interpolationEnabled;
            Client.Interpolation = interpolation;
        }

        if (Mode == NetworkMode.Client && host != null)
            Client?.Connect(host.Address.ToString(), host.Port);
    }

    private void OnValidate()
    {
        if (Server != null) Server.SimulatedLag = simulatedLag;

        if (Client != null)
        {
            Client.InterpolationEnabled = interpolationEnabled;
            Client.Interpolation = interpolation;
        }
    }

    private void Update()
    {
        Client?.Update();
        Server?.Update();
    }

    private void LateUpdate()
    {
        Client?.LateUpdate();
        Server?.LateUpdate();
    }

    private void OnDestroy()
    {
        Client?.Shutdown();
        Server?.Shutdown();
    }

    private void OnGUI()
    {
        Client?.OnGUI(5, Screen.height - 100);
    }

    private void OnDrawGizmos()
    {
        Client?.OnDrawGizmos();
    }

    #endregion

    #region Network methods

    public NetworkPlayer CreatePlayer(PlayerInfo info, bool local = false)
    {
        var ply = Instantiate(local ? localPlayerPrefab : networkPlayerPrefab);
        ply.PlayerInfo = info;
        ply.IsLocalPlayer = local;
        ply.Client = Client;

        return ply;
    }

    public void RemovePlayer(NetworkPlayer player)
    {
        Destroy(player.gameObject);
    }

    #endregion
}
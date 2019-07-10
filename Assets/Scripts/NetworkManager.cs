using System;
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

    private Client client;

    public GameObject localPlayerPrefab;
    public NetworkActor networkPlayerPrefab;
    private Server server;

    public NetworkMode Mode { get; set; } = NetworkMode.ListenServer;

    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected");
            Mode = NetworkMode.Server;
        }

        Debug.Log("Starting in mode: " + Mode);

        switch (Mode)
        {
            case NetworkMode.Server:
                server = new Server(Constants.MaxPlayers);
                break;
            case NetworkMode.ListenServer:
                server = new Server(Constants.MaxPlayers);
                client = new HostClient(server);
                break;
            case NetworkMode.Client:
                client = new NetworkClient();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Connect(string host, int port = Constants.AppPort)
    {
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
    }
}
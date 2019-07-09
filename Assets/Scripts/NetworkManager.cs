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
    private Server server;

    public GameObject localPlayerPrefab;
    public NetworkActor networkPlayerPrefab;
    
    private string instantConnectHost = null;

    public NetworkMode Mode { get; set; }

    private void Start()
    {
        Debug.Log("Starting in mode: " + Mode);

        switch (Mode)
        {
            case NetworkMode.Server:
            case NetworkMode.ListenServer:
            case NetworkMode.Client:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        server = new Server(Constants.MaxPlayers);
        client = new HostClient(server);
    }

    public void Connect(string host, int port = Constants.AppPort)
    {
    }

    private void Update()
    {
        client.Update();
    }

    private void FixedUpdate()
    {
        if (server != null)
            server.FixedUpdate();
    }

    private void OnDestroy()
    {
    }
}
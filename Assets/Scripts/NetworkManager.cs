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

    private Peer peer;

    public NetworkMode Mode { get; set; }

    private void Start()
    {
        Mode = NetworkMode.ListenServer;
        
        switch (Mode)
        {
            case NetworkMode.ListenServer:
                peer = new ListenServer
                {
                    LocalPlayerPrefab = localPlayerPrefab,
                    NetworkPlayerPrefab = networkPlayerPrefab
                };
                ((ListenServer) peer).CreateLocalPlayer();
                break;
            case NetworkMode.Client:
                peer = new Client();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        peer.Start();

        client = new Client
        {
            LocalActor = ((ListenServer) peer).LocalActor,
            NetworkPlayerPrefab = networkPlayerPrefab
        };
        client.Start();
        client.Connect("127.0.0.1", Peer.APP_PORT);
    }

    public void Connect(string host, int port)
    {
        if (peer is Client client) client.Connect(host, port);
    }

    private void Update()
    {
        peer.ReadMessages();
        client.ReadMessages();

        peer.Update();
        client.Update();
    }

    private void FixedUpdate()
    {
        peer.FixedUpdate();
        client.FixedUpdate();
    }
}
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
        Debug.Log("Starting in mode: " + Mode);

        switch (Mode)
        {
            case NetworkMode.Server:
            case NetworkMode.ListenServer:
                peer = new ListenServer
                {
                    LocalPlayerPrefab = localPlayerPrefab,
                    NetworkPlayerPrefab = networkPlayerPrefab
                };
                ((ListenServer) peer).CreateLocalPlayer();
                break;
            case NetworkMode.Client:
                peer = new Client
                {
                    LocalPlayerPrefab = localPlayerPrefab,
                    NetworkPlayerPrefab = networkPlayerPrefab
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        peer.Start();
        Connect("127.0.0.1", Peer.AppPort);
    }

    public void Connect(string host, int port)
    {
        if (peer is Client client) client.Connect(host, port);
    }

    private void Update()
    {
        if (!peer.Running) return;
        peer.Update();
    }

    private void FixedUpdate()
    {
        if (!peer.Running) return;

        peer.ReadMessages();

        peer.FixedUpdate();
    }
}
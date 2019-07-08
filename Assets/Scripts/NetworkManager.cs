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

    public Peer Peer { get; private set; }
    private string instantConnectHost = null;

    public NetworkMode Mode { get; set; }

    private void Start()
    {
        Debug.Log("Starting in mode: " + Mode);

        switch (Mode)
        {
            case NetworkMode.Server:
            case NetworkMode.ListenServer:
                Peer = new ListenServer
                {
                    LocalPlayerPrefab = localPlayerPrefab,
                    NetworkPlayerPrefab = networkPlayerPrefab
                };
                ((ListenServer)Peer).CreateLocalPlayer();
                break;
            case NetworkMode.Client:
                Peer = new Client
                {
                    LocalPlayerPrefab = localPlayerPrefab,
                    NetworkPlayerPrefab = networkPlayerPrefab
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Peer.Start();
        if (instantConnectHost != null)
            Connect(instantConnectHost);
    }

    public void Connect(string host, int port = Peer.AppPort)
    {
        if (Peer is Client client) client.Connect(host, port);
        if (Peer == null) instantConnectHost = host;
    }

    private void Update()
    {
        if (!Peer.Running) return;
        Peer.Update();
    }

    private void FixedUpdate()
    {
        if (!Peer.Running) return;

        Peer.ReadMessages();

        Peer.FixedUpdate();
    }

    private void OnDestroy()
    {
        Peer.Shutdown("Bye!");
    }
}
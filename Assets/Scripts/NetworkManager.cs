using UnityEngine;
using Networking;

public class NetworkManager : MonoBehaviour
{
    public NetworkActor networkPlayerPrefab;

    public GameObject LocalActor { get; set; }

    private Server server;
    private Client client;
    
    void Start()
    {
        server = new Server();
        server.Start();

        client = new Client
        {
            NetworkPlayerPrefab = networkPlayerPrefab
        };
        client.Start();
        client.Connect("127.0.0.1", Client.APP_PORT);
    }
    
    void Update()
    {
        server.ReadMessages();
        client.ReadMessages();

        client.LocalActor = LocalActor;
        client.Update();

        server.Update();
    }
}

using UnityEngine;
using Networking;

public class NetworkManager : MonoBehaviour
{
    private Server server;
    private Client client;
    
    void Start()
    {
        server = new Server();
        server.Start();

        client = new Client();
        client.Start();
        client.Connect("127.0.0.1", Client.APP_PORT);
    }
    
    void Update()
    {
        server.ReadMessages();
        client.ReadMessages();
    }
}

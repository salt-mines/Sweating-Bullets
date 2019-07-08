using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public Networking.Peer Peer { get; set; }

    private void Start()
    {
        Peer = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().Peer;
    }

    public void Shoot(GameObject target)
    {
        var na = target.GetComponent<NetworkActor>();
        if (!na) return;

        if (Peer is Networking.Client client)
        {
            client.Shoot(na);
        }
    }
}
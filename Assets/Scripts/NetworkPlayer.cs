using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private void Start()
    {
        //Peer = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().Peer;
    }

    public void Shoot(GameObject target)
    {
        var na = target.GetComponent<NetworkActor>();
        if (!na) return;
    }
}
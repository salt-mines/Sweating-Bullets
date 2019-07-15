using Game;
using Networking;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] private bool isLocalPlayer;
    public byte Id => PlayerInfo.Id;

    public PlayerInfo PlayerInfo { get; internal set; }

    public Client NetworkClient { get; set; }

    public bool IsLocalPlayer
    {
        get => isLocalPlayer;
        set => isLocalPlayer = value;
    }

    private void Update()
    {
        var tr = transform;
        if (IsLocalPlayer)
        {
            PlayerInfo.Position = tr.position;
            PlayerInfo.Rotation = tr.rotation;
            PlayerInfo.Alive = GetComponent<PlayerMechanics>().isAlive;
        }
        else
        {
            tr.position = PlayerInfo.Position;
            tr.rotation = PlayerInfo.Rotation;
            tr.GetChild(0).gameObject.SetActive(PlayerInfo.Alive);
            tr.GetChild(1).gameObject.SetActive(PlayerInfo.Alive);
        }
    }

    public void Kill()
    {
        GetComponent<PlayerMechanics>()?.Kill();
    }

    public void Shoot(NetworkPlayer target)
    {
        NetworkClient.PlayerShoot(target.Id);
    }
}
using Game;
using Networking;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");
    private static readonly int ParamForward = Animator.StringToHash("Forward");
    private static readonly int ParamRight = Animator.StringToHash("Right");

    [SerializeField]
    private bool isLocalPlayer;

    private Animator animator;
    private PlayerMovement playerMovement;

    public byte Id => PlayerInfo.Id;

    public PlayerInfo PlayerInfo { get; internal set; }

    public Client NetworkClient { get; set; }

    public bool IsLocalPlayer
    {
        get => isLocalPlayer;
        set => isLocalPlayer = value;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        var tr = transform;
        if (IsLocalPlayer)
        {
            PlayerInfo.Position = tr.position;
            PlayerInfo.Velocity = playerMovement.Velocity;
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

        var groundVel = tr.InverseTransformVector(PlayerInfo.Velocity);
        groundVel.y = 0;
        animator.SetFloat(ParamSpeed, groundVel.magnitude);
        animator.SetFloat(ParamForward, groundVel.z);
        animator.SetFloat(ParamRight, groundVel.x);
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
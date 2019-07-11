using UnityEngine;

[RequireComponent(typeof(NetworkPlayer))]
public class Gun : MonoBehaviour
{
    private Camera fpsCamera;

    private GameManager gameManager;
    public LayerMask hittableMask;

    public NetworkPlayer player;
    public float range = 100f;
    public float rateOfFire = 1f;
    private ScoreManager scoreManager;

    private float timeToFire;

    // Start is called before the first frame update
    private void Start()
    {
        fpsCamera = GetComponentInChildren<Camera>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        scoreManager = GameObject.Find("PointsPanel").GetComponent<ScoreManager>();

        player = GetComponent<NetworkPlayer>();

        timeToFire = rateOfFire;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && rateOfFire <= timeToFire)
        {
            timeToFire = 0;
            Shoot();
        }

        timeToFire += Time.deltaTime;
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range, hittableMask))
        {
            Debug.Log(hit.transform.name);
            Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward * hit.distance, Color.yellow, 2,
                false);

            if (hit.transform.gameObject.layer == 9)
            {
                var targetNetPlayer = hit.transform.gameObject.GetComponentInParent<NetworkPlayer>();
                if (targetNetPlayer)
                    player.Shoot(targetNetPlayer);

                GetComponent<PlayerMechanics>().points++;
                scoreManager.UpdateScoreText();
            }
        }
    }
}
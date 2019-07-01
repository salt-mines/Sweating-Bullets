using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float rateOfFire = 30f;
    public float range = 100f;

    public LayerMask hittableMask;

    private float timeToFire = 0f;
    private Camera fpsCamera;


    // Start is called before the first frame update
    void Start()
    {
        fpsCamera = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= timeToFire)
        {
            timeToFire = Time.time + 1f / rateOfFire;
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range, hittableMask))
        {
            Debug.Log(hit.transform.name);
            Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward * hit.distance, Color.yellow, 2, false);
            if(hit.transform.gameObject.layer == 9)
            {
                // Kills hittable layer gameObject
                Destroy(hit.transform.gameObject);
            }
        }
    }
}

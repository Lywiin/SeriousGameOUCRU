using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float speed;
    [Range(0, 1)]
    public float maxVelocity;

    [Header("Projectile")]
    public GameObject firePoint;
    public GameObject projectile;
    public float fireRate;

    // Private variables
    private Rigidbody rb;
    private float timeToFire = 0f;
    private Plane plane;
    private GameController gameController;
    private bool dead = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        plane = new Plane(Vector3.up, Camera.main.transform.position.y);

        gameController = Camera.main.GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        //If game not paused
        if (!gameController.IsGamePaused())
        {
            //Firing projectile
            if (Input.GetButton("Fire1") && Time.time >= timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                SpawnProjectile();
            }

            //Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;

            if (plane.Raycast(ray, out enter))
            {
                //Get the point that was touched
                Vector3 hitPoint = ray.GetPoint(enter);
                hitPoint.y = 0.0f;

                //Determine new player rotation
                Quaternion rotation = Quaternion.LookRotation(hitPoint - transform.position, Vector3.up);
                transform.rotation = rotation;
            }
        }
    }

    void FixedUpdate()
    {
        if (!gameController.IsGamePaused())
        {
            float moveHor = Input.GetAxis("Horizontal");
            float moveVer = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveHor, 0.0f, moveVer);

            rb.AddForce(movement * speed, ForceMode.Impulse);
        }
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    void SpawnProjectile()
    {
        //Instantiate projectile at player position and rotation
        GameObject p = Instantiate(projectile, firePoint.transform.position + (firePoint.transform.forward), transform.rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Player dies on collision with bacteria
        if (collision.gameObject.tag == "BadBacteria" && !dead)
        {
            dead = true;
            gameController.PlayerDied();
        }
    }
}

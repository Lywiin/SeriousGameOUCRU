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
    public GameObject projectile1;
    public float fireRateP1 = 10f;
    public GameObject projectile2;
    public float fireRateP2 = 3f;

    [Header("Attack Boost")]
    public float damageMultiplier = 1.5f;
    public float boostDuration = 5f;

    // Private variables
    private Rigidbody rb;
    private GameController gameController;

    private float timeToFireP1 = 0f;
    private float timeToFireP2 = 0f;

    private Plane plane;
    private bool dead = false;

    private bool isBoosted = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameController = GameController.Instance;
        
        plane = new Plane(Vector3.up, Camera.main.transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        // If game not paused
        if (!gameController.IsGamePaused())
        {
            // Firing projectile 1 or 2
            if (Input.GetButton("Fire1") && Time.time >= timeToFireP1)
            {
                timeToFireP1 = Time.time + 1 / fireRateP1;
                SpawnProjectile(projectile1);
            }else if (Input.GetButton("Fire2") && Time.time >= timeToFireP2)
            {
                timeToFireP2 = Time.time + 1 / fireRateP2;
                SpawnProjectile(projectile2);
                gameController.IncreaseAllMutationProba();
            }

            // Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;

            if (plane.Raycast(ray, out enter))
            {
                // Get the point that was touched
                Vector3 hitPoint = ray.GetPoint(enter);
                hitPoint.y = 0.0f;

                // Determine new player rotation
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

    void SpawnProjectile(GameObject projectile)
    {
        // Instantiate projectile at player position and rotation
        GameObject p = Instantiate(projectile, firePoint.transform.position + (firePoint.transform.forward), transform.rotation);

        // If boosted, multiply projectile damage
        if (isBoosted)
        {
            p.GetComponent<ProjectileController>().MultiplyDamage(damageMultiplier);            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Player dies on collision with bacteria
        if (!dead && (collision.gameObject.CompareTag("BadBacteria") || collision.gameObject.CompareTag("Shield")))
        {
            dead = true;
            gameController.PlayerDied();
        } else if (collision.gameObject.CompareTag("GoodBacteria"))
        {
            if (!isBoosted)
            {
                StartCoroutine(TriggerAttackBoost());
            }
            collision.gameObject.GetComponent<GoodBacteria>().KillBacteria();
        }
    }

    //Coroutine for boost duration
    private IEnumerator TriggerAttackBoost()
    {
        isBoosted = true;
        yield return new WaitForSeconds(boostDuration);
        isBoosted = false;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Player Movement")]
    public float speed = 8f;
    [Range(0, 1)]
    public float maxVelocity;

    [Header("Projectile")]
    public GameObject firePoint;
    public GameObject projectile1;
    public float fireRateP1 = 10f;
    public GameObject projectile2;
    public float fireRateP2 = 0.5f;

    [Header("Attack Boost")]
    public float damageMultiplier = 1.5f;
    public float boostDuration = 5f;


    /*** PRIVATE VARIABLES ***/

    // Componenents
    private Rigidbody rb;
    private GameController gameController;
    private Plane plane;

    // Fire time buffer
    private float timeToFireP1 = 0f;
    private float timeToFireP2 = 0f;

    // Status
    private bool dead = false;
    private bool isBoosted = false;

    // Can player interact
    private bool canMove = false;


    /*** INSTANCE ***/

    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        gameController = GameController.Instance;
        plane = new Plane(Vector3.up, Camera.main.transform.position.y);

        // Prevent player movement from start
        canMove = false;
    }

    void Update()
    {
        // If game not paused
        if (!gameController.IsGamePaused() && canMove)
        {
            if (gameController.CanPlayerShoot())
            {
                // Check if player is firing
                CheckFire();
            }

            if (gameController.CanPlayerMoveCamera())
            {
                // Update player rotation
                UpdateRotation();
            }
        }
    }

    void FixedUpdate()
    {
        if (!gameController.IsGamePaused() && canMove && gameController.CanPlayerMove())
        {
            MovePlayer();
        }
    }


    /*** FIRE FUNCTIONS ***/

    // Check if player is firing
    private void CheckFire()
    {
        if (Input.GetButton("Fire1") && Time.time >= timeToFireP1)
        {
            // Fire projectile 1
            Fire(ref timeToFireP1, ref fireRateP1, ref projectile1, 2.0f);
        }else if (Input.GetButton("Fire2") && Time.time >= timeToFireP2)
        {
            // Fire projectile 2
            Fire(ref timeToFireP2, ref fireRateP2, ref projectile2, 2.0f);

            // Increase all proba
            gameController.IncreaseAllMutationProba();
        }
    }

    // Fire a projectile
    private void Fire(ref float timeToFire, ref float fireRate, ref GameObject projectile, float fireDrawback)
    {
        // Update next time to fire
        timeToFire = Time.time + 1 / fireRate;

        // Spawn the projectile
        SpawnProjectile(projectile);

        // Apply a drawback force
        ApplyFireDrawback(fireDrawback);
    }

    // Spawn the desired projectile
    void SpawnProjectile(GameObject projectile)
    {
        // Instantiate projectile at player position and rotation
        GameObject p = Instantiate(projectile, firePoint.transform.position, transform.rotation);

        // If boosted, multiply projectile damage
        if (isBoosted)
        {
            p.GetComponent<Projectile>().MultiplyDamage(damageMultiplier);            
        }
    }

    /*** MOVEMENTS FUNCTIONS ***/

    // Toggle player movement
    public void SetCanMove(bool b)
    {
        canMove = b;
    }

    // Move the player from input
    private void MovePlayer()
    {
        // Get player inputs
        float moveHor = Input.GetAxis("Horizontal");
        float moveVer = Input.GetAxis("Vertical");

        // Create movement vector
        Vector3 movement = new Vector3(moveHor, 0.0f, moveVer);

        // Apply a force to move the player
        rb.AddForce(movement * speed, ForceMode.Impulse);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    // Apply a drawback force to the player when firing
    private void ApplyFireDrawback(float drawbackForce)
    {
        // Get the direction of the drawback
        Vector3 drawbackDirection = transform.position - firePoint.transform.position;
        drawbackDirection.Normalize();

        // Apply force
        rb.AddForce(drawbackDirection * drawbackForce, ForceMode.Impulse);
    }

    // Update the rotation of the player toward aiming direction
    public void UpdateRotation()
    {
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


    /*** COLLISION FUNCTIONS ***/

    private void OnCollisionEnter(Collision collision)
    {
        // Player dies on collision with bacteria
        if (!dead && (collision.gameObject.CompareTag("BadBacteria")))
        {
            dead = true;
            gameController.PlayerDied();
        } else if (collision.gameObject.CompareTag("GoodBacteria"))
        {
            // Boost player when hit good bacteria
            if (!isBoosted)
            {
                StartCoroutine(TriggerAttackBoost());
            }

            // Kill the good bacteria afterwards
            collision.gameObject.GetComponent<GoodBacteria>().KillBacteria();
        }
    }

    // Buffer for boost duration
    private IEnumerator TriggerAttackBoost()
    {
        isBoosted = true;
        yield return new WaitForSeconds(boostDuration);
        isBoosted = false;
    }
}


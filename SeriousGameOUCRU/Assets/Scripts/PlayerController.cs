using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Player Movement")]
    public float speed = 8f;
    public float maxVelocity = 20f;
    public Joystick joystick;
    public bool androidDebug = false;

    [Header("Projectile")]
    public GameObject firePoint;
    public GameObject projectile1;
    public float fireRateP1 = 10f;
    public float fireDrawbackP1 = 2f;
    public GameObject projectile2;
    public float fireRateP2 = 0.5f;
    public float fireDrawbackP2 = 30f;

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

    // Move direction
    Vector3 moveDirection = Vector3.zero;


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
        plane = new Plane(Vector3.up, 0);

        // Prevent player movement from start
        canMove = false;
    }

    void Update()
    {
        // If game not paused
        if (!gameController.IsGamePaused() && canMove)
        {
            // if (gameController.CanPlayerShoot())
            // {
            //     // Check if player is firing
            //     CheckFire();
            // }

        }

        // if (Input.touchCount > 0)
        //     Debug.Log("TOUCH 0: " + Input.GetTouch(0).position);
        // if (Input.touchCount > 1)
        //     Debug.Log("TOUCH 1: " + Input.GetTouch(1).position);
    }

    void FixedUpdate()
    {
        if (!gameController.IsGamePaused() && canMove && gameController.CanPlayerMove() /*&& gameController.CanPlayerMoveCamera()*/)
        {
            // Move and rotate player every frame according to platform
            if (androidDebug || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                MoveAndRotatePlayerMobile();
            }else
            {
                MoveAndRotatePlayerDesktop();
            }
        }
    }


    /*** FIRE FUNCTIONS ***/

    // Check if player is firing
    private void CheckFire()
    {
        if (Input.touchCount > 0)
        {

        }else
        {
            if (Input.GetButton("Fire1") && Time.time >= timeToFireP1)
            {
                // Fire projectile 1
                Fire(ref timeToFireP1, ref fireRateP1, ref projectile1, fireDrawbackP1);
            }else if (Input.GetButton("Fire2") && Time.time >= timeToFireP2)
            {
                // Fire projectile 2
                Fire(ref timeToFireP2, ref fireRateP2, ref projectile2, fireDrawbackP2);

                // Increase all proba
                gameController.IncreaseAllMutationProba();
            }

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
    private void MovePlayer(Vector3 movementDirection)
    {
        // Apply a force to move the player in movementDirection
        rb.AddForce(movementDirection * speed, ForceMode.Impulse);

        // Clamp the player velocity to not go too fast
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    private void RotatePlayer(Vector3 lookAtPosition)
    {
        transform.rotation = Quaternion.LookRotation(lookAtPosition, Vector3.up);
    }

    // Handle player movement and rotation for desktop
    private void MoveAndRotatePlayerDesktop()
    {
        // Get input axes
        float moveHor = Input.GetAxis("Horizontal");
        float moveVer = Input.GetAxis("Vertical");

        // COmpute moveDirection
        moveDirection = new Vector3(moveHor, 0.0f, moveVer);

        // Move the player in axis direction
        MovePlayer(moveDirection);

        // Rotate the player toward mouse position
        RotatePlayer(ScreenPositionToWorldPosition(Input.mousePosition));
    }

    // Handle player movement and rotation for mobile
    private void MoveAndRotatePlayerMobile()
    {
        if (Input.touchCount > 0)
        {
            // Get touch position on screen
            Vector2 touchPosition = Input.GetTouch(0).position;

            // Convert it to world position and keep Y always at player level (0)
            Vector3 touchWorldPosition = ScreenPositionToWorldPosition(touchPosition);
            touchWorldPosition.y = 0;

            // Compute moveDirection and normalize it
            moveDirection = touchWorldPosition - transform.position;
            moveDirection.Normalize();

            // Move and rotate player toward moveDirection
            MovePlayer(moveDirection);
            RotatePlayer(moveDirection);
        }
    }

    // Convert properly a screen position to a world position with raycasting
    private Vector3 ScreenPositionToWorldPosition(Vector2 screenPosition)
    {
        // Create a ray from screen point in world
        Ray ray = CameraController.Instance.GetCamera().ScreenPointToRay(screenPosition);
        float enter = 0.0f;

        // Get the point that intersect the plane at height 0
        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            hitPoint.y = 0.0f;
            return hitPoint;
        }
        
        // Return vector by default
        return Vector3.zero;
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


    /*** GETTERS ***/

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }
}


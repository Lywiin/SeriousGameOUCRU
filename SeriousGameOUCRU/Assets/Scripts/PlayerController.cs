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

    [Header("Attack Range")]
    public float minRange = 15f;
    public float maxRange = 50f;


    /*** PRIVATE VARIABLES ***/

    // Componenents
    private Rigidbody rb;
    private GameController gameController;
    private Plane plane;

    // Fire time buffer
    private float timeToFire;
    private bool isFiring = false;
    private GameObject fireTarget;
    private bool heavyWeaponSelected = false;

    // Intermediate fire variables
    private GameObject currentProjectile;
    private float currentFireRate;
    private float currentFireDrawback;

    // Status
    private bool dead = false;
    private bool isBoosted = false;

    // Can player interact
    private bool canMove = false;

    // Move direction
    Vector3 moveDirection = Vector3.zero;
    private bool keepDistance = false;
    private float currentMaxVelocity;


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

        // Init projectile
        currentProjectile = projectile1;
        currentFireRate = fireRateP1;
        currentFireDrawback = fireDrawbackP1;

        currentMaxVelocity = maxVelocity;
    }

    void Update()
    {
        // If game not paused
        if (!gameController.IsGamePaused() && canMove)
        {
            if (androidDebug || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CheckFireMobile();
            }else
            {
                CheckFireDesktop();
            }
        }
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

    private void CheckFireMobile()
    {
        // Check if player touch the screen and if touch began
        if (Input.touchCount > 0 && Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Began)
        {
            // Get touched world position
            Vector3 touchWorld = ScreenPositionToWorldPosition(Input.GetTouch(Input.touchCount - 1).position);

            // Check if the touch collide with objects in the world
            Collider[] hitColliders = Physics.OverlapSphere(touchWorld, 7f);

            foreach (Collider c in hitColliders)
            {
                // If we touched a bacteria
                if (c.CompareTag("BadBacteria") || c.CompareTag("GoodBacteria"))
                {
                    StartCoroutine(RepeatFire(1f, c.gameObject));
                    break;
                }
            }
        }
    }

    private void CheckFireDesktop()
    {
        if (Time.time >= timeToFire)
        {
            if (Input.GetButton("Fire1"))
            {
                // Switch to light weapon if heavy selected
                if (heavyWeaponSelected)
                    ChangeWeapon();

            }else if (Input.GetButton("Fire2"))
            {
                // Switch to heavy weapon if light selected
                if (!heavyWeaponSelected)
                    ChangeWeapon();
            }
            Fire();
        }
    }

    // Fire projectile over time
    private IEnumerator RepeatFire(float time, GameObject target)
    {
        isFiring = true;
        keepDistance = true;
        fireTarget = target;

        do
        {
            // Keep the player rotated toward the target
            RotatePlayer(fireTarget.transform.position - transform.position);

            // Fire projectile as normal
            if (Time.time >= timeToFire)
                Fire();

            yield return null;

        // Keep firing until bacteria die or get out of range
        }while (fireTarget && Vector3.Distance(transform.position, fireTarget.transform.position) < maxRange && !heavyWeaponSelected);

        isFiring = false;

        // If fire heavy projectile stop keeping distance after some time to prevent unintended movement toward the bacteria
        if (heavyWeaponSelected)
            StartCoroutine(UnkeepDistance(2f));
        else
            StartCoroutine(UnkeepDistance(0f));
    }

    // Stop keeping distance with bacteria after some time
    private IEnumerator UnkeepDistance(float delay)
    {
        yield return new WaitForSeconds(delay);
        keepDistance = false;
        fireTarget = null;
    }

    // Fire a projectile
    private void Fire()
    {
        // Update next time to fire
        timeToFire = Time.time + 1 / currentFireRate;

        // Spawn the projectile
        SpawnProjectile(currentProjectile);

        // Apply a drawback force
        ApplyFireDrawback(currentFireDrawback);

        // Increase mutation proba if heavy projectile is fired
        if (heavyWeaponSelected)
            GameController.Instance.IncreaseAllMutationProba();
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

    // Called by UI to change current weapon
    public void ChangeWeapon()
    {
        // Switch weapon 
        heavyWeaponSelected = !heavyWeaponSelected;

        // Switch to heavy weapon
        if (heavyWeaponSelected)
        {
            currentProjectile = projectile2;
            currentFireRate = fireRateP2;
            currentFireDrawback = fireDrawbackP2;
        }
        // Else switch to light weapon
        else
        {
            currentProjectile = projectile1;
            currentFireRate = fireRateP1;
            currentFireDrawback = fireDrawbackP1;
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
        
        // If player is firing and too close from a bacteria it gets repulsed from it
        if (keepDistance && fireTarget && Vector3.Distance(transform.position, fireTarget.transform.position) < minRange)
        {
            // Get the opposite force direction
            Vector3 forceDirection = transform.position - fireTarget.transform.position;
            forceDirection.Normalize();

            // Apply the opposite force
            rb.AddForce(forceDirection * speed, ForceMode.Impulse);
        }

        // Clamp the player velocity to not go too fast
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, currentMaxVelocity);
    }

    private void RotatePlayer(Vector3 lookAtDirection)
    {
        // Rotate player toward a direction
        transform.rotation = Quaternion.LookRotation(lookAtDirection, Vector3.up);
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
        if (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Began)
        {
            // Get touch position on screen
            Vector2 touchPosition = Input.GetTouch(0).position;

            // Convert it to world position and keep Y always at player level (0)
            Vector3 touchWorldPosition = ScreenPositionToWorldPosition(touchPosition);
            touchWorldPosition.y = 0;

            // Compute moveDirection
            moveDirection = touchWorldPosition - transform.position;

            // Compute new max velocity
            ComputeCurrentMaxVelocity(moveDirection);

            moveDirection.Normalize();

            // Only move and rotate if player clock away from the player
            if ((touchWorldPosition - transform.position).magnitude > 10f)
            {
                // Move player toward moveDirection
                MovePlayer(moveDirection);

                // Rotate player toward moveDirection if not firing
                if (!isFiring)
                    RotatePlayer(moveDirection);
            }else
            {
                // if touch on the player, doesn't move or rotate
                moveDirection = Vector3.zero;
            }

        }
    }

    // Change max velocity according to input distance from the player
    private void ComputeCurrentMaxVelocity(Vector3 inputDistance)
    {
        // Compute a multiplier
        float velocityMultiplier = inputDistance.magnitude / 60f + 0.5f;

        // apply this multiplier on base velocity
        currentMaxVelocity = maxVelocity * velocityMultiplier;

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


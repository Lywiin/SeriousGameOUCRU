using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Player Movement")]
    public float speed = 80f;
    public float maxVelocity = 20f;

    [Header("Projectile")]
    public GameObject firePoint;
    public float fireRateP1 = 0.1f;
    public float fireDrawbackP1 = 2f;
    public float fireRateP2 = 0.5f;
    public float fireDrawbackP2 = 30f;

    [Header("Weapon Change")]
    public float weaponChangeDuration = 0.5f;
    public float weaponChangeCooldown = 0.5f;

    [Header("Attack Range")]
    public float minRange = 10f;
    public float maxRange = 60f;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private MobileUI mobileUI;

    // Components
    private Rigidbody2D rb;

    private Organism fireTarget;

    // Weapon change
    private bool heavyWeaponSelected = false;
    private float weaponChangeTimer = 0f;

    // Status
    private bool isDead = false;

    // Move direction
    private Vector2 moveDirection = Vector3.zero;
    private float currentMaxVelocity;

    private Vector2 oppositeForceDirection;
    private float targetDistance;


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
        gameController = GameController.Instance;
        mobileUI = MobileUI.Instance;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();

        currentMaxVelocity = maxVelocity;

        oppositeForceDirection = Vector2.zero;
        targetDistance = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }


    /*** FIRE FUNCTIONS ***/

    // If not firing start to fire, otherwise just change the target
    public void RepeatFire(Organism newTarget)
    {
        if (!fireTarget) 
        {
            fireTarget = newTarget;
            StartCoroutine(StartRepeatFire());
        }else
        {
            fireTarget = newTarget;
        }
    }

    // Fire projectile over time
    public IEnumerator StartRepeatFire()
    {
        do
        {
            // Keep the player rotated toward the target
            RotatePlayer(fireTarget.transform.position - transform.position);

            Fire();

            // yield return null;
            yield return new WaitForSeconds(heavyWeaponSelected ? fireRateP2 : fireRateP1);

        // Keep firing until cell die or get out of range
        } while (fireTarget && !fireTarget.IsFading() && Vector3.Distance(transform.position, fireTarget.transform.position) < maxRange && !heavyWeaponSelected);

        fireTarget = null;
    }

    // Fire a projectile
    private void Fire()
    {
        // Spawn the projectile
        SpawnProjectile();

        // Apply a drawback force
        ApplyFireDrawback(heavyWeaponSelected? fireDrawbackP2 : fireDrawbackP1);

        // Increase mutation proba if heavy projectile is fired
        if (heavyWeaponSelected)
            gameController.IncreaseAllMutationProba();
    }

    // Spawn the desired projectile
    void SpawnProjectile()
    {
        // Instantiate projectile at player position and rotation
        if (heavyWeaponSelected)
            ProjectileHeavy.InstantiateProjectileHeavy(firePoint.transform.position, transform.rotation, fireTarget);
        else
            ProjectileLight.InstantiateProjectileLight(firePoint.transform.position, transform.rotation, fireTarget);
    }

    // Called by UI to change current weapon
    public void ChangeWeapon()
    {
        gameController.SetCanPlayerChangeWeapon(false);

        // Reset target when changing weapon
        fireTarget = null;
        
        // Switch weapon 
        heavyWeaponSelected = !heavyWeaponSelected;
        mobileUI.ToggleCurrentWeaponImage(heavyWeaponSelected);

        StartCoroutine(ChangeWeaponBuffer());
    }

    private IEnumerator ChangeWeaponBuffer()
    {
        // Only allow weapon change if not blocked by tutorial
        if (!Tutorial.Instance || Tutorial.Instance && !Tutorial.Instance.IsWeaponChangedBlocked())
        {
            yield return new WaitForSeconds(weaponChangeCooldown);
            gameController.SetCanPlayerChangeWeapon(true);
        }
    }



    /*** MOVEMENTS FUNCTIONS ***/

    // Move the player from input
    private void MovePlayer()
    {
        if (fireTarget)
        {
            // Compute values used to repeal player from target
            oppositeForceDirection = transform.position - fireTarget.transform.position;
            targetDistance = oppositeForceDirection.magnitude - (fireTarget.GetOrganismSize() / 2);
        }

        if (fireTarget && targetDistance < minRange)
        {
            // Apply opposite force if player too close from target
            rb.AddForce(oppositeForceDirection.normalized * speed * (1f - targetDistance / minRange), ForceMode2D.Impulse);
        }else
        {
            // Apply a force to move the player in movementDirection
            rb.AddForce(moveDirection * speed, ForceMode2D.Impulse);
        }

        // Clamp the player velocity to not go too fast
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, currentMaxVelocity);
    }

    private void RotatePlayer(Vector2 lookAtDirection)
    {
        // Rotate player toward a direction
        float angle = Mathf.Atan2(lookAtDirection.y, lookAtDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    public void MovePlayerMobile(Vector2 inputDistance)
    {
        moveDirection = inputDistance.normalized;

        ResetWeaponChangeTimer();

        // Rotate player toward moveDirection if not firing
        if (!fireTarget)
            RotatePlayer(moveDirection);
    }

    public void StayPlayerMobile()
    {
        // if touch on the player, doesn't move or rotate
        ResetMoveDirection();
        
        // Increase timer to change weapon
        if (gameController.CanPlayerChangeWeapon())
            IncreaseWeaponChangeTimer();
    }

    private void IncreaseWeaponChangeTimer()
    {
        // Increase timer every frame
        weaponChangeTimer += Time.deltaTime;

        // Update UI slider
        mobileUI.FillWeaponChangeSlider(weaponChangeTimer / weaponChangeDuration);

        // When timer over weapon changing duration trigger the weapon changing procedure
        if (weaponChangeTimer > weaponChangeDuration)
        {
            ResetWeaponChangeTimer();
            ChangeWeapon();
        }
    }

    public void ResetWeaponChangeTimer()
    {
        // Reset timer
        weaponChangeTimer = 0f;

        // Reset UI slider
        mobileUI.FillWeaponChangeSlider(0f);
    }

    // Change max velocity according to input distance from the player
    public void ComputeCurrentMaxVelocity(Vector2 inputDistance)
    {
        // Compute a multiplier
        float velocityMultiplier = inputDistance.magnitude / 60f + 0.5f;

        // apply this multiplier on base velocity
        currentMaxVelocity = maxVelocity * velocityMultiplier;

    }

    // Apply a drawback force to the player when firing
    private void ApplyFireDrawback(float drawbackForce)
    {
        // Get the direction of the drawback
        Vector2 drawbackDirection = transform.position - firePoint.transform.position;
        drawbackDirection.Normalize();

        // Apply force
        rb.AddForce(drawbackDirection * drawbackForce, ForceMode2D.Impulse);
    }


    /*** COLLISION FUNCTIONS ***/

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player dies on collision with cell
        if (!isDead && collision.gameObject.layer == LayerMask.NameToLayer("Ennemy") && SceneManager.GetActiveScene().buildIndex != 1)
        {
            isDead = true;
            gameController.GameOver();
            Destroy(gameObject);
        }
    }


    /*** GETTERS/SETTERS ***/

    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    
    public bool IsHeavyWeaponSelected()
    {
        return heavyWeaponSelected;
    }

    public void ResetMoveDirection()
    {
        moveDirection = Vector2.zero;
    }

    public bool IsFiring()
    {
        return fireTarget ? true : false;
    }
}


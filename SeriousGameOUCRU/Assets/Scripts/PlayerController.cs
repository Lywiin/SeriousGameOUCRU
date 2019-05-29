﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Player Movement")]
    public float speed = 8f;
    public float maxVelocity = 20f;

    [Header("Projectile")]
    public GameObject firePoint;
    public GameObject projectile1;
    public float fireRateP1 = 10f;
    public float fireDrawbackP1 = 2f;
    public GameObject projectile2;
    public float fireRateP2 = 2f;
    public float fireDrawbackP2 = 30f;

    [Header("Weapon Change")]
    public float weaponChangeDuration = 0.5f;
    public float weaponChangeCooldown = 0.5f;

    [Header("Attack Range")]
    public float minRange = 15f;
    public float maxRange = 60f;


    /*** PRIVATE VARIABLES ***/

    // Componenents
    private Rigidbody rb;

    // Fire time buffer
    private float timeToFire;
    private bool isFiring = false;
    private GameObject fireTarget;

    // Weapon change
    private bool heavyWeaponSelected = false;
    private bool canChangeWeapon = true;
    private float weaponChangeTimer = 0f;

    // Intermediate fire variables
    private GameObject currentProjectile;
    private float currentFireRate;
    private float currentFireDrawback;

    // Status
    private bool dead = false;

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

        // Prevent player movement from start
        canMove = false;

        // Init projectile
        currentProjectile = projectile1;
        currentFireRate = fireRateP1;
        currentFireDrawback = fireDrawbackP1;

        currentMaxVelocity = maxVelocity;
    }


    /*** FIRE FUNCTIONS ***/

    // Fire projectile over time
    public IEnumerator RepeatFire(GameObject target)
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

        // Keep firing until cell die or get out of range
        }while (fireTarget && Vector3.Distance(transform.position, fireTarget.transform.position) < maxRange && !heavyWeaponSelected && fireTarget.GetComponent<Collider>().enabled);

        isFiring = false;

        // If fire heavy projectile stop keeping distance after some time to prevent unintended movement toward the cell
        if (heavyWeaponSelected)
            StartCoroutine(UnkeepDistance(2f));
        else
            StartCoroutine(UnkeepDistance(0f));
    }

    // Stop keeping distance with cell after some time
    private IEnumerator UnkeepDistance(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isFiring)
        {
            keepDistance = false;
            fireTarget = null;
        }
    }

    public void FireDesktop()
    {
        if (Time.time >= timeToFire)
        {
            Fire();
        }
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
    }

    // Called by UI to change current weapon
    public IEnumerator ChangeWeapon()
    {
        canChangeWeapon = false;
        
        // Switch weapon 
        heavyWeaponSelected = !heavyWeaponSelected;

        MobileUI.Instance.ToggleCurrentWeaponImage(heavyWeaponSelected);

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

        yield return new WaitForSeconds(weaponChangeCooldown);
        canChangeWeapon = true;
    }



    /*** MOVEMENTS FUNCTIONS ***/

    // Toggle player movement
    public void SetCanMove(bool b)
    {
        canMove = b;
    }

    // Move the player from input
    public void MovePlayer(Vector3 movementDirection)
    {
        // Apply a force to move the player in movementDirection
        rb.AddForce(movementDirection * speed, ForceMode.Impulse);
        
        // If player is firing and too close from a cell it gets repulsed from it
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

    public void RotatePlayer(Vector3 lookAtDirection)
    {
        // Rotate player toward a direction
        transform.rotation = Quaternion.LookRotation(lookAtDirection, Vector3.up);
    }

    public void MovePlayerMobile(Vector3 inputDistance)
    {
        inputDistance.Normalize();
        moveDirection = inputDistance;

        // Reset timer
        ResetWeaponChangeTimer();

        // Move player toward moveDirection
        MovePlayer(moveDirection);

        // Rotate player toward moveDirection if not firing
        if (!isFiring)
            RotatePlayer(moveDirection);
    }

    public void NotMovePlayerMobile()
    {
        // if touch on the player, doesn't move or rotate
        moveDirection = Vector3.zero;
        
        // Increase timer to change weapon
        IncreaseWeaponChangeTimer();
    }

    private void IncreaseWeaponChangeTimer()
    {
        // Condition needed to change weapon
        if (canChangeWeapon)
        {
            // Increase timer every frame
            weaponChangeTimer += Time.deltaTime;

            // Update UI slider
            MobileUI.Instance.FillWeaponChangeSlider(weaponChangeTimer / weaponChangeDuration);

            // When timer over weapon changing duration trigger the weapon changing procedure
            if (weaponChangeTimer > weaponChangeDuration)
            {
                ResetWeaponChangeTimer();
                StartCoroutine(ChangeWeapon());
            }
        }
    }

    public void ResetWeaponChangeTimer()
    {
        // Reset timer
        weaponChangeTimer = 0f;

        // Reset UI slider
        MobileUI.Instance.FillWeaponChangeSlider(0f);
    }

    // Change max velocity according to input distance from the player
    public void ComputeCurrentMaxVelocity(Vector3 inputDistance)
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
        Vector3 drawbackDirection = transform.position - firePoint.transform.position;
        drawbackDirection.Normalize();

        // Apply force
        rb.AddForce(drawbackDirection * drawbackForce, ForceMode.Impulse);
    }


    /*** COLLISION FUNCTIONS ***/

    private void OnCollisionEnter(Collision collision)
    {
        // Player dies on collision with cell
        if (!dead && collision.gameObject.CompareTag("Targetable"))
        {
            dead = true;
            GameController.Instance.GameOver();
        }
    }


    /*** GETTERS ***/

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    public bool CanPlayerMove()
    {
        return canMove;
    }

    public bool IsHeavyWeaponSelected()
    {
        return heavyWeaponSelected;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class PlayerController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Player Movement")]
    public float speed = 80f;
    public float maxVelocity = 15f;
    public float velocityAccelFactor = 1.2f;

    [Header("Projectile")]
    public GameObject firePoint;
    public float fireRateP1 = 0.1f;
    public float fireDrawbackP1 = 2f;
    public float fireRateP2 = 0.5f;
    public float fireDrawbackP2 = 30f;

    [Header("Weapon Change")]
    public float weaponChangeDuration = 0.5f;
    public float weaponChangeCooldown = 0.5f;
    public int maxAntibioticUseBeforeAlert = 1;

    [Header("Attack Range")]
    public float minRange = 10f;
    public float maxRange = 60f;

    [Header("Particles")]
    public ParticleSystem[] smokeParticles;
    public ParticleSystem explosionParticles;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private MobileUI mobileUI;
    private AudioManager audioManager;

    // Components
    private Rigidbody2D rb;

    private Organism fireTarget;

    // Weapon change
    private bool heavyWeaponSelected = false;
    private float weaponChangeTimer = 0f;
    private int antibodyUseCount = 0;
    private int antibioticUseCount = 0;
    private int antibioticUseInRowCount = 0;

    // Move direction
    private Vector2 moveDirection = Vector3.zero;
    private float currentMaxVelocity;

    private Vector2 oppositeForceDirection;
    private float targetDistance;

    private Sound motorSound = null;


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
        audioManager = AudioManager.Instance;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();

        currentMaxVelocity = maxVelocity;

        oppositeForceDirection = Vector2.zero;
        targetDistance = 0f;

        if (AudioManager.Instance) motorSound = AudioManager.Instance.SmoothPlay("ShipMotor", 1f);
    }

    private void FixedUpdate()
    {
        if (gameController.CanPlayerMove())
            MovePlayer();

        // Change motor sound according to velocity
        if (motorSound != null) motorSound.source.pitch = motorSound.pitch + rb.velocity.sqrMagnitude / 1000f;
    }


    /*** FIRE FUNCTIONS ***/

    // If not firing start to fire, otherwise just change the target
    public void RepeatFire(Organism newTarget)
    {
        if (!fireTarget) 
        {
            SetFireTarget(newTarget);
            StartCoroutine(StartRepeatFire());
        }else
        {
            SetFireTarget(newTarget);
        }
    }

    private void SetFireTarget(Organism newTarget)
    {
        if (newTarget != fireTarget)
        {
            fireTarget = newTarget;
            if (SceneManager.GetActiveScene().buildIndex > 1) IncreaseFireCount();
        }
    }

    private void IncreaseFireCount()
    {
        if(IsHeavyWeaponSelected())
        {
            // Increase count
            antibioticUseCount++;
            antibioticUseInRowCount++;

            // If use antibiotic too much in a row display message
            if (antibioticUseInRowCount == maxAntibioticUseBeforeAlert) 
            {
                WeaponUseUI.Instance.DisplayWeaponUsePanel();
            }
        }
        else
        {
            antibodyUseCount++;
            antibioticUseInRowCount = 0;    // Reset count in row
        }
    }

    public void UpdateFireAnalytics()
    {
        AnalyticsEvent.Custom("FireLevel" + (SceneManager.GetActiveScene().buildIndex - 1), new Dictionary<string, object>
        {
            { "antibiotic", antibioticUseCount},
            { "antibody", antibodyUseCount}
        });
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
        } while (fireTarget && !fireTarget.IsFading() && Vector3.Distance(transform.position, fireTarget.transform.position) < maxRange && !heavyWeaponSelected && gameController.CanPlayerShoot());

        fireTarget = null;
    }

    // Fire a projectile
    private void Fire()
    {
        // UpdateVirusAntibioticUseAnalytics();

        // Spawn the projectile
        SpawnProjectile();

        // Apply a drawback force
        ApplyFireDrawback(heavyWeaponSelected? fireDrawbackP2 : fireDrawbackP1);

        // Increase mutation proba if heavy projectile is fired
        if (heavyWeaponSelected)
        {
            if (fireTarget && fireTarget.GetOrgMutation()) fireTarget.GetOrgMutation().ShineShields();
            gameController.IncreaseAllMutationProba();
            if(audioManager) audioManager.Play("FireHeavy");
        }else
        {
            if(audioManager) audioManager.Play("FireLight");
        }
    }

    private void UpdateVirusAntibioticUseAnalytics()
    {
        if (fireTarget && fireTarget.GetComponentInParent<Virus>() && IsHeavyWeaponSelected())
        {
            AnalyticsEvent.Custom("AntibioticUseOnVirusLevel" + (SceneManager.GetActiveScene().buildIndex - 1));
        }
    }

    // Spawn the desired projectile
    private void SpawnProjectile()
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
        if (AudioManager.Instance) AudioManager.Instance.Play("Select1");

        gameController.SetCanPlayerChangeWeapon(false);

        // Reset target when changing weapon
        fireTarget = null;
        
        // Switch weapon 
        heavyWeaponSelected = !heavyWeaponSelected;
        mobileUI.ToggleCurrentWeaponImage(heavyWeaponSelected);

        // Reset antibiotic use too much message
        if (!heavyWeaponSelected && antibioticUseInRowCount >= maxAntibioticUseBeforeAlert) 
        {
            WeaponUseUI.Instance.HideWeaponUsePanel();
            antibioticUseInRowCount = 0;
        }

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
            // if (SceneManager.GetActiveScene().buildIndex > 1) UpdateWeaponChangeAnalytics(); // Not for tutorial
        }
    }

    public void ResetWeaponChangeTimer()
    {
        // Reset timer
        weaponChangeTimer = 0f;

        // Reset UI slider
        mobileUI.FillWeaponChangeSlider(0f);
    }

    private void UpdateWeaponChangeAnalytics()
    {
        AnalyticsEvent.Custom("WeaponChangeLevel" + (SceneManager.GetActiveScene().buildIndex - 1));
    }

    // Change max velocity according to input distance from the player
    public void ComputeCurrentMaxVelocity(Vector2 inputDistance)
    {
        // Compute a multiplier
        float inputDistanceMagnitude = (Mathf.Clamp(inputDistance.magnitude, 8f, 30f) - 8f) / 22f;
        float velocityMultiplier = 1f + inputDistanceMagnitude * velocityAccelFactor;

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
        if (gameController.CanPlayerMove() && collision.gameObject.layer == LayerMask.NameToLayer("Ennemy") && SceneManager.GetActiveScene().buildIndex != 1)
        {
            StartCoroutine(KillShip());
        }
    }

    private IEnumerator KillShip()
    {
        explosionParticles.Play();
        if(AudioManager.Instance) AudioManager.Instance.Play("ShipExplosion");
        StopMotorSound();

        smokeParticles[0].Stop();
        smokeParticles[1].Stop();
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;

        rb.velocity = Vector2.zero;
        gameController.SetCanPlayerMove(false);
        gameController.SetCanPlayerShoot(false);
        gameController.GameOver(true);

        UpdateFireAnalytics();

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }


    /*** SOUNDS FUNCTIONS ***/

    public void StopMotorSound()
    {
        if (motorSound != null && motorSound.source.isPlaying && AudioManager.Instance)
            AudioManager.Instance.SmoothStop("ShipMotor", 0.5f);
        else if (motorSound != null)
            motorSound.source.Stop();
    }

    public void PauseMotorSound()
    {
        if (motorSound != null) motorSound.source.Pause();
    }

    public void PlayMotorSound()
    {
        if (motorSound != null) motorSound.source.Play();
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Camera mainCamera;
    public bool androidDebug = true;

    
    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private CameraController cameraController;

    private float repeatFireTimer = 0f;

    private Plane plane;

    private Vector3 inputDistance;

    /*** INSTANCE ***/

    private static InputController _instance;
    public static InputController Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        // Init plane at player level
        plane = new Plane(Vector3.up, 0);

        inputDistance = Vector3.zero;
    }

    private void Start()
    {
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
        cameraController = CameraController.Instance;
    }

    void Update()
    {
        // If game not paused
        if (!gameController.IsGamePaused() && playerController && gameController.CanPlayerShoot())
        {
            if (androidDebug || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                HandleFireMobile();
            }else
            {
                HandleFireDesktop();
            }
        }
    }

    void FixedUpdate()
    {
        if (!gameController.IsGamePaused() && playerController && gameController.CanPlayerMove())
        {
            // Move and rotate player every frame according to platform
            if (androidDebug || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                HandlePlayerControlsMobile();
            }else
            {
                HandlePlayerControlsDesktop();
            }
        }
    }
    

    /*** FIRE FUNCTIONS ***/

    private void HandleFireDesktop()
    {
        if (playerController)
        {
            if (Input.GetButton("Fire1"))
            {
                if (playerController.IsHeavyWeaponSelected())
                {
                    // Switch to light weapon if heavy selected
                    StartCoroutine(playerController.ChangeWeapon());
                }

                playerController.FireDesktop();

            }else if (Input.GetButton("Fire2"))
            {
                if (!playerController.IsHeavyWeaponSelected())
                {
                    // Switch to heavy weapon if light selected
                    StartCoroutine(playerController.ChangeWeapon());
                }

                playerController.FireDesktop();
            }
        }
    }

    private void HandleFireMobile()
    {
        // Check if player touch the screen and if touch began
        if (Input.touchCount > 0)
        {
            // Increase timer every frame
            repeatFireTimer += Time.deltaTime;

            if (Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Ended)
            {
                // When touch end we check if the input last for less than some time
                if (repeatFireTimer < 0.4f)
                {
                    // If so if try to fire
                    CheckRepeatFireTouch();
                }

                // After every end of touch we reset the timer
                repeatFireTimer = 0f;
            }
        }
    }

    // Check the touch of the player to see if it trigger the repeat fire
    private void CheckRepeatFireTouch()
    {
        // Get touched world position
        Vector2 touchWorld = mainCamera.ScreenToWorldPoint(Input.GetTouch(Input.touchCount - 1).position);

        // Check if the touch collide with objects in the world
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(touchWorld, 7f, 1 << LayerMask.NameToLayer("Ennemy"));

        // Get the futur target
        GameObject bestTarget = GetClosestTarget(hitColliders);
        
        // If this target exist, start to fire at it
        if (bestTarget)
            playerController.RepeatFire(bestTarget.GetComponentInParent<Organism>());
    }

    // Return the closest object to the player
    private GameObject GetClosestTarget(Collider2D[] hitColliders)
    {
        GameObject bestTarget = null;
        float bestDistance = 999999f;  // Init with a high distance

        Vector2 playerPos = playerController.transform.position;

        // Go through all object to find the closest one
        foreach (Collider2D c in hitColliders)
        {
            // If the object touched is targetable we compute his distance to the player
            if (c.CompareTag("Targetable"))
            {
                // Compute distance from player
                Vector2 distance = c.ClosestPoint(playerPos) - playerPos;
                float sqrDistance = distance.sqrMagnitude;

                if (sqrDistance < bestDistance)
                {
                    bestTarget = c.gameObject;
                    bestDistance = sqrDistance;
                }
            }
        }

        return bestTarget;
    }


    /*** MOVEMENTS FUNCTIONS ***/

    // Handle player movement and rotation for desktop
    private void HandlePlayerControlsDesktop()
    {
        if (playerController)
        {
            // Get input axes
            float moveHor = Input.GetAxis("Horizontal");
            float moveVer = Input.GetAxis("Vertical");

            // Compute moveDirection
            Vector2 moveDir = new Vector2(moveHor, moveVer);

            // Move the player in axis direction
            playerController.MovePlayer(moveDir);

            // Rotate the player toward mouse position
            playerController.RotatePlayer((Vector2)(mainCamera.ScreenToWorldPoint(Input.mousePosition) - playerController.transform.position));
        }
    }

    // Handle player movement and rotation for mobile
    private void HandlePlayerControlsMobile()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Began)
        {
            // Compute moveDirection
            inputDistance = ComputeInputDistanceFromPlayer(0);

            // Compute new max velocity
            playerController.ComputeCurrentMaxVelocity(inputDistance);

            // Only move and rotate if player click away from the player
            if (inputDistance.magnitude > 10f)
            {
                playerController.MovePlayerMobile(inputDistance);
            }else
            {
                playerController.NotMovePlayerMobile();
            }
        }else
        {
            // Reset player weapon change timer
            playerController.ResetWeaponChangeTimer();
            playerController.ResetMoveDirection();
        }
    }

    private Vector2 ComputeInputDistanceFromPlayer(int touchIndex)
    {
        // Get touch position on screen
        Vector2 touchPosition = Input.GetTouch(touchIndex).position;

        // Convert it to world position and keep Y always at player level (0)
        Vector2 touchWorldPosition = mainCamera.ScreenToWorldPoint(touchPosition);

        // Compute moveDirection
        return touchWorldPosition - (Vector2)playerController.transform.position;
    }
}

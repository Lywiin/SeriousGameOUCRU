using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Camera Movement")]
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 12f;
    
    [Header("Camera Look At")]
    public float lookAtSpeed = 1.5f;
    public float lookAtFactor = 3.5f;

    [Header("Camera Size")]
    public float cameraSizingSpeed = 2f;
    public float cameraSizingFactor = 1.1f;
    public float cameraSmoothSpeed = 6f;

    [Header("Projectile")]
    public float projectileFollowFactor = 0.1f;
    public float projectileFollowZoomFactor = 2f;
    public float projectileFollowReturnSpeed = 6f;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private InputController inputController;

    private Plane plane;
    private Camera cam;
    private float cameraBaseSize;

    // Camera zone size
    private Vector2 cameraZone;

    // Projectile
    private bool followProjectile = false;
    private ProjectileHeavy projectile;
    private Vector2 projectileOffset = Vector3.zero;

    // Look at offset
    Vector2 lookAtOffset = Vector2.zero;


    /*** INSTANCE ***/

    private static CameraController _instance;
    public static CameraController Instance { get { return _instance; } }


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


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
        inputController = InputController.Instance;

        // plane = new Plane(Vector3.up, 0);
        cam = transform.GetComponentInChildren<Camera>();
        cameraBaseSize = cam.orthographicSize;

        // Initialize zone where the camera can go
        cameraZone = gameController.gameZoneRadius - new Vector2(15f, 5f);
    }

    void FixedUpdate()
    {
        MoveCamera();
    }


    /***** CAMERA FUNCTIONS *****/

    // Move camera according to target movements
    private void MoveCamera()
    {
        if (target != null)
        {
            // Get camera base offset position
            Vector3 desiredPosition = target.position + offset;

            // Smooth that position to add delay in camera movement
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Get look at offset
            ComputeLookAtOffset();

            if (gameController.CanPlayerMoveCamera())
            {
                // Handle the zoom of the camera
                ZoomCamera();
            }

            // Add offset if follow projectile
            if (followProjectile)
            {
                // // Compute new offset
                // Vector2 tempFollowOffset = (projectile.transform.position - target.transform.position) * projectileFollowFactor;

                // // Prevent screen exit
                // tempFollowOffset.y /= 2f;

                // COmpute and affect new offset
                projectileOffset = (projectile.transform.position - target.transform.position) * projectileFollowFactor;
                
                // Prevent screen exit
                projectileOffset /= 2f;

                // Add temporary zoom out
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, cameraBaseSize + projectileOffset.magnitude * projectileFollowZoomFactor, Time.deltaTime * cameraSmoothSpeed);;
                
            }else
            {
                projectileOffset = Vector2.Lerp(projectileOffset, Vector2.zero, projectileFollowReturnSpeed * Time.deltaTime);
            }

            // Set the camera final position to that smooth position plus an offset from player mouse position
            Vector3 finalPosition = smoothedPosition + (Vector3)lookAtOffset + (Vector3)projectileOffset;

            // If player has completed tutorial can move camera everywhere
            if (gameController.CanPlayerMoveCamera())
            {
                // Clamp the final position in the camera zone
                transform.position = new Vector3(Mathf.Clamp(finalPosition.x, -cameraZone.x, cameraZone.x), finalPosition.y, Mathf.Clamp(finalPosition.z, -cameraZone.y, cameraZone.y));
            }else   
            {
                // Otherwise camera only follow player position
                transform.position = smoothedPosition;
            }
        }
    }

    // Return offset position from difference between player and mouse position
    private void ComputeLookAtOffset()
    {
        // Init direction for the offset
        Vector2 offsetDirection = Vector2.zero;

        // Compute offsetDirection if we are on mobile and touch the screen or anything else
        if (Input.touchCount > 0 || (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android && !inputController.androidDebug))
        {
            offsetDirection = playerController.GetMoveDirection() * lookAtFactor;
        }

        // Smooth the offset to be applied
        lookAtOffset = Vector2.Lerp(lookAtOffset, offsetDirection, Time.deltaTime * lookAtSpeed);
    }

    // Zoom the camera when player fires
    private void ZoomCamera()
    {
        // Init desired size
        float desiredSize = cameraBaseSize;

        // Use that offset to zoom camera in or out
        if(Input.GetButton("Fire1"))
        {
            // Compute new camera size and clamp it
            float newSize = cameraBaseSize + lookAtOffset.magnitude * cameraSizingSpeed; 
            desiredSize = Mathf.Clamp(newSize, cameraBaseSize, cameraBaseSize * cameraSizingFactor);
        }

        // Apply the new camera size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.deltaTime * cameraSmoothSpeed);
    }

    public void FollowProjectile(ProjectileHeavy p)
    {
        followProjectile = true;
        projectile = p;
        StartCoroutine(StopFollowProjectile(p.lifeTime + 0.5f));
    }

    private IEnumerator StopFollowProjectile(float t)
    {
        yield return new WaitForSeconds(t);

        // Reset follow
        followProjectile = false;
        projectile = null;
    }


    /***** GETTERS *****/

    public Camera GetCamera()
    {
        return cam;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Camera Movement")]
    public Vector3 offset;
    public float smoothSpeed = 10f;
    
    [Header("Camera Look At Offset")]
    public float lookAtSpeed = 1.5f;
    public float lookAtFactor = 3.5f;

    [Header("Camera Zoom")]
    public float cameraZoomSpeed = 6f;
    public float cameraZoomFactor = 1.15f;
    public float projectileFollowZoomFactor = 2f;

    [Header("Projectile")]
    public float projectileFollowFactor = 0.3f;
    public float projectileFollowReturnSpeed = 3f;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private InputController inputController;

    private Transform targetTransform;

    private Camera cam;
    private float cameraBaseSize;

    // Projectile
    private ProjectileHeavy projectileTarget;
    private Vector3 projectileOffset;

    // Cached
    private Vector3 lookAtOffset;
    private Vector3 desiredPosition;
    private Vector3 smoothedDesiredPosition;
    private Vector3 offsetMovementDirection;
    private Vector3 finalPosition;


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

    private void Start()
    {
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
        inputController = InputController.Instance;

        cam = transform.GetComponentInChildren<Camera>();
        cameraBaseSize = cam.orthographicSize;

        projectileOffset = Vector3.zero;
        lookAtOffset = Vector3.zero;
        desiredPosition = Vector3.zero;
        smoothedDesiredPosition = Vector3.zero;
        offsetMovementDirection = Vector3.zero;
        finalPosition = Vector3.zero;

        targetTransform = PlayerController.Instance.transform;
    }

    private void FixedUpdate()
    {
        MoveCamera();
    }


    /***** CAMERA FUNCTIONS *****/

    // Move camera according to target movements
    private void MoveCamera()
    {
        if (targetTransform != null)
        {
            // Smooth that position to add delay in camera movement
            smoothedDesiredPosition = Vector2.Lerp(transform.position, targetTransform.position, smoothSpeed * Time.deltaTime);

            ComputeLookAtOffset();

            ComputeProjectileOffset();

            ComputeCameraZoom();

            // Set the final position 
            transform.position = smoothedDesiredPosition + lookAtOffset + projectileOffset + offset;
        }
    }

    // Return offset position from difference between player and mouse position
    private void ComputeLookAtOffset()
    {
        offsetMovementDirection = playerController.GetMoveDirection() * lookAtFactor;

        // Smooth the offset to be applied
        lookAtOffset = Vector2.Lerp(lookAtOffset, offsetMovementDirection, Time.deltaTime * lookAtSpeed);
    }

    private void ComputeProjectileOffset()
    {
        // Add offset if follow projectile
        if (projectileTarget)
        {
            // Compute and affect new offset
            projectileOffset = (projectileTarget.transform.position - targetTransform.position) * projectileFollowFactor;
            
            // Prevent screen exit
            projectileOffset /= 2f;            
        }else
        {
            projectileOffset = Vector2.Lerp(projectileOffset, Vector2.zero, projectileFollowReturnSpeed * Time.deltaTime);
        }
    }

    // Zoom the camera when player fires
    private void ComputeCameraZoom()
    {
        if (projectileTarget)
        {
            // Zoom camera out when fire projectile
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, cameraBaseSize + projectileOffset.magnitude * projectileFollowZoomFactor, Time.deltaTime * cameraZoomSpeed);
        }else
        {
            // Init desired size
            float desiredSize = cameraBaseSize;

            if(Input.touchCount > 0)
            {
                // Compute new camera size
                desiredSize = cameraBaseSize + lookAtOffset.magnitude * cameraZoomFactor; 
            }

            // Apply the new camera size
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.deltaTime * cameraZoomSpeed);
        }
    }

    public void StartFollowProjectile(ProjectileHeavy p)
    {
        projectileTarget = p;
    }

    public void StopFollowProjectile()
    {
        projectileTarget = null;
    }


    /***** GETTERS/SETTERS *****/

    public Camera GetCamera()
    {
        return cam;
    }
}

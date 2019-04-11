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
    public float lookAtSpeed = 1.5f;

    [Header("Camera Size")]
    public float cameraSizingSpeed = 2.5f;
    public float cameraSizingFactor = 1.2f;
    public float cameraSmoothSpeed = 6f;


    /*** PRIVATE VARIABLES ***/

    private Plane plane;
    private Camera cam;
    private float cameraBaseSize;

    // Camera zone size
    private Vector2 cameraZone;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        plane = new Plane(Vector3.up, Vector3.zero);
        cam = transform.GetComponentInChildren<Camera>();
        cameraBaseSize = cam.orthographicSize;

        // Initialize zone where the camera can go
        cameraZone = GameController.Instance.gameZoneRadius - new Vector2(15f, 5f);
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
            Vector3 lookAtOffset = GetLookAtOffset();

            if (GameController.Instance.CanPlayerMoveCamera())
            {
                // Handle the zoom of the camera
                ZoomCamera(lookAtOffset);
            }

            // Set the camera final position to that smooth position plus an offset from player mouse position
            Vector3 finalPosition = smoothedPosition + lookAtOffset;

            // If player has completed tutorial can move camera everywhere
            if (GameController.Instance.CanPlayerMoveCamera())
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
    private Vector3 GetLookAtOffset()
    {
        //Debug.Log(Input.mousePosition);
        // Create a raycast to get mouse position in world
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        Vector3 lookAtOffset = Vector3.zero;

        if (plane.Raycast(ray, out enter))
        {
            // Get the point that was touched
            Vector3 hitPoint = ray.GetPoint(enter);

            // Compute offset position and clamp it to have the same max distance everywhere on the screen
            Vector3 direction = hitPoint - target.position;
            direction.Normalize();
            direction *= cam.orthographicSize;
            lookAtOffset = direction * Time.deltaTime * lookAtSpeed;
        }

        return lookAtOffset;
    }

    // Zoom the camera when player fires
    private void ZoomCamera(Vector3 lookAtOffset)
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 12f;
    public float lookAtSpeed = 1.5f;

    [Header("Camera Size")]
    public float cameraSizingSpeed = 2f;
    public float cameraSizingFactor = 1.3f;
    public float cameraSmoothSpeed = 6f;

    private Plane plane;
    private Camera cam;
    private float cameraBaseSize;

    void Start()
    {
        plane = new Plane(Vector3.up, Vector3.zero);
        cam = transform.GetComponentInChildren<Camera>();
        cameraBaseSize = cam.orthographicSize;
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Get camera base offset position
            Vector3 desiredPosition = target.position + offset;

            // Smooth that position to add delay in camera movement
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Get look at offset
            Vector3 lookAtOffset = GetLookAtOffset();

            float desiredSize = cameraBaseSize;
            // Use that offset to zoom camera in or out
            if(Input.GetButton("Fire1"))
            {
                float newSize = cameraBaseSize + lookAtOffset.magnitude * cameraSizingSpeed;
                desiredSize = Mathf.Clamp(newSize, cameraBaseSize, cameraBaseSize * cameraSizingFactor);
            }
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.deltaTime * cameraSmoothSpeed);

            // Set the camera position to that smooth position plus an offset from player mouse position
            transform.position = smoothedPosition + lookAtOffset;
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
            Vector3 direction = hitPoint - target.position; //Vector3.ClampMagnitude(hitPoint - target.position, cam.orthographicSize);
            direction.Normalize();
            direction *= cam.orthographicSize;
            lookAtOffset = direction * Time.deltaTime * lookAtSpeed;
        }

        return lookAtOffset;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 12f;
    public float lookAtSpeed = 1.5f;

    private Plane plane;

    void Start()
    {
        plane = new Plane(Vector3.up, Vector3.zero);
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Get camera base offset position
            Vector3 desiredPosition = target.position + offset;

            // Smooth that position to add delay in camera movement
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Set the camera position to that smooth position plus an offset from player mouse position
            transform.position = smoothedPosition + GetLookAtOffset();
        }
    }

    // Return offset position from difference between player and mouse position
    private Vector3 GetLookAtOffset()
    {
        // Create a raycast to get mouse position in world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        Vector3 lookAtOffset = Vector3.zero;

        if (plane.Raycast(ray, out enter))
        {
            // Get the point that was touched
            Vector3 hitPoint = ray.GetPoint(enter);

            // Compute offset position
            lookAtOffset = (hitPoint - target.position) * Time.deltaTime * lookAtSpeed;
        }

        return lookAtOffset;
    }
}

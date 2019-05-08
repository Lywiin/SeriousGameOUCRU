﻿using System.Collections;
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

    private Plane plane;
    private Camera cam;
    private float cameraBaseSize;

    // Camera zone size
    private Vector2 cameraZone;

    // Projectile
    private bool followProjectile = false;
    private ProjectileHeavy projectile;
    private Vector3 projectileOffset = Vector3.zero;

    // Look at offset
    Vector3 lookAtOffset = Vector3.zero;


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
        plane = new Plane(Vector3.up, 0);
        cam = transform.GetComponentInChildren<Camera>();
        cameraBaseSize = cam.fieldOfView;

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
            ComputeLookAtOffset();

            if (GameController.Instance.CanPlayerMoveCamera())
            {
                // Handle the zoom of the camera
                ZoomCamera(lookAtOffset);
            }

            // Add offset if follow projectile
            if (followProjectile)
            {
                // Compute new offset
                Vector3 tempFollowOffset = (projectile.transform.position - target.transform.position) * projectileFollowFactor;

                // Prevent screen exit
                tempFollowOffset.z /= 2f;

                // Affect new offset
                projectileOffset = tempFollowOffset;

                // Add temporary zoom out
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cameraBaseSize + projectileOffset.magnitude * projectileFollowZoomFactor, Time.deltaTime * cameraSmoothSpeed);;
                
            }else
            {
                projectileOffset = Vector3.Lerp(projectileOffset, Vector3.zero, projectileFollowReturnSpeed * Time.deltaTime);
            }

            // Set the camera final position to that smooth position plus an offset from player mouse position
            Vector3 finalPosition = smoothedPosition + lookAtOffset + projectileOffset;

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
    private void ComputeLookAtOffset()
    {
        // Init direction for the offset
        Vector3 offsetDirection = Vector3.zero;

        // Compute offsetDirection if we are on mobile and touch the screen or anything else
        if (Input.touchCount > 0 || (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android && !PlayerController.Instance.androidDebug))
        {
            offsetDirection = PlayerController.Instance.GetMoveDirection() * lookAtFactor;
        }

        // Smooth the offset to be applied
        lookAtOffset = Vector3.Lerp(lookAtOffset, offsetDirection, Time.deltaTime * lookAtSpeed);
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
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredSize, Time.deltaTime * cameraSmoothSpeed);
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

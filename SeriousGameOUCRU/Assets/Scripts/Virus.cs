using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Movement")]
    public float moveForce = 200f;
    public float moveChangeAngle = 1f;
    public float maxVelocity = 10f;


    /*** PRIVATE VARIABLES ***/

    // Component
    private Rigidbody rb;

    // Movement
    private Vector3 moveDirection;
    private float currentAngle;
    private Quaternion currentRotation;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Init rigidbody
        rb = GetComponent<Rigidbody>();

        // Init starting direction
        currentAngle = 0f;
        currentRotation = new Quaternion(0f, 0f, 0f, 1f);
        moveDirection = Vector3.zero;
    }


    private void FixedUpdate()
    {
        MoveVirus();
    }


    /***** MOVEMENTS FUNCTIONS *****/

    private void MoveVirus()
    {
        SlightlyChangeDirection();

        // Add force to move the virus
        rb.AddForce(moveDirection * moveForce, ForceMode.Impulse);

        // Clamp the velocity of the virus
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    private void SlightlyChangeDirection()
    {
        // Compute new angle
        AddAngle((float)Random.Range(-1, 2) * moveChangeAngle);

        // Get new rotation from angle
        currentRotation = Quaternion.Euler(0f, currentAngle, 0f);

        // Apply rotation to forward vector to get movement direction
        moveDirection = currentRotation * Vector3.forward;
    }

    private void AddAngle(float amount)
    {
        // Add angle amount
        currentAngle += amount;

        // Clamp angle between 0 and 360
        if (currentAngle > 360f || currentAngle < 0)
            currentAngle = 360f - currentAngle;
    }


    /***** COLLISION FUNCTIONS *****/

    private void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag("Player") && !c.gameObject.CompareTag("Projectile"))
        {
            // Reserve angle to go in opposition direction when hitting any non-player object
            AddAngle(180f);
        }
    }
}

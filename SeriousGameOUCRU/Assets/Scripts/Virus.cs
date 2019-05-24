using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Movement")]
    public float moveForce = 50f;
    public float moveChangeAngle = 12f;
    public float maxVelocity = 8f;

    [Header("Disolve")]
    public float disolveSpeed = 0.1f;


    /*** PRIVATE VARIABLES ***/

    // Component
    private Rigidbody rb;

    // Movement
    private Vector3 moveDirection;
    private float currentAngle;
    private Quaternion currentRotation;

    // Symptoms
    private int nbSymptomsLeft = 2;

    // Death
    private bool dead = false;
    private Renderer render;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Init components
        rb = GetComponent<Rigidbody>();
        render = transform.GetChild(0).GetComponent<Renderer>();

        // Init starting direction
        currentAngle = 0f;
        currentRotation = new Quaternion(0f, 0f, 0f, 1f);
        moveDirection = Vector3.zero;
    }

    private void Update()
    {
        if (dead)
        {
            // Animate disolve of the virus
            AnimateDeath();
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            // Move virus while it's not dead
            MoveVirus();
        }
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
        if (currentAngle >= 360f)
            currentAngle = 0f;
        else if (currentAngle < 0)
            currentAngle = 360f;
    }


    /***** COLLISION FUNCTIONS *****/

    private void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag("NotTargetable"))
        {
            // Reserve angle to go in opposition direction when hitting any non-player object
            AddAngle(180f);
        }
    }


    /***** DEATH FUNCTIONS *****/

    // Called by a symptom on it's death
    public void NotifySymptomDeath()
    {
        nbSymptomsLeft--;

        if (nbSymptomsLeft == 0)
        {
            // Trigger the death of the virus
            KillVirus();
        }
    }

    // Trigger the virus death procedure
    private void KillVirus()
    {
        dead = true;
        //rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
    }

    // Disolve the cell according to deltaTime
    protected virtual void AnimateDeath()
    {
        //Compute new disolve value
        float newDisolveValue = Mathf.MoveTowards(render.material.GetFloat("_DisolveValue"), 1f, disolveSpeed * Time.deltaTime);

        // Animate the disolve of the virus
        render.material.SetFloat("_DisolveValue", newDisolveValue);

        // Animate the color of the virus as the opposite
        render.material.SetFloat("_LerpValue", 1 - newDisolveValue);

        if (newDisolveValue == 1f)
        {
            // GameObject is destroyed after disolve
            Destroy(gameObject);
        }
    }

}

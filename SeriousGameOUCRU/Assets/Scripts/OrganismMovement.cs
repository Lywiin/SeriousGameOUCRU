using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismMovement : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Components")]
    public Rigidbody2D rb;

    [Header("Movement")]
    public float moveForce = 200f;
    public float moveRate = 2f;
    public float moveRateVariance = 1f;
    public float maxVelocity = 8f;

    [Header("Random Movement")]
    public bool randomMovement = false;

    [Header("Constant Movement")]
    public float angleToRotate = 1f;


    /*** PRIVATE VARIABLES ***/

    // Components
    private Organism selfOrganism;
    private Organism targetOrganism;

    // Movement
    private Vector2 moveDirection;
    private bool canMove = true;

    // Random movement
    private float timeToMove = 0f;
    private float randomMoveRate;

    // Constant movement
    private float currentAngle;
    

    
    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Start()
    {
        selfOrganism = GetComponent<Organism>();
    }

    void FixedUpdate()
    {
        if (!GameController.Instance.IsGamePaused() && canMove)
        {
            if (targetOrganism) 
                MoveTowardstargetOrganism();
            else if (randomMovement)
                MoveRandom();
            else 
                MoveConstant();
        }
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        timeToMove = 0f;
        canMove = true;

        moveDirection = Vector2.up;
        currentAngle = Random.Range(0, 360);
    }


    /***** MOVEMENTS FUNCTIONS *****/

    private void MoveOrganism(bool relative)
    {
        if (relative)
            rb.AddRelativeForce(moveDirection * moveForce, ForceMode2D.Impulse);
        else
            rb.AddForce(moveDirection * moveForce, ForceMode2D.Impulse);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    private void RotateOrganism()
    {
        AddAngle(Random.Range(0f, 1f) > 0.5f ? angleToRotate : -angleToRotate);
        rb.MoveRotation(currentAngle);
    }

    public void AddAngle(float angleToAdd)
    {
        // Add angle and clamp it between 0 and 360
        currentAngle = (currentAngle + angleToAdd) % 360;
        if (currentAngle < 0) currentAngle += 360;
    }


    /***** TARGET MOVEMENTS FUNCTIONS *****/

    // Move organism towards targetOrganism
    private void MoveTowardstargetOrganism()
    {
        moveDirection = targetOrganism.transform.position - transform.position;

        // Stick to target but doesn't push it
        if (moveDirection.magnitude > (selfOrganism.GetOrganismSize() / 2f + targetOrganism.GetOrganismSize() / 2f) + 0.1f)
        {
            // Normalize direction before applying force to keep force constant
            moveDirection.Normalize();

            MoveOrganism(false);
        }
    }


    /***** RANDOM MOVEMENTS FUNCTIONS *****/

    private void MoveRandom()
    {
        // Move only every timeToMove seconds
        if (Time.time >= timeToMove)
        {
            // Computer next time cell should move
            randomMoveRate = Random.Range(moveRate - moveRateVariance, moveRate + moveRateVariance);
            timeToMove = Time.time + 1 / randomMoveRate;

            // Compute a moveDirection and move
            moveDirection.x = Random.Range(-1f, 1f);
            moveDirection.y = Random.Range(-1f, 1f);
            MoveOrganism(false);
        }
    }


    /***** CONSTANT MOVEMENTS FUNCTIONS *****/

    // Move organism normally across the level
    private void MoveConstant()
    {
        RotateOrganism();

        MoveOrganism(true);
    }

    
    /***** GETTERS/SETTERS *****/

    public void SetCanMove(bool b)
    {
        canMove = b;
    }

    public void SetTarget(Organism newTarget)
    {
        targetOrganism = newTarget;
    }
}

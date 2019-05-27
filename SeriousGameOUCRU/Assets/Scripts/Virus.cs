﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Organism
{
    /*** PUBLIC VARIABLES ***/

    [Header("Movement")]
    public float moveForce = 60f;
    public float moveChangeAngle = 12f;
    public float maxVelocity = 8f;

    [Header("Infection")]
    public float infectionTime = 10f;
    public float infectionRecallTime = 5f;


    /*** PRIVATE VARIABLES ***/

    // Movement
    private Vector3 moveDirection;
    private float currentAngle;
    private Quaternion currentRotation;

    // Death
    private bool dead = false;

    // Attack
    private GameObject target;
    private bool canInfect = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        // Init starting direction
        currentAngle = 0f;
        currentRotation = new Quaternion(0f, 0f, 0f, 1f);
        moveDirection = Vector3.zero;

        // Cannot infect at spawn
        StartCoroutine(InfectionRecall());
    }

    private void FixedUpdate()
    {
        // Move virus while do not disolve so it's not dead
        if (!disolve)
        {
            // If virus has a target move towards it, otherwise move normally
            if (target)
            {
                MoveVirusTowardsCell();
            }else
            {
                MoveVirus();
            }
        }
    }


    /***** MOVEMENTS FUNCTIONS *****/

    // Move virus towards targeted cell
    private void MoveVirusTowardsCell()
    {
        // Compute direction toward the cell
        moveDirection = target.transform.position - transform.position;

        // Keep a small distance to stick to the cell without pushing it
        if (moveDirection.magnitude > GetComponentInChildren<Renderer>().bounds.size.x + 0.1f)
        {
            // Normalize direction before applying force to keep force constant
            moveDirection.Normalize();

            // Add force to move
            rb.AddForce(moveDirection * moveForce, ForceMode.Impulse);
        }
    }

    // Move virus normally across the level
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
        if (c.gameObject.GetComponent<BacteriaCell>() || c.gameObject.CompareTag("Level"))
        {
            // Reserve angle to go in opposition direction when hitting  a bacteria
            AddAngle(180f);
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        // If doesn't have a target, detect a human cell and can infect the cell
        if (!target && c.GetComponent<HumanCell>() && canInfect)
        {
            // Set new target
            target = c.gameObject;

            StartCoroutine(StartInfection());
        }
    }


    /***** INFECTION FUNCTIONS *****/

    private IEnumerator StartInfection()
    {
        canInfect = false;
        
        yield return new WaitForSeconds(infectionTime);

        // Check if target is still active after wait time
        if (target)
        {
            // Get rid of cell after infection time
            target.GetComponent<HumanCell>().KillOrganism();

            // Instantiate a new virus instead of cell
            Instantiate(gameObject, target.transform.position, Quaternion.identity);

            // Start recall to prevent chain infection
            StartCoroutine(InfectionRecall());

            // Reset target for next infection
            target = null;
        }
    }

    private IEnumerator InfectionRecall()
    {
        canInfect = false;
        yield return new WaitForSeconds(infectionRecallTime);
        canInfect = true;
    }

}

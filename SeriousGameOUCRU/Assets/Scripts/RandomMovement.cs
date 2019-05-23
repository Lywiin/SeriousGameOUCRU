using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Components")]
    public Rigidbody rb;

    [Header("Movement")]
    public float moveForce = 200f;
    public float moveRate = 2f;
    public float moveRateVariance = 1f;


    /*** PRIVATE VARIABLES ***/

    // Movement
    private float timeToMove = 0f;
    private float randomMoveRate;
    private bool canMove = true;

    
    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void FixedUpdate()
    {
        if (!GameController.Instance.IsGamePaused() && canMove)
        {
            // Attempt to move every frame
            TryToMove();
        }
    }


    /***** MOVEMENTS FUNCTIONS *****/

    private void TryToMove()
    {
        // Randomly moves the bacteria across the level
        if (Time.time >= timeToMove)
        {
            // Computer next time bacteria should move
            randomMoveRate = Random.Range(moveRate - moveRateVariance, moveRate + moveRateVariance);
            timeToMove = Time.time + 1 / randomMoveRate;

            // Add force to the current bacteria velocity
            rb.AddForce(new Vector3(Random.Range(-moveForce, moveForce), 0f, Random.Range(-moveForce, moveForce)), ForceMode.Impulse);
        }
    }

    public void SetCanMove(bool b)
    {
        canMove = b;
    }
}

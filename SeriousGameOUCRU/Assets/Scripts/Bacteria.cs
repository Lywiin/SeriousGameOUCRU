using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Bacteria : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/
    
    [Header("Movement")]
    public float moveForce = 200;
    public float moveAwayForce = 20;
    public float moveRate = 2f;
    public float moveRateVariance = 1f;

    [Header("Health")]
    public int maxHealth = 100;
    public Color fullHealthColor;
    public Color lowHealthColor;

    [Header("Replication")]
    public float mutationProba = 0.01f;
    public float duplicationProba = 0.02f;
    public float duplicationRecallTime = 5f;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;
    protected GameController gameController;

    // Movement
    protected float timeToMove = 0f;
    protected float randomMoveRate;

    // Health
    protected int health;

    // Replication
    protected bool canDuplicate = false;

    // Resistance
    protected bool isResistant = false;

    // Size
    protected float bacteriaSize;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected virtual void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        gameController = GameController.Instance;

        // Initialize Health
        health = maxHealth;
        UpdateHealthColor();

        // Initialize bacteria size
        bacteriaSize = transform.localScale.x;

        // To avoid duplication on spawn
        StartCoroutine(DuplicationRecall());
    }

    protected virtual void Update()
    {
        // Check is game is not currently paused
        if (!gameController.IsGamePaused())
        {
            // Attempt to move bacteria every frame
            TryToMoveBacteria();

            // Attempt to duplicate bacteria every frame
            TryToDuplicateBacteria();
        }
    }


    /***** MOVEMENTS FUNCTIONS *****/

        private void TryToMoveBacteria()
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


    /***** DUPLICATION FUNCTIONS *****/

    private void TryToDuplicateBacteria()
    {
        // If duplication is triggered
        if (canDuplicate && Random.Range(0f, 1f) < duplicationProba)
        {
            // Buffer to prevent quick duplication
            StartCoroutine(DuplicationRecall());

            // Spawn new bacteria
            SpawnDuplicatedBacteria();
        }
    }

    // Buffer to prevent duplication for a short time
    public IEnumerator DuplicationRecall()
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duplicationRecallTime); // Time to wait before it can duplicate again
        canDuplicate = true;
    }
    
    //Spawn a new bacteria around the current one
    private void SpawnDuplicatedBacteria()
    {
        //Check if there is no object at position before spawing, if yes find a new position
        Vector3 randomPos = new Vector3();
        int nbTry = 0;
        while (nbTry < 5) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider[] hitColliders = TestPosition(randomPos);

            // If touch something doesn't duplicate (avoid bacteria spawning on top of each other)
            if (hitColliders.Length > 0)
            {
                continue;
            }

            InstantiateBacteria(randomPos);
            break;
        }
    }

    //Compute a random spawn position around bacteria
    protected virtual Vector3 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);
        return transform.position + newTrans.forward * bacteriaSize * 1.5f; // Add a little gap with *1.5f
    }

    // Test an overlap at position with size of the bacteria
    protected virtual Collider[] TestPosition(Vector3 randomPos)
    {
        return Physics.OverlapSphere(randomPos, bacteriaSize / 2 * 1.1f); // Test 1.1 times bigger
    }

    // Instantiate bacteria at given position and add it to gameController list
    protected virtual GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = Instantiate(gameObject, randomPos, Quaternion.identity);
        return b;
    }


    /***** HEALTH FUNCTIONS *****/
    
    // Change color of the material according to health
    protected void UpdateHealthColor()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(lowHealthColor, fullHealthColor, (float)health / maxHealth));
    }

    // Apply damage to bacteria, update color and kill it if needed
    public virtual void DamageBacteria(int dmg)
    {
        //Apply damage to bacteria's health
        health -= dmg;

        //Change material color according to health
        UpdateHealthColor();

        //If health is below 0, the bacteria dies
        if (health <= 0)
        {
            KillBacteria();
        }
    }

    // Called when the bacteria has to die
    public virtual void KillBacteria()
    {
        // GameObject is destroyed in the gamecontroller
        Destroy(gameObject);
    }


    /***** RESISTANCE FUNCTIONS *****/

    public virtual void ActivateResistance()
    {
        isResistant = true;
    }

    public bool IsResistant()
    {
        return isResistant;
    }

}

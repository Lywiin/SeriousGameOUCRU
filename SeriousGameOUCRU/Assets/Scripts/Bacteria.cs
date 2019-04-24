using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float mutationProba = 0.001f;
    public float duplicationProba = 0.0002f;
    public float duplicationRecallTime = 5f;

    [Header("Disolve")]
    public float disolveSpeed = 2f;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;

    // Movement
    protected float timeToMove = 0f;
    protected float randomMoveRate;

    // Health
    protected int health;

    // Replication
    protected bool canDuplicate = false;
    protected float bacteriaBaseSize;

    // Resistance
    protected bool isResistant = false;

    // Size
    protected float bacteriaSize;

    // Disolve
    protected Renderer render;
    protected bool disolve = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected virtual void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();

        // Initialize Health
        health = maxHealth;
        //UpdateHealthColor();

        // Initialize bacteria size
        bacteriaSize = transform.localScale.x;
        bacteriaBaseSize = transform.GetComponent<Collider>().bounds.size.x;

        // To avoid duplication on spawn
        StartCoroutine(DuplicationRecall());
    }

    protected virtual void Update()
    {
        // Check is game is not currently paused
        if (!GameController.Instance.IsGamePaused())
        {
            // Attempt to move bacteria every frame
            TryToMoveBacteria();

            // Attempt to duplicate bacteria every frame
            TryToDuplicateBacteria();

            // Attempt to mutate bacteria every frame
            TryToMutateBacteria();
        }

        // Animate disolve of the bacteria
        if (disolve)
        {
            DisolveOverTime();
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


    /***** MUTATION FUNCTIONS *****/

    protected virtual void TryToMutateBacteria()
    {
        // If bacteria not already resistant and mutation is triggered
        if (!isResistant && Random.Range(0f, 1f) < mutationProba)
        {
            ActivateResistance();
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

        // Compute new spawning position
        Vector3 spawnPos = transform.position + newTrans.forward * bacteriaBaseSize * bacteriaSize * 1.5f;

        // Clamp spawning position inside the game zone
        spawnPos.x = Mathf.Clamp(spawnPos.x, -GameController.Instance.gameZoneRadius.x, GameController.Instance.gameZoneRadius.x);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -GameController.Instance.gameZoneRadius.y, GameController.Instance.gameZoneRadius.y);

        return spawnPos; // Add a little gap with *1.5f
    }

    // Test an overlap at position with size of the bacteria
    protected virtual Collider[] TestPosition(Vector3 randomPos)
    {
        return Physics.OverlapSphere(randomPos, bacteriaBaseSize * bacteriaSize / 2 * 1.1f); // Test 1.1 times bigger
    }

    // Instantiate bacteria at given position and add it to gameController list
    protected virtual GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = Instantiate(gameObject, randomPos, Quaternion.identity);

        // Activate the resistance on duplicated bacteria is already resistant
        // if(isResistant)
        //     b.GetComponent<Bacteria>().ActivateResistance();

        return b;
    }


    /***** HEALTH FUNCTIONS *****/
    
    // Change color of the material according to health
    protected void UpdateHealthColor()
    {
        render.material.SetFloat("_LerpValue", (float)health / 100f);
        //GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(lowHealthColor, fullHealthColor, (float)health / maxHealth));
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
        // Trigger disolve anim in update
        disolve = true;

        // Prevent colliding again during animation
        GetComponent<Collider>().enabled = false;
        rb.Sleep();
    }

    // Disolve the bacteria according to deltaTime
    protected virtual float DisolveOverTime()
    {
        //Compute new value
        float newDisolveValue = Mathf.MoveTowards(render.material.GetFloat("_DisolveValue"), 1f, disolveSpeed * Time.deltaTime);

        // Animate the disolve of the bacteria
        render.material.SetFloat("_DisolveValue", newDisolveValue);

        if (newDisolveValue == 1f)
        {
            // GameObject is destroyed after disolve
            Destroy(gameObject);
        }
        
        return newDisolveValue;
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

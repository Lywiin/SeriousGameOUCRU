using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/
    [Header("Prefab")]
    public GameObject cellPrefab;

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
    public ParticleSystem explosionParticle;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;
    protected Renderer render;
    protected Collider coll;

    // Health
    protected int health;

    // Replication
    protected bool canDuplicate = false;

    // Resistance
    protected bool isResistant = false;

    // Size
    protected float cellSize;

    // Disolve
    protected bool disolve = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/
    
    protected virtual void Awake()
    {
        InitComponents();

        // Initialize Health
        health = maxHealth;

        // Initialize cell size
        cellSize = render.bounds.size.x;
    }

    protected virtual void Start()
    {
        // To avoid duplication on spawn
        StartCoroutine(DuplicationRecall());
    }

    protected virtual void InitComponents()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        render = GetComponentInChildren<Renderer>();
        coll = GetComponent<Collider>();
    }

    protected virtual void Update()
    {
        // Check is game is not currently paused
        if (!GameController.Instance.IsGamePaused() && !disolve)
        {
            // Attempt to duplicate cell every frame
            TryToDuplicateCell();

            // Attempt to mutate cell every frame
            TryToMutateCell();
        }

        // Animate disolve of the cell
        if (disolve)
        {
            DisolveOverTime();
        }
    }


    /***** MUTATION FUNCTIONS *****/

    protected virtual void TryToMutateCell()
    {
        // If cell not already resistant and mutation is triggered
        if (!isResistant && Random.Range(0f, 1f) < mutationProba)
        {
            ActivateResistance();
        }
    }


    /***** DUPLICATION FUNCTIONS *****/

    private void TryToDuplicateCell()
    {
        // If duplication is triggered
        if (canDuplicate && Random.Range(0f, 1f) < duplicationProba)
        {
            // Buffer to prevent quick duplication
            StartCoroutine(DuplicationRecall());

            // Spawn new cell
            SpawnDuplicatedCell();
        }
    }

    // Buffer to prevent duplication for a short time
    public IEnumerator DuplicationRecall()
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duplicationRecallTime); // Time to wait before it can duplicate again
        canDuplicate = true;
    }
    
    //Spawn a new cell around the current one and return a bool if did so
    private bool SpawnDuplicatedCell()
    {
        //Check if there is no object at position before spawing, if yes find a new position
        Vector3 randomPos = new Vector3();
        int nbTry = 0;
        while (nbTry < 5) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider[] hitColliders = TestPosition(randomPos);

            // If touch something doesn't duplicate (avoid cell spawning on top of each other)
            if (hitColliders.Length > 0)
            {
                continue;
            }

            InstantiateCell(randomPos);
            return true;
        }
        return false;
    }

    //Compute a random spawn position around cell
    protected virtual Vector3 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);

        // Compute new spawning position
        Vector3 spawnPos = transform.position + newTrans.forward * cellSize * 1.5f;

        // Clamp spawning position inside the game zone
        spawnPos.x = Mathf.Clamp(spawnPos.x, -GameController.Instance.gameZoneRadius.x, GameController.Instance.gameZoneRadius.x);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -GameController.Instance.gameZoneRadius.y, GameController.Instance.gameZoneRadius.y);

        return spawnPos;
    }

    // Test an overlap at position with size of the cell
    protected virtual Collider[] TestPosition(Vector3 randomPos)
    {
        // Test a sphere slightly bigger to keep some space between bacteria
        // Only testing on Ennemy layer mask and ignoring trigger
        return Physics.OverlapSphere(randomPos, cellSize / 2 * 1.1f, 1 << LayerMask.NameToLayer("Ennemy"), QueryTriggerInteraction.Ignore);
    }

    // Instantiate cell at given position and add it to gameController list
    protected virtual GameObject InstantiateCell(Vector3 randomPos)
    {
        // Instantiate new cell
        GameObject b = Instantiate(cellPrefab, randomPos, Quaternion.identity);

        // Get the reference to the cell script
        Cell duplicatedCell = b.GetComponent<Cell>();

        // Set the health of the new becteria to the health of the parent one
        duplicatedCell.SetCellHealth(health);

        if (isResistant)
        {
            // If parent is resistant, transmit resistance to the child
            duplicatedCell.ActivateResistance();
        }

        return b;
    }


    /***** HEALTH FUNCTIONS *****/
    
    // Set the health of the cell to a new value
    public void SetCellHealth(int newHealth)
    {
        health = newHealth;
        UpdateHealthColor();
    }

    // Change color of the material according to health
    protected void UpdateHealthColor()
    {
        render.material.SetFloat("_LerpValue", (float)health / maxHealth);
    }

    // Apply damage to cell, update color and kill it if needed
    public virtual void DamageCell(int dmg)
    {
        //Apply damage to cell's health
        health -= dmg;

        //Change material color according to health
        UpdateHealthColor();

        //If health is below 0, the cell dies
        if (health <= 0)
        {
            KillCell();
        }
    }

    // Called when the cell has to die
    public virtual void KillCell()
    {
        // Trigger particle effect
        explosionParticle.gameObject.SetActive(true);
        explosionParticle.Play();

        // Trigger disolve anim in update
        disolve = true;

        // Prevent colliding again during animation
        coll.enabled = false;
        rb.Sleep();
    }

    // Disolve the cell according to deltaTime
    protected virtual void DisolveOverTime()
    {
        //Compute new value
        float newDisolveValue = Mathf.MoveTowards(render.material.GetFloat("_DisolveValue"), 1f, disolveSpeed * Time.deltaTime);

        // Animate the disolve of the cell
        render.material.SetFloat("_DisolveValue", newDisolveValue);

        if (newDisolveValue == 1f)
        {
            // GameObject is destroyed after disolve
            Destroy(gameObject);
        }
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

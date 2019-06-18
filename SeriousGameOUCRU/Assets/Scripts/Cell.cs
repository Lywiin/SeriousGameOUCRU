using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell : Organism
{
    /*** PUBLIC VARIABLES ***/

    [Header("Replication")]
    public float mutationProba = 0.0005f;
    public float duplicationProba = 0.0002f;
    public float duplicationRecallTime = 1f;

    [Header("Particles")]
    public ParticleSystem explosionParticle;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Replication
    protected bool canDuplicate = false;

    // Resistance
    protected bool isResistant = false;

    // Size
    protected float cellSize;
    protected float baseCellSize;


    /***** MONOBEHAVIOUR FUNCTIONS *****/
    
    protected override void Awake()
    {
        base.Awake();

        // Initialize base cell size
        baseCellSize = render.bounds.size.x;
    }

    public override void OnObjectToSpawn()
    {
        explosionParticle.gameObject.SetActive(false);
        isResistant = false;

        StartCoroutine(DuplicationRecall());

        base.OnObjectToSpawn();
    }

    protected override void Update()
    {
        base.Update();
        
        // Check is game is not currently paused
        if (!gameController.IsGamePaused() && !disolve)
        {
            // Attempt to duplicate cell every frame
            TryToDuplicateCell();

            // Attempt to mutate cell every frame
            TryToMutateCell();
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
        Vector2 randomPos = new Vector2();
        int nbTry = 0;
        while (nbTry < 5) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider2D[] hitColliders = TestPosition(randomPos);

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
    protected virtual Vector2 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, 0.0f, Random.Range(0f, 360f)), Space.World);

        // Compute new spawning position
        Vector3 spawnPos = transform.position + newTrans.right * cellSize * 1.2f;

        // Clamp spawning position inside the game zone
        spawnPos.x = Mathf.Clamp(spawnPos.x, -gameController.gameZoneRadius.x, gameController.gameZoneRadius.x);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -gameController.gameZoneRadius.y, gameController.gameZoneRadius.y);

        return spawnPos;
    }

    // Test an overlap at position with size of the cell
    protected virtual Collider2D[] TestPosition(Vector2 randomPos)
    {
        // Test a sphere slightly bigger to keep some space between bacteria
        // Testing on everything except first layer
        return Physics2D.OverlapCircleAll(randomPos, cellSize / 2 * 1.2f, ~(1 << 1));
    }

    // Instantiate cell at given position and add it to gameController list
    protected abstract GameObject InstantiateCell(Vector2 randomPos);

    public override void ResetOrganismAtPosition(Vector2 position)
    {
        base.ResetOrganismAtPosition(position);

        if (isResistant)
        {
            // If parent is resistant, transmit resistance to the child
            ActivateResistance();
        }
    }


    /***** HEALTH FUNCTIONS *****/

    // Called when the cell has to die
    public override void KillOrganism()
    {
        base.KillOrganism();

        // Trigger particle effect
        explosionParticle.gameObject.SetActive(true);
        explosionParticle.Play();
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

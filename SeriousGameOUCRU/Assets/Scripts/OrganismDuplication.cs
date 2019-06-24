using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismDuplication : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Duplication")]
    public float duplicationProba = 0.0002f;
    public float duplicationRecallTime = 1f;


    /*** PRIVATE VARIABLES ***/

    // Components
    private Organism selfOrganism;
    private GameController gameController;

    protected bool canDuplicate = false;

    // Cached
    Vector2 spawnPos;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        selfOrganism = GetComponent<Organism>();
        spawnPos = Vector2.zero;
    }

    private void Start()
    {
        gameController = GameController.Instance;
    }

    protected void Update()
    {        
        // Check is game is not currently paused
        if (!gameController.IsGamePaused() && !selfOrganism.IsFading())
        {
            // Attempt to duplicate organism every frame
            TryToDuplicateOrganism();
        }
    }

    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        StartCoroutine(DuplicationRecall());
    }


    /***** DUPLICATION FUNCTIONS *****/

    // Buffer to prevent duplication for a short time
    public IEnumerator DuplicationRecall()
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duplicationRecallTime); // Time to wait before it can duplicate again
        canDuplicate = true;
    }
    
    private void TryToDuplicateOrganism()
    {
        // If duplication is triggered
        if (canDuplicate && Random.Range(0f, 1f) < duplicationProba)
        {
            // Buffer to prevent quick duplication
            StartCoroutine(DuplicationRecall());

            // Spawn new organism
            SpawnDuplicatedOrganism();
        }
    }

    //Spawn a new organism around the current one and return a bool if did so
    private bool SpawnDuplicatedOrganism()
    {
        //Check if there is no object at position before spawing, if yes find a new position
        Vector2 randomPos = new Vector2();
        int nbTry = 0;
        while (nbTry < 3) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider2D[] hitColliders = TestPosition(randomPos);

            // If touch something doesn't duplicate (avoid organism spawning on top of each other)
            if (hitColliders.Length > 0)
            {
                continue;
            }

            Organism spawnedOrganism = selfOrganism.InstantiateOrganism(randomPos);

            // Copy shield health if organism has shield
            if (spawnedOrganism && spawnedOrganism.GetOrgMutation()) 
                spawnedOrganism.GetOrgMutation().SetShieldHealth(selfOrganism.GetOrgMutation().GetShieldHealth());
            
            return true;
        }
        return false;
    }
    
    //Compute a random spawn position around organism
    protected virtual Vector2 ComputeRandomSpawnPosAround()
    {
        spawnPos = Random.insideUnitCircle.normalized;
        spawnPos *= selfOrganism.GetOrganismSize() + 1f;
        spawnPos += (Vector2)transform.position;

        return spawnPos;
    }

    // Test an overlap at position with size of the organism
    protected virtual Collider2D[] TestPosition(Vector2 randomPos)
    {
        // Test a sphere slightly bigger to keep some space between bacteria
        // Testing on everything except first layer
        return Physics2D.OverlapCircleAll(randomPos, selfOrganism.GetOrganismSize() / 2 + 0.5f, ~(1 << 1));
    }

}

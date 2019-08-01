using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismDuplication : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Duplication")]
    public float minDuplicationProba = 0.0001f;
    public float maxDuplicationProba = 0.005f;


    /*** PRIVATE VARIABLES ***/

    // Components
    private Organism selfOrganism;
    private GameController gameController;

    [HideInInspector] public bool canDuplicate = false;

    private float duplicationProbaIncreaseRate;
    private float duplicationRecallTime;
    private int duplicationSoftCap;

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
        if (canDuplicate && !gameController.IsGamePaused() && !selfOrganism.IsFading())
        {
            // Attempt to duplicate organism every frame
            TryToDuplicateOrganism();
        }
    }

    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        duplicationProbaIncreaseRate = (maxDuplicationProba - minDuplicationProba) / (duplicationSoftCap - 1);
        StartCoroutine(DuplicationRecall(Random.Range(duplicationRecallTime / 2, duplicationRecallTime)));
    }


    /***** DUPLICATION FUNCTIONS *****/

    public void SetDuplicationRecallTime(float duration)
    {
        duplicationRecallTime = duration;
    }

    public void SetDuplicationSoftCap(int softCap)
    {
        duplicationSoftCap = softCap;
    }

    // Buffer to prevent duplication for a short time
    public IEnumerator DuplicationRecall(float duration)
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duration); // Time to wait before it can duplicate again
        canDuplicate = true;
    }
    
    private void TryToDuplicateOrganism()
    {
        // Compute duplication proba depending on number of current similar organism
        float currentDuplicationProba = maxDuplicationProba - duplicationProbaIncreaseRate * Mathf.Min(selfOrganism.GetListCount() - 1, duplicationSoftCap - 1);

        // If duplication is triggered
        if (canDuplicate && Random.Range(0f, 1f) < currentDuplicationProba)
        {
            // Buffer to prevent quick duplication
            StartCoroutine(DuplicationRecall(duplicationRecallTime));

            // Spawn new organism
            SpawnDuplicatedOrganism();
        }
    }

    // Stop all organism from duplicating
    public static void StopDuplication()
    {
        foreach (BacteriaCell b in BacteriaCell.bacteriaCellList)
            b.GetOrgDuplication().canDuplicate = false;
        foreach (HumanCell h in HumanCell.humanCellList)
            h.GetOrgDuplication().canDuplicate = false;
    }


    /***** SPAWN DUPLICATE FUNCTIONS *****/

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

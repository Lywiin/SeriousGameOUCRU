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


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Start()
    {
        selfOrganism = GetComponent<Organism>();
        gameController = GameController.Instance;
    }

    protected void Update()
    {        
        // Check is game is not currently paused
        if (!gameController.IsGamePaused() && !selfOrganism.IsDisolving())
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
        while (nbTry < 5) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider2D[] hitColliders = TestPosition(randomPos);

            // If touch something doesn't duplicate (avoid organism spawning on top of each other)
            if (hitColliders.Length > 0)
            {
                continue;
            }

            selfOrganism.InstantiateOrganism(randomPos);
            return true;
        }
        return false;
    }

    //Compute a random spawn position around organism
    protected virtual Vector2 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, 0.0f, Random.Range(0f, 360f)), Space.World);

        // Compute new spawning position
        Vector3 spawnPos = transform.position + newTrans.right * selfOrganism.GetOrganismSize() * 1.2f;

        // Clamp spawning position inside the game zone
        spawnPos.x = Mathf.Clamp(spawnPos.x, -gameController.gameZoneRadius.x, gameController.gameZoneRadius.x);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -gameController.gameZoneRadius.y, gameController.gameZoneRadius.y);

        return spawnPos;
    }

    // Test an overlap at position with size of the organism
    protected virtual Collider2D[] TestPosition(Vector2 randomPos)
    {
        // Test a sphere slightly bigger to keep some space between bacteria
        // Testing on everything except first layer
        return Physics2D.OverlapCircleAll(randomPos, selfOrganism.GetOrganismSize() / 2 * 1.2f, ~(1 << 1));
    }

}

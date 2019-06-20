using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Organism
{
    /*** PUBLIC VARIABLES ***/

    public static List<Virus> virusList = new List<Virus>();


    /*** PRIVATE VARIABLES ***/

    private GenericObjectPool<Virus> virusPool;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Start()
    {
        base.Start();
        
        virusPool = VirusPool.Instance;
    }


    /***** POOL FUNCTIONS *****/

    public override void OnObjectToSpawn()
    {
        base.OnObjectToSpawn();

        // Add to list
        virusList.Add(this);
    }


    /***** HEALTH FUNCTIONS *****/

    public override void KillOrganism()
    {
        // Prevent player to keep targeting virus
        playerController.ResetTarget();

        // Remove from list
        RemoveFromList();

        base.KillOrganism();
    }

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        virusPool.ReturnToPool(this);
    }


    /***** COLLISION FUNCTIONS *****/

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.GetComponentInParent<BacteriaCell>() || c.gameObject.GetComponentInParent<Virus>() || c.gameObject.CompareTag("Level"))
        {
            // Reserve angle to go in opposition direction when hitting  a bacteria
            orgMovement.AddAngle(180f);
        }
    }


    /***** LIST FUNCTIONS *****/
    
    private void RemoveFromList()
    {
        virusList.Remove(this);

        if (BacteriaCell.bacteriaCellList.Count == 0 && Virus.virusList.Count == 0)
        {
            gameController.PlayerWon();
        }
    }

    public override GameObject InstantiateOrganism(Vector2 spawnPosition)
    {
        Virus virusToSpawn = virusPool.Get();
        virusToSpawn.ResetOrganismAtPosition(spawnPosition);
        virusToSpawn.OnObjectToSpawn();

        return virusToSpawn.gameObject;
    }
}

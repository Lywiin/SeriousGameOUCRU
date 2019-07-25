using System.Collections.Generic;
using UnityEngine;

public class BacteriaCell : Organism
{
    /*** PUBLIC VARIABLES ***/

    public static List<BacteriaCell> bacteriaCellList = new List<BacteriaCell>();


    /*** PRIVATE/PROTECTED VARIABLES ***/

    private static GenericObjectPool<BacteriaCell> bacteriaCellPool;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Start()
    {
        base.Start();
        
        bacteriaCellPool = BacteriaCellPool.Instance;
    }


    /***** POOL FUNCTIONS *****/

    protected override void OnObjectSpawn()
    {
        base.OnObjectSpawn();

        bacteriaCellList.Add(this);
        if (uiController) uiController.UpdateBacteriaCellCount();
    }

    public override Organism InstantiateOrganism(Vector2 spawnPosition)
    {
        return BacteriaCell.InstantiateBacteriaCell(spawnPosition);
    }

    public static Organism InstantiateBacteriaCell(Vector2 spawnPosition)
    {
        BacteriaCell bacteriaCellToSpawn = BacteriaCellPool.Instance.Get();
        bacteriaCellToSpawn.ResetOrganismAtPosition(spawnPosition);
        bacteriaCellToSpawn.OnObjectToSpawn();

        return bacteriaCellToSpawn;
    }


    /***** DEATH FUNCTIONS *****/

    // Apply damage to cell if shield health is at 0, otherwise damage the shield
    public override void DamageOrganism(int dmg)
    {
        if (orgMutation.GetShieldHealth() == 0)
        {
            base.DamageOrganism(dmg);
        }else
        {
            orgMutation.DamageShield(dmg);
        }
    }

    // Called when the cell has to die
    public override void KillOrganism()
    {
        if (render.isVisible)
            AudioManager.Instance.Play("BacteriaCellDeath");

        // Stop moving
        orgMovement.SetCanMove(false);

        // Increase killed count
        gameController.IncrementBacteriaCellKillCount();

        // Remove from list
        RemoveFromList();

        base.KillOrganism();
    }

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        bacteriaCellPool.ReturnToPool(this);
    }


    /***** LIST FUNCTIONS *****/
    
    protected override void RemoveFromList()
    {
        bacteriaCellList.Remove(this);
        uiController.UpdateBacteriaCellCount();

        if (BacteriaCell.bacteriaCellList.Count == 0 && Virus.virusList.Count == 0)
        {
            gameController.PlayerWon();
        }
    }
}

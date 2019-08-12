using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class HumanCell : Organism
{
    /*** PUBLIC VARIABLES ***/

    public static List<HumanCell> humanCellList = new List<HumanCell>();

    [HideInInspector] public OrganismAttack targetedBy;


    /*** PRIVATE VARIABLES ***/

    private static GenericObjectPool<HumanCell> humanCellPool;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Start()
    {
        base.Start();
        
        humanCellPool = HumanCellPool.Instance;
    }


    /***** POOL FUNCTIONS *****/

    protected override void OnObjectSpawn()
    {
        if (orgDuplication) 
        {
            orgDuplication.SetDuplicationRecallTime(gameController.duplicationRecallTimeHumanCell);
            orgDuplication.SetDuplicationSoftCap(gameController.duplicationSoftCapHumanCell);
        }

        base.OnObjectSpawn();

        humanCellList.Add(this);
        if (uiController) uiController.UpdateHumanCellCount();
    }

    public override Organism InstantiateOrganism(Vector2 spawnPosition)
    {
        return InstantiateHumanCell(spawnPosition);
    }

    public static Organism InstantiateHumanCell(Vector2 spawnPosition)
    {
        HumanCell humanCellToSpawn = HumanCellPool.Instance.Get();
        humanCellToSpawn.ResetOrganismAtPosition(spawnPosition);
        humanCellToSpawn.OnObjectToSpawn();

        return humanCellToSpawn;
    }


    /***** DEATH FUNCTIONS *****/

    // Called when the cell has to die
    public override void KillOrganism()
    {
        // Reset attacker if one
        if (targetedBy) targetedBy.ResetTarget();

        if (render.isVisible && AudioManager.Instance)
            AudioManager.Instance.Play("HumanCellDeath");

        // Remove from list
        RemoveFromList();

        // If no human cell left it's game over
        if (humanCellList.Count == 0)
        {
            gameController.GameOver(false);
        }

        base.KillOrganism();
    }

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        humanCellPool.ReturnToPool(this);
    }


    /***** LIST FUNCTIONS *****/
    
    protected override void RemoveFromList()
    {
        humanCellList.Remove(this);
        uiController.UpdateHumanCellCount();
    }

    public override int GetListCount()
    {
        return HumanCell.humanCellList.Count;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCell : Cell
{
    /*** PUBLIC VARIABLES ***/

    public static List<HumanCell> humanCellList = new List<HumanCell>();


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        humanCellList.Add(this);
    }


    /***** HEALTH FUNCTIONS *****/

    // Called when the cell has to die
    public override void KillOrganism()
    {
        // Remove from list
        humanCellList.Remove(this);

        // If no human cell left it's game over
        if (humanCellList.Count == 0)
        {
            GameController.Instance.GameOver();
        }

        base.KillOrganism();
    }


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateCell(Vector3 randomPos)
    {
        GameObject spawnedCell = base.InstantiateCell(randomPos);
        
        // If cell is spawned set her parent to the current cell
        if (spawnedCell)
        {
            spawnedCell.transform.parent = transform.parent;
        }

        return spawnedCell;
    }
}

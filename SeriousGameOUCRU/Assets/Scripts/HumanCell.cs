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

    public override void OnObjectToSpawn()
    {
        base.OnObjectToSpawn();

        humanCellList.Add(this);
        cellSize = baseCellSize;
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

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        HumanCellPool.Instance.ReturnToPool(this);
    }


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateCell(Vector3 randomPos)
    {
        HumanCell humanCellToSpawn = HumanCellPool.Instance.Get();
        humanCellToSpawn.ResetOrganismAtPosition(randomPos);
        humanCellToSpawn.OnObjectToSpawn();

        // Attach the joint to the root rigidbody
        humanCellToSpawn.GetComponent<SpringJoint>().connectedBody = transform.parent.GetComponent<Rigidbody>();    // A OPTIMISER
        humanCellToSpawn.transform.parent = transform.parent;


        return humanCellToSpawn.gameObject;
    }
}

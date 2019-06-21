using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCell : Organism
{
    /*** PUBLIC VARIABLES ***/

    public static List<HumanCell> humanCellList = new List<HumanCell>();

    [HideInInspector]
    public bool isTargeted;


    /*** PRIVATE VARIABLES ***/

    private GenericObjectPool<HumanCell> humanCellPool;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        humanCellList.Add(this);
    }

    protected override void Start()
    {
        base.Start();
        
        humanCellPool = HumanCellPool.Instance;
    }


    /***** POOL FUNCTIONS *****/

    public override void OnObjectToSpawn()
    {
        base.OnObjectToSpawn();

        humanCellList.Add(this);

        isTargeted = false;
    }

    public override GameObject InstantiateOrganism(Vector2 spawnPosition)
    {
        HumanCell humanCellToSpawn = humanCellPool.Get();
        humanCellToSpawn.ResetOrganismAtPosition(spawnPosition);
        humanCellToSpawn.OnObjectToSpawn();

        return humanCellToSpawn.gameObject;
    }


    /***** DEATH FUNCTIONS *****/

    // Called when the cell has to die
    public override void KillOrganism()
    {
        // Remove from list
        humanCellList.Remove(this);
        // UIController.Instance.UpdateBacteriaCellCount(humanCellList.Count);

        // If no human cell left it's game over
        if (humanCellList.Count == 0)
        {
            gameController.GameOver();
        }

        base.KillOrganism();
    }

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        humanCellPool.ReturnToPool(this);
    }
}

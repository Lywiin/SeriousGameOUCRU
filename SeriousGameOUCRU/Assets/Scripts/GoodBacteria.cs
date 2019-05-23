﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodBacteria : Bacteria
{
    /*** PUBLIC VARIABLES ***/

    public static List<GoodBacteria> goodBacteriaList = new List<GoodBacteria>();


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        goodBacteriaList.Add(this);
    }


    /***** HEALTH FUNCTIONS *****/

    // Called when the bacteria has to die
    public override void KillBacteria()
    {
        // Remove from list
        goodBacteriaList.Remove(this);

        // If no good bacteria left it's game over
        if (goodBacteriaList.Count == 0)
        {
            GameController.Instance.GameOver();
        }

        base.KillBacteria();
    }


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject spawnedCell = base.InstantiateBacteria(randomPos);
        
        // If cell is spawned set her parent to the current cell
        if (spawnedCell)
        {
            spawnedCell.transform.parent = transform.parent;
        }

        return spawnedCell;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodBacteria : Bacteria
{
    /*** PUBLIC VARIABLES ***/

    public static List<GoodBacteria> goodBacteriaList = new List<GoodBacteria>();


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Start()
    {
        base.Start();

        goodBacteriaList.Add(this);

    }

    protected override void Update()
    {
        base.Update();
    }


    /***** HEALTH FUNCTIONS *****/

    // Called when the bacteria has to die
    public override void KillBacteria()
    {
        // Remove from list
        goodBacteriaList.Remove(this);

        base.KillBacteria();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    /*** PRIVATE VARIABLES ***/

    private BacteriaCell bacteriaCellScript;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected void Awake()
    {
        // Component init
        bacteriaCellScript = GetComponent<BacteriaCell>();
    }


    // When collide calls parent event
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // // Handle conjugaison of cell
        // bacteriaCellScript.CollisionEvent(collision);

        // HumanCell humanCell = collision.gameObject.GetComponent<HumanCell>();
        // // Kill cell on touch
        // if (humanCell)
        // {
        //     humanCell.KillOrganism();

        //     // Notify parent from cell killed
        //     bacteriaCellScript.UnTargetCell();
        // }
    }
}

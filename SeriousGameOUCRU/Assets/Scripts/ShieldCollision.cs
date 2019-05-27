using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    // When collide calls parent event
    private void OnCollisionEnter(Collision collision)
    {
        // Handle conjugaison of cell
        transform.parent.GetComponent<BacteriaCell>().CollisionEvent(collision);

        // Kill cell on touch
        if (collision.gameObject.GetComponent<HumanCell>())
        {
            collision.gameObject.GetComponent<HumanCell>().KillOrganism();

            // Notify parent from cell killed
            transform.parent.GetComponent<BacteriaCell>().UnTargetCell();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    // When collide calls parent event
    private void OnCollisionEnter(Collision collision)
    {
        // Handle conjugaison of bacteria
        transform.parent.GetComponent<BadBacteria>().CollisionEvent(collision);

        // Kill cell on touch
        if (collision.gameObject.GetComponent<GoodBacteria>())
        {
            collision.gameObject.GetComponent<GoodBacteria>().KillBacteria();
        }
    }
}

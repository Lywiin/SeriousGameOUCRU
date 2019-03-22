using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    // When collide calls parent event
    private void OnCollisionEnter(Collision collision)
    {
        transform.parent.GetComponent<BadBacteria>().CollisionEvent(collision);
    }
}

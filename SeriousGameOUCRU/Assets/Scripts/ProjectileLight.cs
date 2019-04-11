using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLight: Projectile
{
    /***** COLLISION FUNCTIONS *****/

    protected override void OnCollisionEnter(Collision collision)
    {
        // Used to prevent double collision with one projectile
        if (canCollide)
        {
            canCollide = false;

            // Apply damage on the collided object
            ApplyDamage(collision.gameObject);

            Destroy(gameObject);
        }
    }

    protected override void ApplyDamage(GameObject g)
    {
        base.ApplyDamage(g);

        // Add screen shake if touch an ennemy
        if (g.transform.GetComponentInParent<Bacteria>() || g.gameObject.CompareTag("ResistantGene"))
        {
            CameraShake.Instance.LightScreenShake();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLight: Projectile
{
    /***** TRIGGER FUNCTIONS *****/

    private void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag("Player") && !c.gameObject.CompareTag("Projectile"))
        {
            Hide();

            // Apply damage on the collided object
            ApplyDamage(c.gameObject);

            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter(Collider c)
    {
        if (!c.gameObject.CompareTag("Player") && !c.gameObject.CompareTag("Projectile") && !c.gameObject.CompareTag("Level") && !PlayerController.Instance.switchInput)
        {
            // Change target if a bacteria enter the detection collider
            target = c.gameObject;
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

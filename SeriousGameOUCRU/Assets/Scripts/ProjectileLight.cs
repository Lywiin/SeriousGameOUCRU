using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLight: Projectile
{
    /***** TRIGGER FUNCTIONS *****/

    private void OnCollisionEnter2D(Collision2D c)
    {
        Hide();

        // Apply damage on the collided object
        ApplyDamage(c.collider.gameObject);

        Destroy(gameObject);
    }

    protected override void ApplyDamage(GameObject g)
    {
        base.ApplyDamage(g);

        // Add screen shake when touch an ennemy
        CameraShake.Instance.LightScreenShake();
    }
}

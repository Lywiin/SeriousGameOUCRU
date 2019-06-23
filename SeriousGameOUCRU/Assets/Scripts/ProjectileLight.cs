using UnityEngine;

public class ProjectileLight: Projectile
{
    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void FixedUpdate()
    {
        if (!hidden && (!target || target.IsDisolving()))
        {
            KillProjectile();
        }

        base.FixedUpdate();
    }


    /***** POOL FUNCTIONS *****/

    public static Projectile InstantiateProjectileLight(Vector2 spawnPosition, Quaternion spawnRotation, Organism newTarget)
    {
        ProjectileLight projectileLightToSpawn = ProjectileLightPool.Instance.Get();
        projectileLightToSpawn.transform.position = spawnPosition;
        projectileLightToSpawn.transform.rotation = spawnRotation;
        projectileLightToSpawn.gameObject.SetActive(true);

        projectileLightToSpawn.OnObjectToSpawn();
        projectileLightToSpawn.SetTarget(newTarget);

        return projectileLightToSpawn;
    }


    /***** KILL FUNCTIONS *****/

    protected override void KillProjectile()
    {
        base.KillProjectile();

        ProjectileLightPool.Instance.ReturnToPool(this);
    }


    /***** COLLISION FUNCTIONS *****/

    protected override void OnCollisionEnter2D(Collision2D c)
    {
        // Apply damage on the collided object
        if (c.gameObject.CompareTag("Targetable"))
            ApplyDamage(c.collider.GetComponentInParent<Organism>());

        KillProjectile();
    }

    protected override void ApplyDamage(Organism o)
    {
        base.ApplyDamage(o);

        // Add screen shake when touch an ennemy
        CameraShake.Instance.LightScreenShake();
    }
}

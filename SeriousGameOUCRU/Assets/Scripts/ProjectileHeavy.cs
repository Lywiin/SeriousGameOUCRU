using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class ProjectileHeavy : Projectile
{
    /*** PUBLIC VARIABLES ***/

    public float explosionRadius = 30f;
    public ParticleSystem particle;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        ParticleSystem.ShapeModule shapeModule = particle.shape;
        shapeModule.radius = explosionRadius - 5f;
    }


    /***** POOL FUNCTIONS *****/

    public override void OnObjectToSpawn()
    {
        base.OnObjectToSpawn();

        particle.Clear();
        CameraController.Instance.StartFollowProjectile(this);
    }

    public static Projectile InstantiateProjectileHeavy(Vector2 spawnPosition, Quaternion spawnRotation, Organism newTarget)
    {
        ProjectileHeavy projectileHeavyToSpawn = ProjectileHeavyPool.Instance.Get();
        projectileHeavyToSpawn.transform.position = spawnPosition;
        projectileHeavyToSpawn.transform.rotation = spawnRotation;
        projectileHeavyToSpawn.gameObject.SetActive(true);

        projectileHeavyToSpawn.OnObjectToSpawn();
        projectileHeavyToSpawn.SetTarget(newTarget);

        return projectileHeavyToSpawn;
    }


    /***** KILL FUNCTIONS *****/

    protected override void KillProjectile()
    {
        if (spriteRender.enabled)
        {
            Hide();
            Explode();
            CameraController.Instance.StopFollowProjectile();

            StartDelayKillCoroutine(particle.main.duration);
        }else
        {
            base.KillProjectile();
            ProjectileHeavyPool.Instance.ReturnToPool(this);
        }
    }


    /***** TRIGGER FUNCTIONS *****/

    private void Explode()
    {
        if (AudioManager.Instance) AudioManager.Instance.Play("Explosion");

        // Apply the damage in zone
        ApplyZoneDamage();

        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Trigger particle effect
        // particle.gameObject.SetActive(true);
        particle.Play();
    }

    private void ApplyZoneDamage()
    {
        // Catch all the objects in the range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, 1 << LayerMask.NameToLayer("Ennemy"));

        // int hitBacteriaCount = 0;
        
        // Apply damage to each object
        foreach(Collider2D c in hitColliders)
        {
            // Antibiotic doesn't damage virus
            Organism hitOrganism = c.GetComponentInParent<BacteriaCell>();

            if (hitOrganism)
            {
                ApplyDamage(hitOrganism);
                // hitBacteriaCount++;
            }
        }

        // if (hitBacteriaCount > 0)
        // {
        //     // Notify analytics of number of bacteria hit at the same time
        //     AnalyticsEvent.Custom("SuccessfulAntibioticHitLevel" + (SceneManager.GetActiveScene().buildIndex - 1), new Dictionary<string, object>
        //     {
        //         { "hit_count", hitBacteriaCount }
        //     });
        // }
    }
}

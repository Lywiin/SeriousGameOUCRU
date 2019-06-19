using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHeavy : Projectile
{
    /*** PUBLIC VARIABLES ***/

    public float explosionRadius = 30f;
    public ParticleSystem particle;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected void Awake()
    {
        ParticleSystem.ShapeModule shapeModule = particle.shape;
        shapeModule.radius = explosionRadius - 5f;
    }
    
    protected override void Start()
    {
        base.Start();

        CameraController.Instance.FollowProjectile(this);
    }


    /***** KILL FUNCTIONS *****/

    // Kill the projectile after some time
    protected override IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(lifeTime);

        // Only explode if still enabled
        if (render.enabled)
        {
            Hide();
            Explode();
        }
    }


    /***** TRIGGER FUNCTIONS *****/

    private void OnCollisionEnter2D(Collision2D c)
    {
        // Explode on impact
        Hide();
        Explode();
    }

    private void Explode()
    {
        // Apply the damage in zone
        ApplyZoneDamage();

        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Trigger particle effect
        particle.Play();

        // Destroy object when particle effect finish
        StartCoroutine(DestroyAfterTime(particle.main.duration));
    }

    private void ApplyZoneDamage()
    {
        // Catch all the objects in the range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, 1 << LayerMask.NameToLayer("Ennemy"));
        
        // Apply damage to each object
        foreach(Collider2D c in hitColliders)
        {
            // Antibiotic doesn't damage virus
            if (!c.gameObject.GetComponentInParent<Virus>())
                ApplyDamage(c.gameObject);
        }
    }

    private IEnumerator DestroyAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }
}

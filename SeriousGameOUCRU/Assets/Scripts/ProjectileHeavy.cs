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
        if (transform.GetComponent<MeshRenderer>().enabled)
            Explode();
    }


    /***** COLLISION FUNCTIONS *****/

    protected override void OnCollisionEnter(Collision collision)
    {
        // Used to prevent double collision with one projectile
        if (canCollide)
        {
            canCollide = false;

            // Check if collided object is a bacteria or a resistant gene
            if (collision.transform.GetComponentInParent<Bacteria>() || collision.gameObject.CompareTag("ResistantGene"))
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        // Apply the damage in zone
        ApplyZoneDamage();

        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Hide the projectile before the explosion
        transform.GetComponent<MeshRenderer>().enabled = false;
        transform.GetComponent<Collider>().enabled = false;

        // Freeze the projectile before playing the effect
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Trigger particle effect
        particle.Play();

        // Destroy object when particle effect finish
        StartCoroutine(DestroyAfterTime(particle.main.duration));
    }

    private void ApplyZoneDamage()
    {
        // Catch all the objects in the range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        // Apply damage to each object
        foreach(Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("BadBacteria") || c.gameObject.CompareTag("ResistantGene"))
            {
                ApplyDamage(c.gameObject);
            }
        }
    }

    private IEnumerator DestroyAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }
}

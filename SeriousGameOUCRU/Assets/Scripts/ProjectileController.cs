using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    /*** PRIVATE VARIABLES ***/

    // Components
    private Rigidbody rb;

    // Speed
    public float speed = 20f;
    public float maxVelocity = 100f;

    // Living duration
    public float lifeTime = 1f;

    // Damage
    public int damage = 10;

    // Color when boosted
    public Color boostedColor;

    // Collision
    private bool canCollide = true;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();

        // Trigger destroy countdown
        StartCoroutine(KillProjectile());
    }

    void FixedUpdate()
    {
        //Add force to the projectile
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

        //Clamp max velocity
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }


    /***** KILL FUNCTIONS *****/

    // Kill the projectile after some time
    IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }


    /***** COLLISION FUNCTIONS *****/

    private void OnCollisionEnter(Collision collision)
    {
        // Used to prevent double collision with one projectile
        if (canCollide)
        {
            canCollide = false;

            // Check if collided object is a bacteria
            Bacteria bacteriaScript = collision.transform.GetComponentInParent<Bacteria>();
            if (bacteriaScript)
            {
                // If so damage bacteria
                bacteriaScript.DamageBacteria(damage);
                CameraShake.Instance.LightScreenShake();
            }
            else if (collision.gameObject.CompareTag("ResistantGene"))
            {
                // Otherwise if gene, damage gene
                collision.gameObject.GetComponent<ResistantGene>().DamageGene(damage);
            }

            Destroy(gameObject);
        }
    }


    /***** BOOST FUNCTIONS *****/

    // Multiply damage of the current projectile
    public void MultiplyDamage(float multiplier)
    {
        // Compute new damage
        damage = (int)(damage * multiplier);

        // Apply new color
        GetComponent<Renderer>().material.SetColor("_Color", boostedColor);
    }
}

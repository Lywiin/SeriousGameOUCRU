using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    // Speed
    public float speed = 20f;
    public float maxVelocity = 100f;

    // Living duration
    public float lifeTime = 1f;

    // Damage
    public int damage = 10;

    // Color when boosted
    public Color boostedColor;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;

    // Collision
    protected bool canCollide = true;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected virtual void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();

        // Trigger destroy countdown
        StartCoroutine(KillProjectile());
    }

    protected void FixedUpdate()
    {
        //Add force to the projectile
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

        //Clamp max velocity
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }


    /***** KILL FUNCTIONS *****/

    // Kill the projectile after some time
    protected virtual IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }


    /***** COLLISION FUNCTIONS *****/

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    protected virtual void ApplyDamage(GameObject g)
    {
        // Check if collided object is a bacteria
            Bacteria bacteriaScript = g.transform.GetComponentInParent<Bacteria>();
            if (bacteriaScript)
            {
                // If so damage bacteria
                bacteriaScript.DamageBacteria(damage);
            }
            else if (g.gameObject.CompareTag("ResistantGene"))
            {
                // Otherwise if gene, damage gene
                g.gameObject.GetComponent<ResistantGene>().DamageGene(damage);
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

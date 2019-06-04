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


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;
    protected MeshRenderer meshRenderer;
    private Collider coll;

    // Collision
    protected bool canCollide = true;

    // Movement
    protected Vector3 moveDirection;
    protected GameObject target;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected virtual void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        coll = GetComponent<Collider>();

        // Trigger destroy countdown
        StartCoroutine(KillProjectile());

        moveDirection = transform.forward;
    }

    protected void FixedUpdate()
    {
        // Only change moveDirection if a cell is targeted
        if (target)
        {
            moveDirection = target.transform.position - transform.position;
            moveDirection.Normalize();
        }

        //Add force to the projectile
        rb.AddForce(moveDirection * speed, ForceMode.Impulse);

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

    protected virtual void ApplyDamage(GameObject g)
    {
        // Check if collided object is a targetable organism
        if (g.CompareTag("Targetable"))
        {
            g.transform.GetComponentInParent<Organism>().DamageOrganism(damage);
        }
    }

    // Hide projectile, freeze it and prevent it from colliding again
    protected void Hide()
    {
        // Hide the projectile before the explosion
        meshRenderer.enabled = false;
        coll.enabled = false;

        // Freeze the projectile before playing the effect
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}

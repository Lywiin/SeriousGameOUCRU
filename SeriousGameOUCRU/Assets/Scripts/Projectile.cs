using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
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
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRender;
    private CapsuleCollider2D coll;

    protected bool hidden;

    // Movement
    protected Vector2 moveDirection;
    protected Organism target;

    protected IEnumerator delayKillCoroutine;
    protected bool delayKillCoroutineRunning;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected virtual void Awake()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();

    }

    protected virtual void FixedUpdate()
    {
        if (!hidden)
        {
            // Only change moveDirection if a cell is targeted
            if (target && !target.IsDisolving())
            {
                moveDirection = target.transform.position - transform.position;
                moveDirection.Normalize();
            }

            //Add force to the projectile
            rb.AddForce(moveDirection * speed, ForceMode2D.Impulse);

            //Clamp max velocity
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
        }
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        hidden = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        spriteRender.enabled = true;
        coll.enabled = true;

        moveDirection = transform.up;
        rb.velocity = Vector2.zero;

        delayKillCoroutineRunning = false;
        
        // Trigger destroy countdown
        StartDelayKillCoroutine(lifeTime);
    }


    /***** KILL FUNCTIONS *****/

    protected virtual void StartDelayKillCoroutine(float delay)
    {
        StopDelayKillCoroutine();

        delayKillCoroutine = DelayKillProjectile(delay);
        StartCoroutine(delayKillCoroutine);
        delayKillCoroutineRunning = true;
    }

    private void StopDelayKillCoroutine()
    {
        if (delayKillCoroutineRunning)
        {
            StopCoroutine(delayKillCoroutine);
            delayKillCoroutineRunning = false;
            delayKillCoroutine = null;
        }
    }

    // Kill the projectile after some time
    private IEnumerator DelayKillProjectile(float delay)
    {
        yield return new WaitForSeconds(delay);
        KillProjectile();
    }

    protected virtual void KillProjectile()
    {
        Hide();
        target = null;
        StopDelayKillCoroutine();
    }


    /***** COLLISION FUNCTIONS *****/

    protected virtual void OnCollisionEnter2D(Collision2D c)
    {
        KillProjectile();
    }

    protected virtual void ApplyDamage(Organism o)
    {
        o.DamageOrganism(damage);
    }

    // Hide projectile, freeze it and prevent it from colliding again
    protected void Hide()
    {
        // Hide the projectile before the explosion
        spriteRender.enabled = false;
        coll.enabled = false;

        // Freeze the projectile before playing the effect
        // rb.constraints = RigidbodyConstraints2D.FreezeAll;
        hidden = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }


    /***** GETTERS/SETTERS *****/

    public void SetTarget(Organism newTarget)
    {
        target = newTarget;
    }
}

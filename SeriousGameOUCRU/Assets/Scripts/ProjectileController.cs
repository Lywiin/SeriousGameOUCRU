using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 20f;
    public float maxVelocity = 100f;
    public float lifeTime = 1f;
    public int damage = 10;
    public Color boostedColor;

    private bool canCollide = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(KillProjectile());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Add force to the projectile
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

        //Clamp max velocity
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

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
                // Otherwise if gene damage gene
                collision.gameObject.GetComponent<ResistantGene>().DamageGene(damage);
            }

            Destroy(gameObject);
        }
    }

    public void MultiplyDamage(float multiplier)
    {
        damage = (int)(damage * multiplier);
        GetComponent<Renderer>().material.SetColor("_Color", boostedColor);
    }
}

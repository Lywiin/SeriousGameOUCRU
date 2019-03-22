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
        if (collision.gameObject.CompareTag("Shield"))
        {
            collision.transform.parent.gameObject.GetComponent<Shield>().DamageShield(damage);
        }
        else if (collision.gameObject.CompareTag("BadBacteria") || collision.gameObject.CompareTag("GoodBacteria"))
        {
            collision.gameObject.GetComponent<Bacteria>().DamageBacteria(damage);
        }

        Destroy(gameObject);
    }

    public void MultiplyDamage(float multiplier)
    {
        damage = (int)(damage * multiplier);
        GetComponent<Renderer>().material.SetColor("_Color", boostedColor);
    }
}

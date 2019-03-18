using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody rb;

    public float speed;
    public float maxVelocity;
    public float lifeTime;
    public int damage;

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
    }

    IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "BadBacteria")
        {
            collision.gameObject.GetComponent<BadBacteria>().DamageBacteria(damage);
        }

        Destroy(gameObject);
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 20f;
    public float maxVelocity = 100f;
    public float lifeTime = 1f;
    public int damage = 10;

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
            collision.gameObject.GetComponent<Shield>().DamageShield(damage);
        }
        else if (collision.gameObject.CompareTag("BadBacteria"))
        {
            collision.gameObject.GetComponent<BadBacteria>().DamageBacteria(damage);
        }

        Destroy(gameObject);
    }
}

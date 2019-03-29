using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistantGene : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    
    [Header("Health")]
    public int maxHealth = 100;

    // Private variables
    private Rigidbody rb;
    private Vector3 direction;
    private int health;

    private int oldShieldMaxHealth;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Move gene across the level
        RandomlyMoveGene();
    }

    private void RandomlyMoveGene()
    {
        direction.x = Random.Range(-1f, 1f);
        direction.z = Random.Range(-1f, 1f);
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    public void DamageGene(int dmg)
    {
        //Apply damage to bacteria's health
        health -= dmg;

        //If health is below 0, the bacteria dies
        if (health <= 0)
        {
            KillGene();
        }
    }

    public void KillGene()
    {
        Destroy(gameObject);
    }

    public void SetOldShieldMaxHealth(int h)
    {
        oldShieldMaxHealth = h;
    }

    public void OnCollisionEnter(Collision collision)
    {
        // If hit a bacteria
        Bacteria bacteriaScript = collision.gameObject.GetComponent<Bacteria>();
        if (bacteriaScript)
        {
            bacteriaScript.ActivateResistance();
        }

        // If hit a shield
        Shield shieldScript = collision.gameObject.GetComponentInParent<Shield>();
        if (shieldScript)
        {
            shieldScript.SetShieldHealth(Mathf.Max(oldShieldMaxHealth, shieldScript.GetShieldHealth()));
        }

        // If gave his resistant gene it can be destroyed
        if (bacteriaScript || shieldScript)
            KillGene();
    }
}

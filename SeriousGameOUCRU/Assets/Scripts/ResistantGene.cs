using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistantGene : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Movement")]
    public float speed = 20f;
    
    [Header("Health")]
    public int maxHealth = 100;


    /*** PRIVATE VARIABLES ***/

    // Components
    private Rigidbody rb;

    // Movement direction
    private Vector3 direction;

    // Health
    private int health;

    // Track of previous shield health
    private int oldShieldMaxHealth;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        // Initialize components
        rb = transform.GetComponent<Rigidbody>();

        // Initialize direction
        direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Move gene across the level
        RandomlyMoveGene();
    }


    /***** MOVEMENTS FUNCTIONS *****/

    // Randdomly move the gene across the level
    private void RandomlyMoveGene()
    {
        // Compute a random direction
        direction.x = Random.Range(-1f, 1f);
        direction.z = Random.Range(-1f, 1f);

        // Apply force in this direction to move
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }


    /***** HEALTH FUNCTIONS *****/

    // Apply damage to the gene
    public void DamageGene(int dmg)
    {
        //Apply damage to gene's health
        health -= dmg;

        //If health is below 0, the gene dies
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


    /***** COLLISION FUNCTIONS *****/

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

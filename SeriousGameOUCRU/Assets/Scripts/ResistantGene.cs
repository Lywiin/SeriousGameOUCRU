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
        // If hit a good bacteria, activate the resistant gene
        if (collision.gameObject.CompareTag("GoodBacteria"))
        {
            collision.gameObject.GetComponent<Bacteria>().ActivateResistance();
        }

        // If hit a bad bacteria, activate the resistant gene then apply the new health
        if (collision.gameObject.CompareTag("BadBacteria"))
        {
            collision.gameObject.GetComponent<Bacteria>().ActivateResistance();
            collision.gameObject.GetComponent<Shield>().SetShieldHealth(oldShieldMaxHealth);
        }

        // If hit a shield, only change the shield health
        if (collision.gameObject.CompareTag("Shield"))
        {
            // Componenent is on parent bacteria
            collision.gameObject.transform.parent.GetComponent<Shield>().SetShieldHealth(oldShieldMaxHealth);
        }

        // Kills gene afterward
        if (collision.gameObject.CompareTag("BadBacteria") || collision.gameObject.CompareTag("GoodBacteria") || collision.gameObject.CompareTag("Shield"))
            KillGene();
    }
}

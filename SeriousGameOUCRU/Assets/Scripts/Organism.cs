using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organism : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Prefab")]
    public GameObject organismPrefab;

    [Header("Health")]
    public int maxHealth = 100;

    [Header("Disolve")]
    public float disolveSpeed = 2f;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody rb;
    protected Renderer render;
    protected Collider coll;

    // Health
    protected int health;

    // Disolve
    protected bool disolve = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/
    
    protected virtual void Awake()
    {
        InitComponents();

        // Initialize Health
        health = maxHealth;
    }

    protected virtual void InitComponents()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        render = GetComponentInChildren<Renderer>();
        coll = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        // Animate disolve of the organism
        if (disolve)
        {
            DisolveOverTime();
        }
    }


    /***** HEALTH FUNCTIONS *****/
    
    // Set the health of the organism to a new value
    public void SetOrganismHealth(int newHealth)
    {
        health = newHealth;
        UpdateHealthColor();
    }

    // Change color of the material according to health
    protected void UpdateHealthColor()
    {
        render.material.SetFloat("_LerpValue", (float)health / maxHealth);
    }

    // Apply damage to organism, update color and kill it if needed
    public virtual void DamageOrganism(int dmg)
    {
        //Apply damage to organism's health
        health -= dmg;

        //Change material color according to health
        UpdateHealthColor();

        //If health is below 0, the organism dies
        if (health <= 0)
        {
            KillOrganism();
        }
    }

    // Called when the cell has to die
    public virtual void KillOrganism()
    {
        // Trigger disolve anim in update
        disolve = true;

        // Prevent colliding again during animation
        coll.enabled = false;
    }

    // Disolve the cell according to deltaTime
    protected virtual void DisolveOverTime()
    {
        //Compute new value
        float newDisolveValue = Mathf.MoveTowards(render.material.GetFloat("_DisolveValue"), 1f, disolveSpeed * Time.deltaTime);

        // Animate the disolve of the cell
        render.material.SetFloat("_DisolveValue", newDisolveValue);

        if (newDisolveValue >= 0.75f)
        {
            // GameObject is destroyed after disolve
            Destroy(gameObject);
        }
    }
}

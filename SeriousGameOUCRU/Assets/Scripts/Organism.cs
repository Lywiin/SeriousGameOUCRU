using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organism : MonoBehaviour, IPooledObject
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
    protected Rigidbody2D rb;
    // protected Renderer render;
    protected SpriteRenderer render;
    protected CircleCollider2D coll;

    protected UIController uiController;
    protected GameController gameController;
    protected PlayerController playerController;

    // Health
    protected int health;
    protected Color baseHealthColor;
    protected Color targetHealthColor;

    // Disolve
    protected bool disolve = false;
    protected float disolveValue;


    /***** MONOBEHAVIOUR FUNCTIONS *****/
    
    protected virtual void Awake()
    {
        InitComponents();
    }

    protected virtual void InitComponents()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        // render = GetComponentInChildren<Renderer>();
        render = GetComponentInChildren<SpriteRenderer>();
        coll = transform.GetChild(0).GetComponent<CircleCollider2D>();
    }

    protected virtual void Start()
    {
        uiController = UIController.Instance;
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
    }

    public virtual void OnObjectToSpawn()
    {
        health = maxHealth;
        baseHealthColor = render.color;
        targetHealthColor = Color.white;
        disolve = false;
        disolveValue = 0f;
        UpdateHealthColor();
        // render.material.SetFloat("_DisolveValue", 0f);
        coll.enabled = true;
        rb.velocity = Vector3.zero;
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
        // render.material.SetFloat("_LerpValue", (float)health / maxHealth);
        render.color = Color.Lerp(targetHealthColor, baseHealthColor, (float)health / maxHealth);
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

    public virtual void ResetOrganismAtPosition(Vector2 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);
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
        // //Compute new value
        disolveValue = Mathf.MoveTowards(disolveValue, 1f, disolveSpeed * Time.deltaTime);

        // // Animate the disolve of the cell
        // render.material.SetFloat("_DisolveValue", newDisolveValue);

        if (disolveValue >= 1f)
        {
            // GameObject is destroyed after disolve
            DestroyOrganism();
        }
    }

    protected virtual void DestroyOrganism()
    {
        Destroy(gameObject);
    }
}

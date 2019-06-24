using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Organism : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Prefab")]
    public GameObject organismPrefab;

    [Header("Health")]
    public int maxHealth = 100;

    [Header("Death")]
    public float disolveSpeed = 2f;
    public ParticleSystem explosionParticle;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Components
    protected Rigidbody2D rb;
    protected SpriteRenderer render;
    protected CircleCollider2D bodyColl;

    protected OrganismMovement orgMovement;
    protected OrganismAttack orgAttack;
    protected OrganismDuplication orgDuplication;
    protected OrganismMutation orgMutation;

    // Instances
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

    protected float organismSize;


    /***** MONOBEHAVIOUR FUNCTIONS *****/
    
    protected virtual void Awake()
    {
        InitComponents();

        targetHealthColor = Color.white;
        baseHealthColor = render.color;
    }

    protected virtual void InitComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        render = transform.GetChild(0).GetComponent<SpriteRenderer>();
        bodyColl = transform.GetChild(0).GetComponent<CircleCollider2D>();

        orgMovement = GetComponent<OrganismMovement>();
        orgAttack = GetComponent<OrganismAttack>();
        orgDuplication = GetComponent<OrganismDuplication>();
        orgMutation = GetComponent<OrganismMutation>();
    }

    protected virtual void Start()
    {
        uiController = UIController.Instance;
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
    }

    protected virtual void Update()
    {
        // Animate disolve of the organism
        if (disolve)
        {
            DisolveOverTime();
        }
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        // explosionParticle.gameObject.SetActive(false);
        explosionParticle.Clear();

        health = maxHealth;
        UpdateHealthColor();

        disolve = false;
        disolveValue = 0.1f;
        render.material.SetFloat("_DisolveValue", disolveValue);
        
        bodyColl.enabled = true;
        rb.velocity = Vector3.zero;

        Debug.Log(render.bounds.size.x);
        UpdateOrganismSize(render.bounds.size.x);

        orgMovement.OnObjectToSpawn();
        if (orgAttack) orgAttack.OnObjectToSpawn();
        if (orgDuplication) orgDuplication.OnObjectToSpawn();
        if (orgMutation) orgMutation.OnObjectToSpawn();
    }

    public virtual void ResetOrganismAtPosition(Vector2 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);
    }

    public abstract Organism InstantiateOrganism(Vector2 spawnPosition);


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
        // render.color = Color.Lerp(targetHealthColor, baseHealthColor, (float)health / maxHealth);
    }

    // Apply damage to organism, update color and kill it if needed
    public virtual void DamageOrganism(int dmg)
    {
        //Apply damage to organism's health
        health -= dmg;

        //Change material color according to health
        UpdateHealthColor();

        //If health is below 0, the organism dies
        if (health <= 0) KillOrganism();
    }


    /***** DEATH FUNCTIONS *****/

    // Called when the organism has to die
    public virtual void KillOrganism()
    {
        // Trigger disolve anim in update
        disolve = true;

        // Prevent colliding again during animation
        bodyColl.enabled = false;
        rb.angularVelocity = 0f;
        if (orgMovement) orgMovement.SetCanMove(false);

        // explosionParticle.gameObject.SetActive(true);
        explosionParticle.Play();
    }

    // Disolve the organism according to deltaTime
    protected virtual void DisolveOverTime()
    {
        //Compute new value
        disolveValue = Mathf.MoveTowards(disolveValue, 0.75f, disolveSpeed * Time.deltaTime);
        render.material.SetFloat("_DisolveValue", disolveValue);

        if (disolveValue >= 0.75f)
        {
            // GameObject is destroyed after disolve
            DestroyOrganism();
        }
    }

    protected virtual void DestroyOrganism()
    {
        Destroy(gameObject);
    }


    /***** GETTERS/SETTERS FUNCTIONS *****/

    public int GetHealth()
    {
        return health;
    }

    public bool IsDisolving()
    {
        return disolve;
    }

    public float GetOrganismSize()
    {
        return organismSize;
    }

    public void UpdateOrganismSize(float newSize)
    {
        organismSize = newSize;
        bodyColl.radius = organismSize / 2;
        if (orgAttack) orgAttack.UpdateDetectionColliderRadius(organismSize / 2);
    }

    public OrganismMutation GetOrgMutation()
    {
        return orgMutation;
    }

}

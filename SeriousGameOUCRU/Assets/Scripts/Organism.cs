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
    public float fadeSpeed = 0.5f;
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

    // Fade
    protected bool fade = false;

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


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        explosionParticle.Clear();

        health = maxHealth;
        UpdateHealthColor();

        fade = false;
        render.material.SetFloat("_FadeValue", 1f);
        
        bodyColl.enabled = true;
        rb.velocity = Vector3.zero;

        if (!orgMutation) UpdateOrganismSize(render.bounds.size.x);

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
        // Trigger fade anim in update
        fade = true;
        StartCoroutine(FadeOverTime());

        // Prevent colliding again during animation
        bodyColl.enabled = false;
        rb.angularVelocity = 0f;
        if (orgMovement) orgMovement.SetCanMove(false);

        explosionParticle.Play();
    }

    // Fade the organism according to deltaTime
    protected IEnumerator FadeOverTime()
    {
        float i = 0.0f;
        float rate = (1.0f / fadeSpeed);
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            render.material.SetFloat("_FadeValue", 1 - i);
            yield return null;
        }

        // Destroy organism when fadding over
        DestroyOrganism();
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

    public bool IsFading()
    {
        return fade;
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

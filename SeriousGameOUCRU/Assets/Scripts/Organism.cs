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

    [Header("Effect")]
    public float wobbleDuration = 1f;


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

    // Shader effect
    protected bool fade = false;
    protected bool isWobbling = false;
    protected float wobblingTime;
    protected float wobbleAmount;

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
        if (isWobbling)
            wobblingTime += Time.deltaTime;
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        explosionParticle.Clear();

        health = maxHealth;
        UpdateHealthColor();

        isWobbling = false;
        wobblingTime = 0f;
        wobbleAmount = 0f;
        render.material.SetFloat("_WobbleAmount", wobbleAmount);

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

        if (!isWobbling)
            StartCoroutine(Wobble(wobbleDuration));
        else
            wobblingTime = 0f;

        //If health is below 0, the organism dies
        if (health <= 0) KillOrganism();
    }

    public IEnumerator Wobble(float duration)
    {
        StartWobbling();
        yield return new WaitUntil(() => wobblingTime > duration);
        StopWobbling();
    }

    private void StartWobbling()
    {
        isWobbling = true;

        StartCoroutine(MoveTowardWobbleAmount(1f));
    }

    private void StopWobbling()
    {
        StartCoroutine(MoveTowardWobbleAmount(0f));

        isWobbling = false;
        wobblingTime = 0f;
    }

    public IEnumerator MoveTowardWobbleAmount(float targetAmount)
    {
        while (wobbleAmount != targetAmount)
        {
            wobbleAmount += wobbleAmount < targetAmount ? 0.05f : -0.05f;
            wobbleAmount = Mathf.Clamp01(wobbleAmount);
            render.material.SetFloat("_WobbleAmount", wobbleAmount);
            yield return new WaitForEndOfFrame();
        }
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
        if (orgAttack) orgAttack.ResetTarget();

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

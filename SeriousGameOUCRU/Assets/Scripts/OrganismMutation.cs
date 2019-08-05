using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismMutation : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Shield")]
    public SpriteRenderer shieldRender;
    public int oneShieldHealth = 20;
    public float shieldGrowthSpeed = 2f;

    [Header("Conjugaison")]
    public float conjugaisonProba = 0.1f;
    public float conjugaisonRecallTime = 5f;

    public static float mutationProba = 0.0005f;


    /*** PRIVATE VARIABLES ***/

    // Components
    private Organism selfOrganism;
    private OrganismAttack orgAttack;
    private GameController gameController;

    // Shield
    private bool isRescaling;
    private Vector3 targetScale;
    private int shieldHealth = 0;
    private Material shieldSharedMaterial;

    // Conjugaison
    private bool canCollide = false;

    [HideInInspector] public bool canMutate = true;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        selfOrganism = GetComponent<Organism>();
        orgAttack = GetComponent<OrganismAttack>();
    }

    private void Start()
    {
        gameController = GameController.Instance;
        
        shieldSharedMaterial = shieldRender.sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        // Check is game is not currently paused
        if (canMutate && !gameController.IsGamePaused() && !selfOrganism.IsFading() && Random.Range(0f, 1f) < mutationProba)
        {
            // Attempt to mutate organism every frame
            DuplicateShield();
        }
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        canMutate = true;
        isRescaling = false;
        targetScale = Vector3.one;
        shieldHealth = 0;
        shieldRender.transform.localScale = Vector3.one;
        selfOrganism.UpdateOrganismSize(shieldRender.bounds.size.x);

        StartCoroutine(CollidingRecall());
    }


    /***** SHIELD SIZE FUNCTIONS *****/

    // Use to visualize shield health
    public void UpdateShieldSize()
    {
        // Compute new scale
        Vector3 newScale = Vector3.one;
        if (shieldHealth > 0)
        {
            shieldRender.enabled = true;
            targetScale.x = targetScale.y = 1.0f + (float)shieldHealth / 100;
        }else
        {
            targetScale = Vector3.one;
        }
        
        // Animate the scale change
        if (!isRescaling)
        {
            StartCoroutine(ResizeShield());
        }
    }

    private IEnumerator ResizeShield()
    {
        isRescaling = true;

        while (shieldRender.transform.localScale != targetScale)
        {
            shieldRender.transform.localScale = Vector3.MoveTowards(shieldRender.transform.localScale, targetScale, shieldGrowthSpeed * Time.deltaTime);
            selfOrganism.UpdateOrganismSize(shieldRender.bounds.size.x);
            yield return new WaitForEndOfFrame();
        }

        if (shieldRender.transform.localScale == Vector3.one)
            shieldRender.enabled = false;
        
        isRescaling = false;
    }

    public void ShineShields()
    {
        if (shieldHealth > 0) StartCoroutine(ShineShield(0.4f));
    }

    private IEnumerator ShineShield(float duration)
    {
        float baseIntensity = shieldSharedMaterial.GetFloat("_Intensity");
        float newIntensity = baseIntensity;
        float timeSpent = 0f;

        while (timeSpent < duration)
        {
            timeSpent += Time.deltaTime;

            newIntensity += (timeSpent < duration / 2 ? Time.deltaTime: -Time.deltaTime) * 9f;
            shieldSharedMaterial.SetFloat("_Intensity", newIntensity);

            yield return new WaitForEndOfFrame();
        }
        
        shieldSharedMaterial.SetFloat("_Intensity", baseIntensity);
    }


    /***** HEALTH FUNCTIONS *****/

    // Called by cell when it mutate stronger
    public void DuplicateShield()
    {
        shieldHealth += oneShieldHealth;
        UpdateShieldSize();
    }

    public void DamageShield(int dmg)
    {
        shieldHealth -= dmg;

        if (shieldHealth < 0)
        {
            // Keep track of damage left
            int dmgLeft = shieldHealth;
            //If shield health is below 0 we set is back to 0
            shieldHealth = 0;

            // Apply remaining damages to cell
            selfOrganism.DamageOrganism(Mathf.Abs(dmgLeft));
        }

        //Change shield size according to health
        UpdateShieldSize();
    }

    public int GetShieldHealth()
    {
        return shieldHealth;
    }

    public void SetShieldHealth(int h)
    {
        shieldHealth = h;
        UpdateShieldSize(); // Update size when changing health
    }

    public static void StopMutation()
    {
        foreach (BacteriaCell b in BacteriaCell.bacteriaCellList)
            b.GetOrgMutation().canMutate = false;
    }


    /***** CONJUGAISON FUNCTIONS *****/

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (canCollide && canMutate)
        {
            // Start coroutine to prevent multiColliding
            StartCoroutine(CollidingRecall());
            
            // Try to trigger the conjugaison
            TryToConjugateCell(c.gameObject.GetComponentInParent<OrganismMutation>());
        }
    }

    // Buffer to prevent collision for a short time
    public IEnumerator CollidingRecall()
    {
        canCollide = false;
        yield return new WaitForSeconds(conjugaisonRecallTime); // Time to wait before it can collide again
        canCollide = true;
    }

    // Process to trigger the conjugaison
    private void TryToConjugateCell(OrganismMutation otherOrgMutation)
    {
        // If collided object is a shield and conjugaison chance is triggered
        if (otherOrgMutation && otherOrgMutation.GetShieldHealth() > GetShieldHealth() && Random.Range(0, 1) < conjugaisonProba)
        {
            // Change shield health if collided object has a larger health amount
            SetShieldHealth(otherOrgMutation.GetShieldHealth());
        }
    }

    public bool CanCollide()
    {
        return canCollide;
    }
}

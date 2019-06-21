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
    private int shieldHealth = 0;

    // Conjugaison
    private bool canCollide = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        selfOrganism = GetComponent<Organism>();
        orgAttack = GetComponent<OrganismAttack>();
    }

    private void Start()
    {
        gameController = GameController.Instance;

        // TEMP to get initial proba
        if (BacteriaCell.bacteriaCellList.Count == 1)
            gameController.globalMutationProba = mutationProba;
    }

    // Update is called once per frame
    void Update()
    {
        // Check is game is not currently paused
        if (!gameController.IsGamePaused() && !selfOrganism.IsDisolving() && Random.Range(0f, 1f) < mutationProba)
        {
            // Attempt to mutate organism every frame
            DuplicateShield();
        }
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
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
            newScale.x = newScale.y = 1.0f + (float)shieldHealth / 100;
        }
        
        // Animate the scale change
        StartCoroutine(RepeatLerp(shieldRender.transform.localScale, newScale, Mathf.Abs(shieldRender.transform.localScale.magnitude - newScale.magnitude) / shieldGrowthSpeed));
    }

    // Do a complete lerp between two vectors
    private IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = (1.0f / time);
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            shieldRender.transform.localScale = Vector3.Lerp(a, b, i);
            selfOrganism.UpdateOrganismSize(shieldRender.bounds.size.x);
            yield return null;
        }
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


    /***** CONJUGAISON FUNCTIONS *****/

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (canCollide)
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

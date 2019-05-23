using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : Bacteria
{
    /*** PUBLIC VARIABLES ***/

    [Header("Conjugaison")]
    public float conjugaisonProba = 0.1f;
    public float conjugaisonRecallTime = 5f;

    [Header("Transformation")]
    public float transformationProbability = 0.5f;
    public GameObject resistantGene;

    public static List<BadBacteria> badBacteriaList = new List<BadBacteria>();

    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Movement
    private RandomMovement rm;

    // Conjugaison
    private bool canCollide = false;

    // Shield
    private Shield shieldScript;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        // Add to list
        badBacteriaList.Add(this);

        // TEMP to get initial proba
        if (BadBacteria.badBacteriaList.Count == 1)
            GameController.Instance.globalMutationProba = mutationProba;

        // Init movement component
        rm = GetComponent<RandomMovement>();

        // Initialize shield script component
        shieldScript = transform.GetComponent<Shield>();
    }

    protected override void Start()
    {
        base.Start();

        // To avoid conjugaison on spawn
        StartCoroutine(CollidingRecall());
    }

    protected override void InitComponents()
    {
        // Initialize components
        rb = transform.GetChild(1).GetComponent<Rigidbody>();
        render = transform.GetChild(0).GetComponent<Renderer>();
        coll = transform.GetChild(1).GetComponent<Collider>();
    }

    protected override void Update()
    {
        base.Update();
        // Check is game is not currently paused
        if (!GameController.Instance.IsGamePaused())
        {
            // Attempt to mutate bacteria every frame
            TryToMutateBacteria();
        }
    }


    /***** SIZE FUNCTIONS *****/

    public void UpdateBacteriaSize()
    {
        // Update size for spawning purposes
        bacteriaSize = shieldScript.GetShieldSize().x;
    }


    /***** MUTATION FUNCTIONS *****/

    protected override void TryToMutateBacteria()
    {
        // If mutation is triggered
        if (Random.Range(0f, 1f) < mutationProba)
        {
            if (isResistant)
            {
                // If shield is already activated, duplicate it
                shieldScript.DuplicateShield();
            }else
            {
                // Activate shield for the first time
                ActivateResistance();
            }
            UpdateBacteriaSize();
        }
    }

    public void IncreaseMutationProba(float increase)
    {
        mutationProba += increase;
    }


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = base.InstantiateBacteria(randomPos);
        
        // Set shield health if bacteria is resistant
        if(isResistant)
            b.GetComponent<Shield>().SetShieldHealth(shieldScript.GetShieldHealth());

        return b;
    }

    
    /***** CONJUGAISON FUNCTIONS *****/

    // Collision event called by both bacteria and shield on collision
    public void CollisionEvent(Collision collision)
    {
        if (canCollide)
        {
            // Start coroutine to prevent multiColliding
            StartCoroutine(CollidingRecall());
            
            // Try to trigger the conjugaison
            TryToConjugateBacteria(collision.gameObject.GetComponentInParent<Shield>());
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
    private void TryToConjugateBacteria(Shield s)
    {
        // If collided object is a shield and conjugaison chance is triggered
        if (s && Random.Range(0, 1) < conjugaisonProba)
        {
            // If first time we activate resistance
            if (!isResistant)
            {
                ActivateResistance();
            }

            // Change shield health if collided object has a larger health amount
            if (s.GetShieldHealth() > shieldScript.GetShieldHealth())
                shieldScript.SetShieldHealth(s.GetShieldHealth());
        }
    }

    public bool CanCollide()
    {
        return canCollide;
    }


    /***** HEALTH FUNCTIONS *****/

    // Apply damage to bacteria if shield health is at 0, otherwise damage the shield
    public override void DamageBacteria(int dmg)
    {
        if (shieldScript.GetShieldHealth() == 0)
        {
            base.DamageBacteria(dmg);
        }else
        {
            shieldScript.DamageShield(dmg);
        }
    }

    // Called when the bacteria has to die
    public override void KillBacteria()
    {
        // Stop moving
        rm.SetCanMove(false);

        // Increase killed count
        GameController.Instance.IncrementBadBacteriaKillCount();

        // Try to transform and leave resistant gene behind
        if (isResistant && Random.Range(0f, 1f) < transformationProbability)
        {
            GameObject g = Instantiate(resistantGene, transform.position, Quaternion.identity);
            g.GetComponent<ResistantGene>().SetOldShieldMaxHealth(shieldScript.GetShieldMaxHealth());
        }

        // Remove from list
        RemoveFromList();

        // Prevent shield to collide again during animation
        transform.GetChild(1).GetComponent<Collider>().enabled = false;
        transform.GetChild(1).GetComponent<Rigidbody>().Sleep();

        base.KillBacteria();
    }

    protected override void DisolveOverTime()
    {
        base.DisolveOverTime();

        // Desactivate shield for disolve
        shieldScript.DesactivateShield();
    }


    /***** LIST FUNCTIONS *****/
    
    private void RemoveFromList()
    {
        badBacteriaList.Remove(this);

        if (BadBacteria.badBacteriaList.Count == 0)
        {
            GameController.Instance.PlayerWon();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : Bacteria
{
    [Header("Conjugaison")]
    public float conjugaisonProba = 0.1f;
    public float recallTime = 5f;

    [Header("Transformation")]
    public float transformationProbability = 0.01f;
    public GameObject resistantGene;

    private bool canCollide = false;
    private bool canMutate = false;

    private Shield shieldScript;
    protected float bacteriaSize;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        shieldScript = transform.GetComponent<Shield>();

        // Init size
        bacteriaSize = transform.localScale.x;
        StartCoroutine(collidingRecall());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Check is game is not currently paused
        if (!gameController.IsGamePaused())
        {
            // Attempt to mutate bacteria every frame
            TryToMutateBacteria();
        }
    }

    private void TryToMutateBacteria()
    {
        canMutate = Random.Range(0f, 1f) < mutationProbability;

        // If mutation is triggered
        if (canMutate)
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

    public void UpdateBacteriaSize()
    {
        // Update size for spawning purposes
        bacteriaSize = transform.localScale.x * shieldScript.shield.transform.localScale.x;
    }

    public override void ActivateResistance()
    {
        base.ActivateResistance();
        shieldScript.shield.SetActive(true);
    }

    protected override void InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = Instantiate(gameObject, randomPos, Quaternion.identity);
        gameController.AddBacteriaToList(this);
        
        // Only set shield health if bacteria is resistant
        if(isResistant)
            b.GetComponent<Shield>().SetShieldHealth(shieldScript.GetShieldHealth());
    }

    protected override Collider[] TestPosition(Vector3 randomPos)
    {
        return Physics.OverlapSphere(randomPos, bacteriaSize / 2);
    }

    //Compute a random spawn position around bacteria
    protected override Vector3 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);
        return transform.position + newTrans.forward * bacteriaSize * 1.5f; // Add a little gap with *1.5f
    }

    public override void DamageBacteria(int dmg)
    {
        if (shieldScript.GetShieldHealth() == 0)
        {
            //Apply damage to bacteria's health
            health -= dmg;

            //Change material color according to health
            UpdateHealthColor();

            //If health is below 0, the bacteria dies
            if (health <= 0)
            {
                KillBacteria();
            }
        }
    }

    // Resistance transmited by conjugation
    private void OnCollisionEnter(Collision collision)
    {
        CollisionEvent(collision);
    }

    public void CollisionEvent(Collision collision)
    {
        if (canCollide)
        {
            // Start coroutine to prevent multiColliding
            StartCoroutine(collidingRecall());
            
            // If conjugaison chance is triggered
            if (Random.Range(0, 1) < conjugaisonProba)
            {
                if (collision.gameObject.CompareTag("Shield"))
                {
                    // If first time we activate resistance
                    if (!isResistant)
                    {
                        ActivateResistance();
                    }

                    // Change shield health if collided object has a larger health amount
                    int collidedShieldHealth = collision.transform.parent.gameObject.GetComponent<Shield>().GetShieldHealth();
                    if (collidedShieldHealth > shieldScript.GetShieldHealth())
                        shieldScript.SetShieldHealth(collidedShieldHealth);
                }

                // Move away from collided object
                //transform.parent.GetComponent<Bacteria>().MoveAway(collision.gameObject.transform.position);
            }
        }
    }

    public IEnumerator collidingRecall()
    {
        canCollide = false;
        yield return new WaitForSeconds(recallTime); // Time to wait before it can collide again
        canCollide = true;
    }

    public bool CanCollide()
    {
        return canCollide;
    }

    public override void KillBacteria()
    {
        // Try to transform and leave resistant gene behind
        if (isResistant && Random.Range(0f, 1f) < transformationProbability)
        {
            GameObject g = Instantiate(resistantGene, transform.position, Quaternion.identity);
            g.GetComponent<ResistantGene>().SetOldShieldMaxHealth(shieldScript.GetShieldMaxHealth());
        }

        base.KillBacteria();
    }
}

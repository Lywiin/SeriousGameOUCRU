﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : Bacteria
{
    /*** PUBLIC VARIABLES ***/

    [Header("Conjugaison")]
    public float conjugaisonProba = 0.1f;
    public float conjugaisonRecallTime = 5f;

    [Header("Transformation")]
    public float transformationProbability = 0.01f;
    public GameObject resistantGene;


    /*** PRIVATE/PROTECTED VARIABLES ***/

    // Conjugaison
    private bool canCollide = false;

    // Shield
    private Shield shieldScript;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Start()
    {
        base.Start();

        // Initialize shield script component
        shieldScript = transform.GetComponent<Shield>();

        // To avoid conjugaison on spawn
        StartCoroutine(CollidingRecall());
    }

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


    /***** SIZE FUNCTIONS *****/

    public void UpdateBacteriaSize()
    {
        // Update size for spawning purposes
        bacteriaSize = transform.localScale.x * shieldScript.shield.transform.localScale.x;
    }


    /***** MUTATION FUNCTIONS *****/

    private void TryToMutateBacteria()
    {
        // If mutation is triggered
        if (Random.Range(0f, 1f) < mutationProbability)
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


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = base.InstantiateBacteria(randomPos);
        
        // Only set shield health if bacteria is resistant
        if(isResistant)
            b.GetComponent<Shield>().SetShieldHealth(shieldScript.GetShieldHealth());

        return b;
    }

    
    /***** CONJUGAISON FUNCTIONS *****/

    // Resistance transmited by conjugation
    private void OnCollisionEnter(Collision collision)
    {
        CollisionEvent(collision);
    }

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
        // Try to transform and leave resistant gene behind
        if (isResistant && Random.Range(0f, 1f) < transformationProbability)
        {
            GameObject g = Instantiate(resistantGene, transform.position, Quaternion.identity);
            g.GetComponent<ResistantGene>().SetOldShieldMaxHealth(shieldScript.GetShieldMaxHealth());
        }

        base.KillBacteria();
    }


    /***** RESISTANCE FUNCTIONS *****/

    public override void ActivateResistance()
    {
        base.ActivateResistance();
        shieldScript.shield.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : Bacteria
{
    private bool canMutate = false;
    private bool isResistant = false;

    private GameObject shield;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // Desactivate child at the start
        shield = transform.GetChild(0).gameObject;
        shield.SetActive(isResistant);

        // Init size
        bacteriaSize = transform.localScale.x;
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
                shield.GetComponent<Shield>().DuplicateShield();
            }else
            {
                // Activate shield for the first time
                isResistant = true;
                shield.SetActive(true);
            }
            // Update size for spawning purposes
            bacteriaSize = transform.localScale.x * shield.transform.localScale.x;
        }
    }

    public override void DamageBacteria(int dmg)
    {
        if (shield.GetComponent<Shield>().GetShieldHealth() == 0)
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

    public void ActivateResistance()
    {
        isResistant = true;
    }
}

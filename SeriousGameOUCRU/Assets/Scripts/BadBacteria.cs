using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : Bacteria
{
    private bool canMutate = false;

    private GameObject shield;
    protected float bacteriaSize;

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
                ActivateResistance(this);
            }
            // Update size for spawning purposes
            bacteriaSize = transform.localScale.x * shield.transform.localScale.x;
        }
    }

    public override void ActivateResistance(Bacteria b)
    {
        base.ActivateResistance(b);
        ActivateResistance(shield.GetComponent<Shield>().GetShieldHealth());
    }

    protected override Collider[] TestPosition(Vector3 randomPos)
    {
        Debug.DrawLine(transform.position, randomPos, Color.red, 10f);
        return Physics.OverlapSphere(randomPos, bacteriaSize / 2);
    }

    //Compute a random spawn position around bacteria
    protected override Vector3 ComputeRandomSpawnPosAround()
    {
        Debug.Log("POS");
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);
        //return transform.position + newTrans.forward * bacteriaSize + newTrans.forward * transform.localScale.x / 2 * 1.5f; // Add a little gap with *1.5f
        return transform.position + newTrans.forward * bacteriaSize * 1.5f; // Add a little gap with *1.5f
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

    private void ActivateResistance(int shieldStartingHealth)
    {
        if (shield)
        {
            //base.ActivateResistance(shieldStartingHealth);
            shield.SetActive(true);
            shield.GetComponent<Shield>().SetShieldHealth(shieldStartingHealth);
        }
    }
}

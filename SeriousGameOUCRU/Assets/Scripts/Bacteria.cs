using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Bacteria : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 200;
    public float moveAwayForce = 20;
    public float moveRate = 2f;
    public float moveRateVariance = 1f;

    [Header("Health")]
    public int maxHealth = 100;
    public Color fullHealthColor;
    public Color lowHealthColor;

    [Header("Replication")]
    public float mutationProbability = 0.01f;
    public float duplicationProbability = 0.02f;
    public float duplicationRecallTime = 5f;

    // Private variables
    protected Rigidbody rb;
    protected GameController gameController;

    protected int health;

    protected float timeToMove = 0f;
    protected float randomMoveRate;
    
    protected bool canDuplicate = false;

    protected bool isResistant = false;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Initialize Health
        health = maxHealth;
        UpdateHealthColor();

        rb = GetComponent<Rigidbody>();
        gameController = GameController.Instance;

        // To avoid duplicating on spawn
        StartCoroutine(DuplicationRecall());
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Check is game is not currently paused
        if (!gameController.IsGamePaused())
        {
            // Attempt to move bacteria every frame
            TryToMoveBacteria();

            // Attempt to duplicate bacteria every frame
            TryToDuplicateBacteria();
        }
    }

    protected void UpdateHealthColor()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(lowHealthColor, fullHealthColor, (float)health / maxHealth));
    }

        private void TryToMoveBacteria()
    {
        // Randomly moves the bacteria across the level
        if (Time.time >= timeToMove)
        {
            // Computer next time bacteria should move
            randomMoveRate = Random.Range(moveRate - moveRateVariance, moveRate + moveRateVariance);
            timeToMove = Time.time + 1 / randomMoveRate;

            // Add force to the current bacteria velocity
            rb.AddForce(new Vector3(Random.Range(-moveForce, moveForce), 0f, Random.Range(-moveForce, moveForce)), ForceMode.Impulse);
        }
    }

    private void TryToDuplicateBacteria()
    {
        // If duplication is triggered
        if (canDuplicate && Random.Range(0f, 1f) < duplicationProbability)
        {
            // Buffer to prevent quick duplication
            StartCoroutine(DuplicationRecall());

            // Spawn new bacteria
            SpawnDuplicatedBacteria();
        }
    }
    
    //Spawn a new bacteria around the current one
    private void SpawnDuplicatedBacteria()
    {
        //Check if there is no object at position before spawing, if yes find a new position
        Vector3 randomPos = new Vector3();
        int nbTry = 0;
        while (nbTry < 5) // Arbitrary
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider[] hitColliders = TestPosition(randomPos);

            // If touch something doesn't duplicate (avoid bacteria spawning on top of each other)
            if (hitColliders.Length > 0)
            {
                continue;
            }

            InstantiateBacteria(randomPos);
            break;
        }
    }

    public IEnumerator DuplicationRecall()
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duplicationRecallTime); // Time to wait before it can duplicate again
        canDuplicate = true;
    }

    protected virtual GameObject InstantiateBacteria(Vector3 randomPos)
    {
        GameObject b = Instantiate(gameObject, randomPos, Quaternion.identity);
        gameController.AddBacteriaToList(b.GetComponent<Bacteria>());
        return b;
    }

    protected virtual Collider[] TestPosition(Vector3 randomPos)
    {
        return Physics.OverlapSphere(randomPos, transform.localScale.x / 2 * 1.1f); // Test 1.1 times bigger
    }

    //Compute a random spawn position around bacteria
    protected virtual Vector3 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);
        return transform.position + newTrans.forward * transform.localScale.x * 1.5f; // Add a little gap with *1.5f
    }
    
    public virtual void DamageBacteria(int dmg)
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

    public virtual void KillBacteria()
    {
        // GameObject is destroyed in the gamecontroller
        gameController.RemoveBacteriaFromList(this);
        Destroy(gameObject);
    }

    public virtual void ActivateResistance()
    {
        isResistant = true;
    }

    public bool IsResistant()
    {
        return isResistant;
    }

    public void IncreaseMutationProba(float increase)
    {
        mutationProbability += increase;
    }

}

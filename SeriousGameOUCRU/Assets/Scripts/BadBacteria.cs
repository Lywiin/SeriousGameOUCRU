using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : MonoBehaviour
{
    [Header("Movement")]
    public int moveForce = 200;
    public float moveRate = 2f;
    public float moveRateVariance = 1f;

    [Header("Health")]
    public int maxHealth = 100;
    public Color fullHealthColor;
    public Color lowHealthColor;

    [Header("Replication")]
    public float mutationProbability = 0.01f;
    public float duplicationRate = 0.02f;
    public GameObject badBacteria;

     // Private variables
    private int health;
    private Rigidbody rb;

    private float timeToMove = 0f;
    private float randomMoveRate;
    private float timeToDuplicate = 0f;
    private float randomDuplicationRate;

    private bool canMutate = false;
    private bool isResistant = false;

    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        // Desactivate child at the start
        transform.GetChild(0).gameObject.SetActive(false);

        // Initialize Health
        health = maxHealth;
        UpdateHealthColor();

        rb = GetComponent<Rigidbody>();
        gameController = Camera.main.GetComponent<GameController>();

        // To avoid duplicating on spawn
        timeToDuplicate = Random.Range(Time.time + 1 / duplicationRate / 2, Time.time + 1 / duplicationRate);
    }

    // Update is called once per frame
    void Update()
    {
        // Check is game is not currently paused
        if (!gameController.IsGamePaused())
        {
            // Attempt to move bacteria every frame
            TryToMoveBacteria();

            // Attempt to duplicate bacteria every frame
            TryToDuplicateBacteria();

            // Attempt to mutate bacteria every frame
            TryToMutateBacteria();
        }
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
        // Randomly duplicate bacteria
        if (Time.time >= timeToDuplicate)
        {
            // Compute next time bacteria should duplicate
            randomDuplicationRate = Random.Range(duplicationRate - duplicationRate / 3f, duplicationRate + duplicationRate / 3f);
            timeToDuplicate = Time.time + 1 / randomDuplicationRate;

            // Spawn a new duplicated bacteria
            SpawnDuplicatedBacteria();
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
                    transform.GetChild(0).gameObject.GetComponent<Shield>().DuplicateShield();
                }else
                {
                    // Activate child which means the resistance shield
                    transform.GetChild(0).gameObject.SetActive(true);
                    isResistant = true;
                }
            }
    }

    private void UpdateHealthColor()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(lowHealthColor, fullHealthColor, (float)health / maxHealth));
    }

    public void DamageBacteria(int dmg)
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

    protected void KillBacteria()
    {
        gameController.RemoveBacteriaFromList(gameObject);
        Destroy(gameObject);
    }

    //Compute a random spawn position around bacteria
    private Vector3 ComputeRandomSpawnPosAround()
    {
        Transform newTrans = transform;
        newTrans.Rotate(new Vector3(0.0f, Random.Range(0f, 360f), 0.0f), Space.World);
        return transform.position + newTrans.forward * gameController.bacteriaSize;
    }

    //Spawn a new bacteria around the current one
    private void SpawnDuplicatedBacteria()
    {
        //Check if there is no object at position before spawing, if yes find a new position
        Vector3 randomPos = new Vector3();
        int nbTry = 0;
        while (nbTry <= 5)
        {
            nbTry++;
            randomPos = ComputeRandomSpawnPosAround();
            Collider[] hitColliders = Physics.OverlapSphere(randomPos, gameController.bacteriaSize);

            // If touch more than one thing doesn't duplicate (avoid overcrowding of a zone)
            if (hitColliders.Length > 1)
            {
                //Debug.Log("OVERCROWDING");
                break;
            }

            // If touch something else than same bacteria try another position
            foreach (Collider c in hitColliders)
            {
                if (!c.gameObject.CompareTag( "BadBacteria"))
                {
                    continue;
                }
            }

            //Debug.Log("DUPLICATION");
            GameObject b = Instantiate(badBacteria, randomPos, Quaternion.identity);
            gameController.AddBacteriaToList(b);
            break;
        }
    }
}

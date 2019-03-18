using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBacteria : MonoBehaviour
{
    [Header("Movement")]
    public int moveForce;
    public float moveRate;
    public float moveRateVariance;

    [Header("Health")]
    public int maxHealth;
    public Color fullHealthColor;
    public Color lowHealthColor;

    [Header("Replication")]
    public float duplicationRate;
    public GameObject badBacteria;

     // Private variables
    private int health;
    private Rigidbody rb;

    private float timeToMove = 0f;
    private float randomMoveRate;
    private float timeToDuplicate = 0f;
    private float randomDuplicationRate;

    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
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
        // Randomly moves the bacteria across the level
        if (!gameController.IsGamePaused() && Time.time >= timeToMove)
        {
            // Computer next time bacteria should move
            randomMoveRate = Random.Range(moveRate - moveRateVariance, moveRate + moveRateVariance);
            timeToMove = Time.time + 1 / randomMoveRate;

            // Add force to the current bacteria velocity
            rb.AddForce(new Vector3(Random.Range(-moveForce, moveForce), 0f, Random.Range(-moveForce, moveForce)), ForceMode.Impulse);
        }

        // Randomly duplicate bacteria
        if (!gameController.IsGamePaused() && Time.time >= timeToDuplicate)
        {
            // Compute next time bacteria should duplicate
            randomDuplicationRate = Random.Range(duplicationRate - duplicationRate / 3f, duplicationRate + duplicationRate / 3f);
            timeToDuplicate = Time.time + 1 / randomDuplicationRate;

            // Spawn a new duplicated bacteria
            SpawnDuplicatedBacteria();
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
                if (c.gameObject.tag != "BadBacteria")
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

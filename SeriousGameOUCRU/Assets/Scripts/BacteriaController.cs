using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaController : MonoBehaviour
{
    [Header("Movement")]
    public int moveForce;
    public float moveRate;
    public float moveRateVariance;

    [Header("Health")]
    public int maxHealth;
    public Color fullHealthColor;
    public Color lowHealthColor;

    // Private variables
    private int health;
    private Rigidbody rb;
    private float timeToMove = 0f;
    private float randomMoveRate;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        gameController = Camera.main.GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Randomly moves the bacteria across the level
        if (!gameController.IsGamePaused() && Time.time >= timeToMove)
        {
            randomMoveRate = Random.Range(moveRate - moveRateVariance, moveRate + moveRateVariance);
            timeToMove = Time.time + 1 / randomMoveRate;

            // Add force to the current bacteria velocity
            rb.AddForce(new Vector3(Random.Range(-moveForce, moveForce), 0f, Random.Range(-moveForce, moveForce)), ForceMode.Impulse);
        }
    }
    
    public void DamageBacteria(int dmg)
    {
        //Apply damage to bacteria's health
        health -= dmg;

        //Change material color according to health
        GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(lowHealthColor, fullHealthColor, (float)health / maxHealth));

        //If health is below 0, the bacteria dies
        if (health <= 0)
        {
            KillBacteria();
        }
    }

    void KillBacteria()
    {
        gameController.RemoveBacteriaFromList(gameObject);
        Destroy(gameObject);
    }

    
}

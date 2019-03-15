﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject bacteria;
    public Vector2 gameZoneRadius;
    public int bacteriaCount;
    public float bacteriaSize;

    // Private variables
    private bool isPaused = false;
    private List<GameObject> bacteriaList;

    // Start is called before the first frame update
    void Start()
    {
        bacteriaList = new List<GameObject>();

        for (int i = 0; i < bacteriaCount; i++)
        {
            //Check if there is no object at position before spawing, if yes find a new position
            int nbHit = 0;
            Vector3 randomPos = new Vector3();
            do
            {
                randomPos = ComputeRandomSpawnPos();
                Collider[] hitColliders = Physics.OverlapSphere(randomPos, bacteriaSize);
                nbHit = hitColliders.Length;
            } while (nbHit != 0);
            
            //Instantiate bacteria at position and add it to the list
            GameObject b = Instantiate(bacteria, randomPos, Quaternion.identity);
            bacteriaList.Add(b);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
        if (Input.GetButtonDown("Cancel"))
        {
            RestartGame();
        }
    }

    //Restart the game
    void RestartGame()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    //Activate or desactivate player input and objects movements
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (player != null)
        {
            ToggleFreeze(player.GetComponent<Rigidbody>());
        }
        foreach (GameObject b in bacteriaList)
        {
            ToggleFreeze(b.GetComponent<Rigidbody>());
        }
    }

    //Return pause state
    public bool IsGamePaused()
    {
        return isPaused;
    }

    //Toggle freeze of all objects position and rotation
    public void ToggleFreeze(Rigidbody rb)
    {
        if (IsGamePaused())
        {
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    //Called by a bacteria when it dies
    public void RemoveBacteriaFromList(GameObject b)
    {
        bacteriaList.Remove(b);
        if (bacteriaList.Count == 0)
        {
            //Player won
            Debug.Log("You won");
        }
    }

    //Compute a random spawn position from gameZoneRadius and bacteriaSize
    private Vector3 ComputeRandomSpawnPos()
    {
        return new Vector3(Random.Range(-gameZoneRadius.x + bacteriaSize, gameZoneRadius.x - bacteriaSize), 
                        0.0f, Random.Range(-gameZoneRadius.y + bacteriaSize, gameZoneRadius.y - bacteriaSize));
    }

    //Called by player when he dies
    public void PlayerDied()
    {
        Debug.Log("You died!");
        Destroy(player);
        Invoke("RestartGame", 2);
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Prefabs")]
    public GameObject player;
    public GameObject badBacteria;
    public GameObject goodBacteria;
    public GameObject goodBacteriaGroup;

    [Header("Spawn")]
    public Vector2 gameZoneRadius;
    public int badBacteriaCount;
    public int goodBacteriaGroupCount;
    public float bacteriaInitSize;
    public float playerSpawnSafeRadius = 15f;

    [Header("Mutation")]
    public float mutationProbaIncrease = 0.00075f;


    /*** PRIVATE VARIABLES ***/

    // Ingame pause
    private bool isPaused = false;

    // Keep track of the mutation proba
    [HideInInspector]
    public float globalMutationProba = 0f;

    // Control what can the player do
    private bool canPlayerMove = true;
    private bool canPlayerMoveCamera = true;
    private bool canPlayerShoot = true;

    // Keep track of number of killed bad bacteria
    private int badBacteriaKillCount = 0;

    /*** INSTANCE ***/

    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }


    void Start()
    {
        // Initialize bacteria lists
        GoodBacteria.goodBacteriaList.Clear();
        BadBacteria.badBacteriaList.Clear();

        // // Setup the game and spawn bacterias
        // SetupGame();
    }

    void Update()
    {
        // Pause the game
        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }

        // Restart the game
        if (Input.GetButtonDown("Cancel"))
        {
            RestartGame();
        }
    }

    /***** START FUNCTIONS *****/

    // Setup the game and spawn bacterias
    public void SetupGame()
    {
        // Fade info UI in
        UIController.Instance.GetComponent<Animator>().SetTrigger("FadeInInfoPanel");

        // Spawn some bacterias
        StartCoroutine(DelaySpawnBadBacteria());
        StartCoroutine(DelaySpawnGoodBacteriaGroup());
    }

    private IEnumerator DelaySpawnBadBacteria()
    {        
        // Spawn some bacterias
        for (int i = 0; i < badBacteriaCount; i++)
        {
            SpawnBacteria(badBacteria);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DelaySpawnGoodBacteriaGroup()
    {
        // Spawn some bacterias
        for (int i = 0; i < goodBacteriaGroupCount; i++)
        {
            SpawnGoodBacteriaGroup();
            yield return new WaitForEndOfFrame();
        }
    }

    //Spawn a new bacteria
    private void SpawnBacteria(GameObject bacteria)
    {
        Vector3 validPos = GetAValidPos(bacteriaInitSize);
        
        //Instantiate bacteria at position and add it to the list
        GameObject b = Instantiate(bacteria, validPos, Quaternion.identity);
    }

    // Spawn a new bacteria group
    private void SpawnGoodBacteriaGroup()
    {
        // We take a larger space since the group will be larger than a normal cell
        Vector3 validPos = GetAValidPos(bacteriaInitSize * 4);

        // Spawn the root of the group
        GameObject root = Instantiate(goodBacteriaGroup, validPos, Quaternion.identity);

        // Compute a random number of cell to spawn
        int nbCellToSpawn = Random.Range(3, 6);

        // Spawn each cell and attach them to the root
        for (int i = 0; i < nbCellToSpawn; i++)
        {
            // Compute an offset to spawn cell
            Vector3 posOffset = new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));

            // Spawn cell
            GameObject cell = Instantiate(goodBacteria, validPos + posOffset, Quaternion.identity);

            // Attach the joint to the root rigidbody
            cell.GetComponent<SpringJoint>().connectedBody = root.GetComponent<Rigidbody>();

            cell.transform.parent = root.transform;
        }
    }

    //Compute a random spawn position from gameZoneRadius and bacteriaSize
    private Vector3 ComputeRandomSpawnPos()
    {
        Vector3 pos = new Vector3(Random.Range(-gameZoneRadius.x + bacteriaInitSize, gameZoneRadius.x - bacteriaInitSize), 
                        0.0f, Random.Range(-gameZoneRadius.y + bacteriaInitSize, gameZoneRadius.y - bacteriaInitSize));

        float playerX = player.transform.position.x;
        float playerZ = player.transform.position.z;

        // Prevent spawning around the player
        if(pos.x > playerX - playerSpawnSafeRadius && pos.x < playerX + playerSpawnSafeRadius)
        {
            pos.x = playerX + playerSpawnSafeRadius * Mathf.Sign(pos.x - playerX);
        }
        if(pos.z > playerZ - playerSpawnSafeRadius && pos.z < playerZ + playerSpawnSafeRadius)
        {
            pos.z = playerZ + playerSpawnSafeRadius * Mathf.Sign(pos.z - playerZ);
        }

        return pos;
    }

    // Return a valid random position for a certain radius
    private Vector3 GetAValidPos(float radiusSize)
    {
        int nbHit = 0;
        Vector3 randomPos = new Vector3();
        do
        {
            randomPos = ComputeRandomSpawnPos();
            Collider[] hitColliders = Physics.OverlapSphere(randomPos, radiusSize);
            nbHit = hitColliders.Length;
        } while (nbHit != 0);

        return randomPos;
    }

    // Restart the game
    public void RestartGame()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        Time.timeScale = 1.0f;
    }


    /***** PAUSE FUNCTIONS *****/

    // Activate or desactivate player input and objects movements
    public void TogglePause()
    {
        // Change pause state
        isPaused = !isPaused;

        // Display pause UI
        UIController.Instance.TogglePauseUI(isPaused);

        // Change time scale according to bool
        Time.timeScale = isPaused ? 0 : 1;
    }

    // Get pause state
    public bool IsGamePaused()
    {
        return isPaused;
    }


    /***** OUTCOME FUNCTIONS *****/

    //Called by player when he dies
    public void GameOver()
    {
        // Hide the minimap
        Minimap.Instance.HideMinimap();

        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Update the UI
        UIController.Instance.TriggerGameOver();

        // Kill the player and restart
        Destroy(player);
    }

    //Called when the player win
    public void PlayerWon()
    {
        // Hide the minimap
        Minimap.Instance.HideMinimap();

        // Update the UI and restart
        UIController.Instance.TriggerVictory();
    }


    /***** MUTATION FUNCTIONS *****/

    // Called by playerController when fire heavy projectile
    public void IncreaseAllMutationProba()
    {
        // Keep trace of global inscrease
        globalMutationProba += mutationProbaIncrease;

        // Update bacteria mutation rate
        foreach (BadBacteria b in BadBacteria.badBacteriaList)
        {
            b.IncreaseMutationProba(mutationProbaIncrease);
        }
    }


    /***** GETTERS *****/

    public float GetGlobalMutationGlobalProba()
    {
        return globalMutationProba;
    }

    public bool CanPlayerMove()
    {
        return canPlayerMove;
    }
    public bool CanPlayerMoveCamera()
    {
        return canPlayerMoveCamera;
    }
    public bool CanPlayerShoot()
    {
        return canPlayerShoot;
    }

    public int GetBadBacteriaKillCount()
    {
        return badBacteriaKillCount;
    }


    /***** SETTERS *****/

    public void BlockPlayerInput()
    {
        canPlayerMove = false;
        canPlayerMoveCamera = false;
        canPlayerShoot = false;
    }

    public void SetCanPlayerMove(bool b)
    {
        canPlayerMove = b;
    }
    public void SetCanPlayerMoveCamera(bool b)
    {
        canPlayerMoveCamera = b;
    }
    public void SetCanPlayerShoot(bool b)
    {
        canPlayerShoot = b;
    }

    public void IncrementBadBacteriaKillCount()
    {
        badBacteriaKillCount++;
    }
}

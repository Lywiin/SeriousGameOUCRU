using System.Collections;
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

    [Header("Spawn")]
    public Vector2 gameZoneRadius;
    public int badBacteriaCount;
    public int goodBacteriaCount;
    public float bacteriaInitSize;

    [Header("Mutation")]
    public float mutationProbaIncrease;

    [Header("UI")]
    public UIController uiController;


    /*** PRIVATE VARIABLES ***/

    // Ingame pause
    private bool isPaused = false;

    // Keep track of the mutation proba
    [HideInInspector]
    public float globalMutationProba = 0f;


    /*** INSTANCE ***/

    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        // Initialize bacteria lists
        GoodBacteria.goodBacteriaList.Clear();
        BadBacteria.badBacteriaList.Clear();

        // Setup the game and spawn bacterias
        SetupGame();
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
    private void SetupGame()
    {
        // Spawn some bacterias
        for (int i = 0; i < badBacteriaCount; i++)
        {
            SpawnBacteria(badBacteria);
        }
        for (int i = 0; i < goodBacteriaCount; i++)
        {
            SpawnBacteria(goodBacteria);
        }
    }

    //Spawn a new bacteria
    private void SpawnBacteria(GameObject bacteria)
    {
        //Check if there is no object at position before spawing, if yes find a new position
            int nbHit = 0;
            Vector3 randomPos = new Vector3();
            do
            {
                randomPos = ComputeRandomSpawnPos();
                Collider[] hitColliders = Physics.OverlapSphere(randomPos, bacteriaInitSize);
                nbHit = hitColliders.Length;
            } while (nbHit != 0);
            
            //Instantiate bacteria at position and add it to the list
            GameObject b = Instantiate(bacteria, randomPos, Quaternion.identity);
    }

    //Compute a random spawn position from gameZoneRadius and bacteriaSize
    private Vector3 ComputeRandomSpawnPos()
    {
        return new Vector3(Random.Range(-gameZoneRadius.x + bacteriaInitSize, gameZoneRadius.x - bacteriaInitSize), 
                        0.0f, Random.Range(-gameZoneRadius.y + bacteriaInitSize, gameZoneRadius.y - bacteriaInitSize));
    }

    // Restart the game
    private void RestartGame()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }


    /***** PAUSE FUNCTIONS *****/

    // Activate or desactivate player input and objects movements
    public void TogglePause()
    {
        // Change pause state
        isPaused = !isPaused;

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
    public void PlayerDied()
    {
        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Update the UI
        uiController.DisplayGameOver();

        // Kill the player and restart
        Destroy(player);
        Invoke("RestartGame", 2);
    }

    //Called when the player win
    private void PlayerWon()
    {
        // Update the UI and restart
        uiController.DisplayVictory();
        Invoke("RestartGame", 2);
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
}

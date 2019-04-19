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
    public float mutationProbaIncrease = 0.00075f;

    [Header("UI")]
    public UIController uiController;


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
        Vector3 pos = new Vector3(Random.Range(-gameZoneRadius.x + bacteriaInitSize, gameZoneRadius.x - bacteriaInitSize), 
                        0.0f, Random.Range(-gameZoneRadius.y + bacteriaInitSize, gameZoneRadius.y - bacteriaInitSize));

        // Prevent spawning around the player
        if(pos.x > -10f && pos.x < 10f)
        {
            pos.x = 10f * Mathf.Sign(pos.x);
        }
        if(pos.z > -10f && pos.z < 10f)
        {
            pos.z = 10f * Mathf.Sign(pos.z);
        }

        return pos;
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
        uiController.TogglePauseUI(isPaused);

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
        // Hide the minimap
        Minimap.Instance.HideMinimap();

        // Shake the screen
        CameraShake.Instance.HeavyScreenShake();

        // Update the UI
        uiController.TriggerGameOver();

        // Kill the player and restart
        Destroy(player);
    }

    //Called when the player win
    public void PlayerWon()
    {
        // Hide the minimap
        Minimap.Instance.HideMinimap();

        // Update the UI and restart
        uiController.TriggerVictory();
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

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
    private float globalMutationProba = 0f;

    // Bacteria lists
    private List<Bacteria> goodBacteriaList;
    private List<Bacteria> badBacteriaList;


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
        goodBacteriaList = new List<Bacteria>();
        badBacteriaList = new List<Bacteria>();

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

        // TEMP to get initial proba
        globalMutationProba = badBacteriaList[0].mutationProbability;
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
            //bacteriaList.Add(b);
            AddBacteriaToList(b.GetComponent<Bacteria>());
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
        isPaused = !isPaused;

        // Freeze player
        if (player)
        {
            ToggleFreeze(player.GetComponent<Rigidbody>());
        }

        // Freeze bad bacteria
        foreach (Bacteria bb in badBacteriaList)
        {
            ToggleFreeze(bb.GetComponent<Rigidbody>());
        }

        // Freeze good bacteria
        foreach (Bacteria gb in goodBacteriaList)
        {
            ToggleFreeze(gb.GetComponent<Rigidbody>());
        }
    }

    // Get pause state
    public bool IsGamePaused()
    {
        return isPaused;
    }

    // Toggle freeze of all objects position and rotation, which means change their rigidbody constraint
    public void ToggleFreeze(Rigidbody rb)
    {
        if (IsGamePaused())
        {
            // When game is paused
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            // When game is not paused
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }


    /***** LISTS FUNCTIONS *****/

    //Called by a bacteria when it dies
    public void RemoveBacteriaFromList(Bacteria b)
    {
        if (b is BadBacteria)
        {
            badBacteriaList.Remove(b);
        }else if (b is GoodBacteria)
        {
            goodBacteriaList.Remove(b);
        }
        
        // If no bad bacteria left, player win
        if (badBacteriaList.Count == 0)
        {
            PlayerWon();
        }
    }

    //Called by a bacteria when it deplicate
    public void AddBacteriaToList(Bacteria b)
    {
        if (b is BadBacteria)
        {
            badBacteriaList.Add(b);
        }else
        {
            goodBacteriaList.Add(b);
        }
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
        foreach (Bacteria bb in badBacteriaList)
        {
            bb.GetComponent<Bacteria>().IncreaseMutationProba(mutationProbaIncrease);
        }
        foreach (Bacteria gb in goodBacteriaList)
        {
            gb.GetComponent<Bacteria>().IncreaseMutationProba(mutationProbaIncrease);
        }
    }


    /***** GETTERS *****/

    public int GetCurrentBadBacteriaCount()
    {
        return badBacteriaList.Count;
    }

    public int GetCurrentGoodBacteriaCount()
    {
        return goodBacteriaList.Count;
    }

    public float GetGlobalMutationGlobalProba()
    {
        return globalMutationProba;
    }
}

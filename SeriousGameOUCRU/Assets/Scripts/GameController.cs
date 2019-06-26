using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Prefabs")]
    public GameObject player;
    public GameObject bacteriaCell;
    public GameObject humanCell;
    public GameObject humanCellGroup;
    public GameObject virus;

    [Header("Spawn")]
    public Vector2 gameZoneRadius;
    public int bacteriaCellCount;
    public int humanCellCount;
    public int virusCount;
    public float cellInitSize;
    public float playerSpawnSafeRadius = 15f;

    [Header("Mutation")]
    public float mutationProbaStart = 0.0005f;
    public float mutationProbaIncrease = 0.00075f;


    /*** PRIVATE VARIABLES ***/

    private UIController uiController;

    // Ingame pause
    private bool isPaused = false;

    // Keep track of the mutation proba
    [HideInInspector]
    public float globalMutationProba = 0f;

    // Control what can the player do
    private bool canPlayerMove = true;
    private bool canPlayerMoveCamera = true;
    private bool canPlayerShoot = true;

    // Keep track of number of killed bacteria cell
    private int bacteriaCellKillCount = 0;

    // Size
    private float bacteriaCellSize;
    private float humanCellSize;
    private float virusSize;


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

        bacteriaCellSize = bacteriaCell.GetComponentInChildren<Renderer>().bounds.size.x;
        humanCellSize = humanCell.GetComponentInChildren<Renderer>().bounds.size.x;
        virusSize = virus.GetComponentInChildren<Renderer>().bounds.size.x;

        OrganismMutation.mutationProba = mutationProbaStart;
    }


    void Start()
    {
        uiController = UIController.Instance;

        // Initialize cell lists
        HumanCell.humanCellList.Clear();
        BacteriaCell.bacteriaCellList.Clear();
        Virus.virusList.Clear();
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

    // Setup the game and spawn cells
    public void SetupGame()
    {
        // Fade info UI in
        UIController.Instance.GetComponent<Animator>().SetTrigger("FadeInInfoPanel");

        StartCoroutine(StartSpawningWithDelay());
    }

    private IEnumerator StartSpawningWithDelay()
    {
        yield return new WaitForFixedUpdate();

        SpawnBacteriaCell();
        SpawnVirus();
        SpawnHumanCell();

    }

    private void SpawnBacteriaCell()
    {        
        // Spawn some bacteria cells
        for (int i = 0; i < bacteriaCellCount; i++)
        {
            // Get a valid position by using object size as parameter
            Vector3 validPos = GetAValidPos(bacteriaCellSize);

            BacteriaCell.InstantiateBacteriaCell(validPos);
        }
    }

    private void SpawnHumanCell()
    {        
        // Spawn some bacteria cells
        for (int i = 0; i < humanCellCount; i++)
        {
            // Get a valid position by using object size as parameter
            Vector2 validPos = GetAValidPos(humanCellSize);

            HumanCell.InstantiateHumanCell(validPos);
        }
    }

    private void SpawnVirus()
    {        
        // Spawn some bacteria cells
        for (int i = 0; i < virusCount; i++)
        {
            // Get a valid position by using object size as parameter
            Vector2 validPos = GetAValidPos(virusSize);

            Virus.InstantiateVirus(validPos);
        }
    }

    //Spawn a new object at a computed valid position
    private void SpawnObjectAtValidPos(GameObject obj, float objSize)
    {
        // Get a valid position by using object size as parameter
        Vector2 validPos = GetAValidPos(objSize);
        
        //Instantiate cell at position and add it to the list
        GameObject b = Instantiate(obj, validPos, Quaternion.identity);
    }

    //Compute a random spawn position from gameZoneRadius and cellSize
    private Vector3 ComputeRandomSpawnPos()
    {
        Vector2 pos = new Vector2(Random.Range(-gameZoneRadius.x + cellInitSize, gameZoneRadius.x - cellInitSize), 
                                  Random.Range(-gameZoneRadius.y + cellInitSize, gameZoneRadius.y - cellInitSize));

        float playerX = player.transform.position.x;
        float playerY = player.transform.position.y;

        // Prevent spawning around the player
        if(pos.x > playerX - playerSpawnSafeRadius && pos.x < playerX + playerSpawnSafeRadius)
        {
            pos.x = playerX + playerSpawnSafeRadius * Mathf.Sign(pos.x - playerX);
        }
        if(pos.y > playerY - playerSpawnSafeRadius && pos.y < playerY + playerSpawnSafeRadius)
        {
            pos.y = playerY + playerSpawnSafeRadius * Mathf.Sign(pos.y - playerY);
        }

        return pos;
    }

    // Return a valid random position for a certain radius
    private Vector2 GetAValidPos(float radiusSize)
    {
        int nbHit = 0;
        Vector2 randomPos = new Vector2();

        do
        {
            // Get a new random position
            randomPos = ComputeRandomSpawnPos();

            // Test to see if any object is near that position
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(randomPos, radiusSize, ~(1 << 1));
            nbHit = hitColliders.Length;

            // if there is any we try again until we find an empty position
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
    public void GameOver()
    {
        // Hide the indicators
        // CloseEnnemyUI.Instance.HideAllIndicators();

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
        // Hide the indicators
        CloseEnnemyUI.Instance.HideAllIndicators();

        // Update the UI and restart
        uiController.TriggerVictory();
    }


    /***** MUTATION FUNCTIONS *****/

    // Called by playerController when fire heavy projectile
    public void IncreaseAllMutationProba()
    {
        OrganismMutation.mutationProba += mutationProbaIncrease;
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

    public int GetBacteriaCellKillCount()
    {
        return bacteriaCellKillCount;
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

    public void IncrementBacteriaCellKillCount()
    {
        bacteriaCellKillCount++;
    }
}

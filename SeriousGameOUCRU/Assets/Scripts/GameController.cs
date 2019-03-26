using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
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

    // Private variables
    private bool isPaused = false;

    private float globalMutationProbaIncrease = 0f;

    // Bacteria lists
    private List<Bacteria> goodBacteriaList;
    private List<Bacteria> badBacteriaList;

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

    // Start is called before the first frame update
    void Start()
    {
        goodBacteriaList = new List<Bacteria>();
        badBacteriaList = new List<Bacteria>();

        for (int i = 0; i < badBacteriaCount; i++)
        {
            SpawnBacteria(badBacteria);
        }

        for (int i = 0; i < goodBacteriaCount; i++)
        {
            SpawnBacteria(goodBacteria);
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

        foreach (Bacteria bb in badBacteriaList)
        {
            ToggleFreeze(bb.GetComponent<Rigidbody>());
        }
        foreach (Bacteria gb in goodBacteriaList)
        {
            ToggleFreeze(gb.GetComponent<Rigidbody>());
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
    public void RemoveBacteriaFromList(Bacteria b)
    {
        if (b is BadBacteria)
        {
            badBacteriaList.Remove(b);
        }else
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

    //Compute a random spawn position from gameZoneRadius and bacteriaSize
    private Vector3 ComputeRandomSpawnPos()
    {
        return new Vector3(Random.Range(-gameZoneRadius.x + bacteriaInitSize, gameZoneRadius.x - bacteriaInitSize), 
                        0.0f, Random.Range(-gameZoneRadius.y + bacteriaInitSize, gameZoneRadius.y - bacteriaInitSize));
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

    //Called by player when he dies
    public void PlayerDied()
    {
        Debug.Log("You died!");
        Destroy(player);
        Invoke("RestartGame", 2);
    }

    //Called when the player win
    private void PlayerWon()
    {
        Debug.Log("You won");
        Invoke("RestartGame", 2);
    }

    // Called by playerController when fire heavy projectile
    public void IncreaseAllMutationProba()
    {
        // Keep trace of global inscrease
        globalMutationProbaIncrease += mutationProbaIncrease;

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
}

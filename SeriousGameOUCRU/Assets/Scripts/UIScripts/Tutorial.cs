using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    public RectTransform cursorRect;

    public GameObject invisibleWall;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private CameraController cameraController;

    private List<Organism> bacteriaCellList;
    private List<Virus> virusList;
    private Organism humanCell;

    private bool blockWeaponChange = true;

    private bool freezeTime = false;


    /*** INSTANCE ***/

    private static Tutorial _instance;
    public static Tutorial Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Setup the instance
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            PlayerPrefs.SetInt("Tutorial", 1);
        }

        bacteriaCellList = new List<Organism>();
    }

    private void Start()
    {
        gameController = GameController.Instance;
        playerController = PlayerController.Instance;
        cameraController = CameraController.Instance;

        gameController.TogglePlayerInput(false);

        // Hide UI since it's useless for now
        // UIController.Instance.gameObject.SetActive(false);
        MobileUI.Instance.gameObject.SetActive(false);

        virusList = Virus.virusList;

    }



    /***** ANIMATOR FUNCTIONS *****/

    public void StartTutorial()
    {
        // ShowMoveText();
        animator.SetTrigger("StartTutorial");
    }

    public void StartTutorialCoroutine()
    {
        StartCoroutine(TutorialCoroutine());
    }

    private IEnumerator TutorialCoroutine()
    {
        ReplaceWalls();
        yield return new WaitForSeconds(1.5f);
        gameController.SetCanPlayerMove(false);

        // Cell tutorial
        // Process to next step
        SpawnLightFireTutorialCell();
        animator.SetTrigger("FadeInBacteriaText");

        // Wait for player to get close to cells
        yield return new WaitUntil(() => bacteriaCellList[0] != null && Vector2.Distance(playerController.transform.position, bacteriaCellList[0].transform.position) < 30f);
        UnfreezeLightFireTutorial();

        // Shoot tutorial
        // Wait for the player to fire to reset time to normal
        yield return new WaitUntil(() => playerController.IsFiring());
        animator.SetTrigger("FadeOutCursor");
        RescaleTime(1f);

        // Wait for the player to kill the bacteria
        yield return new WaitUntil(() => !playerController.IsFiring());
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("FadeInSecondWeaponText");
        RescaleTime(0f);

        // Wait for the player to change weapon
        yield return new WaitUntil(() => playerController.IsHeavyWeaponSelected());
        animator.SetTrigger("FadeInAntibioticText");
        RescaleTime(0f);

        // Setup for heavy fire tutorial
        ReplaceWalls();
        SpawnHeavyFireTutorialCell();
        UIController.Instance.gameObject.SetActive(true);
        UIController.Instance.ToggleInfoPanel(true);
        UIController.Instance.ToggleInfoPanelCount(false);

        // Wait for player to kill all the cells
        yield return new WaitUntil(() => BacteriaCell.bacteriaCellList.Count == 0);
        yield return new WaitForSeconds(1.5f);
        blockWeaponChange = false;
        gameController.SetCanPlayerChangeWeapon(true);
        UIController.Instance.ToggleInfoPanel(false);
        animator.SetTrigger("FadeInVirusText");
        RescaleTime(0f);

        //Setup for virus tutorial
        ReplaceWalls();
        SpawnVirusTutorialCell();

        // Wait for player to get close to virus
        yield return new WaitUntil(() => virusList[0] != null && Vector2.Distance(playerController.transform.position, virusList[0].transform.position) < 50f);
        UnfreezeVirus();

        // Wait for player to kill the virus
        yield return new WaitUntil(() => Virus.virusList.Count == 0);
        gameController.TogglePlayerInput(false);
        animator.SetTrigger("FinishTutorial");
    }    

    public void RescaleTime(float value)
    {
        freezeTime = value == 0f;
        Time.timeScale = value;
    }



    /***** CELL SPAWN FUNCTIONS *****/

    private void SpawnLightFireTutorialCell()
    {
        Vector2 spawnPos = playerController.transform.position;

        // Spawn bacteria cell
        spawnPos.x += 100f;
        SpawnTutorialBacteriaCell(spawnPos);

        // Spawn human cell
        spawnPos.x -= 10f;
        spawnPos.y += 5f;
        SpawnTutorialHumanCell(spawnPos);
        spawnPos.y -= 10f;
        SpawnTutorialHumanCell(spawnPos);
    }

    private void SpawnHeavyFireTutorialCell()
    {
        Vector2 spawnPos = playerController.transform.position;

        spawnPos.x += 100f;
        SpawnTutorialBacteriaCell(spawnPos);

        spawnPos.x += 20f;
        spawnPos.y += 3f;
        SpawnTutorialBacteriaCell(spawnPos);

        spawnPos.x -= 11f;
        spawnPos.y -= 18f;
        SpawnTutorialBacteriaCell(spawnPos);

        UnfreezeBacteriaCell(true);
    }

    private void SpawnVirusTutorialCell()
    {
        Vector2 spawnPos = playerController.transform.position;

        spawnPos.x += 100f;
        SpawnTutorialVirus(spawnPos);

        spawnPos.x -= 10f;
        spawnPos.y += 5f;
        SpawnTutorialHumanCell(spawnPos);
        spawnPos.y -= 10f;
        SpawnTutorialHumanCell(spawnPos);
    }




    private void SpawnTutorialBacteriaCell(Vector2 spawnPos)
    {
        Organism bacteriaCell = BacteriaCell.InstantiateBacteriaCell(spawnPos);
        bacteriaCell.GetComponent<OrganismAttack>().enabled = false;
        bacteriaCell.GetComponent<OrganismMovement>().enabled = false;
        bacteriaCell.GetComponent<OrganismDuplication>().enabled = false;
        bacteriaCell.GetComponent<OrganismMutation>().enabled = false;

        bacteriaCellList.Add(bacteriaCell);
    }

    private void SpawnTutorialHumanCell(Vector2 spawnPos)
    {
        humanCell = HumanCell.InstantiateHumanCell(spawnPos);
        humanCell.GetComponent<OrganismDuplication>().enabled = false;
    }

    private void SpawnTutorialVirus(Vector2 spawnPos)
    {
        Organism virus = Virus.InstantiateVirus(spawnPos);
        virus.GetComponent<OrganismAttack>().enabled = false;
        virus.GetComponent<OrganismMovement>().enabled = false;
    }



    private void UnfreezeLightFireTutorial()
    {
        // Stop time
        RescaleTime(0f);

        UnfreezeBacteriaCell(false);

        // Explain cells difference
        animator.SetTrigger("FadeInHumanCellText");

        // Move cursor to bacteria position
        SetCursorToTargetWorldPosition(bacteriaCellList[0].transform.position);
    }

    private void UnfreezeBacteriaCell(bool mutation)
    {
        foreach(Organism o in bacteriaCellList)
        {
            o.GetComponent<OrganismAttack>().enabled = true;
            o.GetComponent<OrganismMovement>().enabled = true;
            if (mutation) o.GetComponent<OrganismMutation>().enabled = true;
        }
    }

    private void UnfreezeVirus()
    {
        foreach(Organism o in virusList)
        {
            o.GetComponent<OrganismAttack>().enabled = true;
            o.GetComponent<OrganismMovement>().enabled = true;
        }
    }


    private void SetCursorToTargetWorldPosition(Vector2 worldPos)
    {
        cursorRect.position = cameraController.GetCamera().WorldToScreenPoint(worldPos);
    }

    private void ReplaceWalls()
    {
        invisibleWall.transform.position = (Vector2)playerController.transform.position + new Vector2(50f, 0f);
    }

    public bool IsWeaponChangedBlocked()
    {
        return blockWeaponChange;
    }

    public bool IsTimeFreezed()
    {
        return freezeTime;
    }
}

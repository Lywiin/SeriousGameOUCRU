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

    public GameObject textBoxParent;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private CameraController cameraController;

    private List<Organism> bacteriaCellList;
    private List<Virus> virusList;
    private Organism humanCell;

    private bool blockWeaponChange = true;

    private bool freezeTime = false;

    private int textIndex = 0;
    private TextBox currentTextBox;


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
        // MobileUI.Instance.gameObject.SetActive(false);

        virusList = Virus.virusList;

        UIController.Instance.TogglePauseButton(true);
    }


    /***** DIALOG FUNCTIONS *****/

    private void TriggerNextText()
    {
        if (textIndex < textBoxParent.transform.childCount)
        {
            currentTextBox = textBoxParent.transform.GetChild(textIndex).GetComponent<TextBox>();
            currentTextBox.gameObject.SetActive(true);
            textIndex++;
        }
    }


    /***** ANIMATOR FUNCTIONS *****/

    public void StartTutorial()
    {
        StartCoroutine(TutoCoroutine());
    }

    private IEnumerator TutoCoroutine()
    {
        // yield return new WaitForSeconds(0.5f);
        // Welcome text
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);

        // Move text
        TriggerNextText();
        animator.SetTrigger("StartShineScreenOutline");
        gameController.SetCanPlayerMove(true);
        yield return new WaitUntil(() => playerController.GetMoveDirection() != Vector2.zero);

        // Move text fade out
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("StopShineScreenOutline");
        currentTextBox.FadeOut();
        yield return new WaitForSeconds(1f);

        // Bacteria text
        ReplaceWalls();
        SpawnLightFireTutorialCell();
        TriggerNextText();
        RescaleTime(0f);
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);
        
        // Bacteria move text
        RescaleTime(1f);
        TriggerNextText();
        yield return new WaitUntil(() => bacteriaCellList[0] && Vector2.Distance(playerController.transform.position, bacteriaCellList[0].transform.position) < 30f);
        currentTextBox.FadeOut();

        // Human cell text
        RescaleTime(0f);
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);

        // Bacteria cell text
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);
        
        // Bacteria click text
        TriggerNextText();
        animator.SetTrigger("StartGrowCursor");
        SetCursorToTargetWorldPosition(bacteriaCellList[0].transform.position);
        gameController.SetCanPlayerShoot(true);
        yield return new WaitUntil(() => playerController.IsFiring());

        // Player kills bacteria
        RescaleTime(1f);
        UnfreezeBacteriaCell(false);
        animator.SetTrigger("StopGrowCursor");
        currentTextBox.FadeOut();
        yield return new WaitUntil(() => !playerController.IsFiring());
        yield return new WaitForSeconds(1f);

        // Second weapon text
        RescaleTime(0f);
        TriggerNextText();
        UIController.Instance.gameObject.SetActive(true);
        UIController.Instance.ToggleInfoPanel(true);
        UIController.Instance.ToggleInfoPanelCount(false);
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);

        // Weapon change text
        RescaleTime(1f);
        TriggerNextText();
        gameController.SetCanPlayerChangeWeapon(true);
        yield return new WaitUntil(() => playerController.IsHeavyWeaponSelected());
        currentTextBox.FadeOut();

        // Antibiotic text
        RescaleTime(0f);
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);

        // Player kills bacteria group
        RescaleTime(1f);
        TriggerNextText();
        ReplaceWalls();
        SpawnHeavyFireTutorialCell();
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => BacteriaCell.bacteriaCellList.Count == 0);
        currentTextBox.FadeOut();
        yield return new WaitForSeconds(1.5f);

        // Virus text
        blockWeaponChange = false;
        gameController.SetCanPlayerChangeWeapon(true);
        RescaleTime(0f);
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);

        // Setup virus tutorial
        RescaleTime(1f);
        TriggerNextText();
        ReplaceWalls();
        SpawnVirusTutorialCell();
        yield return new WaitForSeconds(0.5f);

        // Wait for player to get close to virus
        yield return new WaitUntil(() => virusList[0] != null && Vector2.Distance(playerController.transform.position, virusList[0].transform.position) < 50f);
        UnfreezeVirus();

        // Player kills virus
        yield return new WaitUntil(() => Virus.virusList.Count == 0);
        gameController.TogglePlayerInput(false);
        currentTextBox.FadeOut();

        // Finish text
        TriggerNextText();
        yield return new WaitUntil(() => !currentTextBox.gameObject.activeSelf);
        LevelChanger.Instance.FadeToLevel(2);
    }

    public void RescaleTime(float value)
    {
        freezeTime = value == 0f;

        if (freezeTime)
            PlayerController.Instance.PauseMotorSound();
        else
            PlayerController.Instance.PlayMotorSound();

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

    private void UnfreezeBacteriaCell(bool mutation)
    {
        foreach(Organism o in bacteriaCellList)
        {
            // o.GetComponent<OrganismAttack>().enabled = true;
            o.GetComponent<OrganismMovement>().enabled = true;
            if (mutation) o.GetComponent<OrganismMutation>().enabled = true;
        }
    }

    private void UnfreezeVirus()
    {
        foreach(Organism o in virusList)
        {
            // o.GetComponent<OrganismAttack>().enabled = true;
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

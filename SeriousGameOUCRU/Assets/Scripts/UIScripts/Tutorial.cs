using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    public RectTransform cursorRect;

    // public RawImage forwardKeyImage;
    // public RawImage leftKeyImage;
    // public Texture zKeyTexture;
    // public Texture qKeyTexture;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;
    private PlayerController playerController;
    private CameraController cameraController;

    private List<Organism> bacteriaCellList;
    private Organism humanCell;

    private bool blockWeaponChange = true;


    // Keep track of what the player did
    private bool playerMoved = false;
    private bool playerMovedCamera = false;
    private bool playerShooted = false;


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

        //TEMPORARY DESACTIVATED
        PlayerPrefs.SetInt("Tutorial", 1);

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

        gameController.BlockPlayerInput();

        // Hide UI since it's useless for now
        // UIController.Instance.ToggleInfoPanel(false);
        UIController.Instance.gameObject.SetActive(false);
        MobileUI.Instance.gameObject.SetActive(false);

        // Change key texture if detect french language
        // if (Application.systemLanguage == SystemLanguage.French) 
        // {
        //     forwardKeyImage.texture = zKeyTexture;
        //     leftKeyImage.texture = qKeyTexture;
        // }
    }

    private void Update()
    {
        // // Trigger when player move the character
        // if (gameController.CanPlayerMove() && !playerMoved && 
        //     (Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d") || Input.GetKeyDown("z") || Input.GetKeyDown("q")))
        // {
        //     // Add delay before next step
        //     StartCoroutine(PlayerMovedCoroutine(0.5f));
        // }

        // // Trigger when player move the camera
        // else if (gameController.CanPlayerMoveCamera() && !playerMovedCamera && (Input.GetAxis("Mouse X") != 0))
        // {
        //     // Add delay before next step
        //     StartCoroutine(PlayerMovedCameraCoroutine(0.5f));
        // }

        // // Trigger when player shoot
        // else if (gameController.CanPlayerShoot() && !playerShooted && (Input.GetButton("Fire1") || Input.GetButton("Fire2")))
        // {
        //     // Add delay before next step
        //     StartCoroutine(PlayerShootedCoroutine(1.0f));
        // }
    }


    /***** COROUTINES FUNCTIONS *****/

    // private IEnumerator PlayerMovedCoroutine(float t)
    // {
    //     yield return new WaitForSeconds(t);
    //     playerMoved = true;
    //     ShowMoveCameraText();
    // }
    // private IEnumerator PlayerMovedCameraCoroutine(float t)
    // {
    //     yield return new WaitForSeconds(t);
    //     playerMovedCamera = true;
    //     ShowShootText();
    // }
    // private IEnumerator PlayerShootedCoroutine(float t)
    // {
    //     yield return new WaitForSeconds(t);
    //     playerShooted = true;
    //     TutorialFinished();
    // }

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

        SpawnHeavyFireTutorialCell();

        // Wait for player to kill all the cells
        yield return new WaitUntil(() => BacteriaCell.bacteriaCellList.Count == 0);
        animator.SetTrigger("FadeInVirusText");
    }

    

    public void RescaleTime(float value)
    {
        Time.timeScale = value;
    }


    /***** CELL SPAWN FUNCTIONS *****/

    private void SpawnLightFireTutorialCell()
    {
        Vector2 spawnPos = playerController.transform.position;
        spawnPos.y = 0f;

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

    private void UnfreezeLightFireTutorial()
    {
        // Stop time
        RescaleTime(0f);

        UnfreezeBacteriaCell(false);

        // Explain cells difference
        animator.SetTrigger("FadeInHumanCellText");

        // Move cursor to bacteria position
        Vector2 screenPos = cameraController.GetCamera().WorldToScreenPoint(bacteriaCellList[0].transform.position);
        cursorRect.anchoredPosition = screenPos;
    }

    private void SpawnHeavyFireTutorialCell()
    {
        Vector2 spawnPos = playerController.transform.position;
        spawnPos.y = 0f;

        spawnPos.x += 100f;
        SpawnTutorialBacteriaCell(spawnPos);

        spawnPos.x += 10f;
        spawnPos.y += 3f;
        SpawnTutorialBacteriaCell(spawnPos);

        spawnPos.x -= 3f;
        spawnPos.y -= 13f;
        SpawnTutorialBacteriaCell(spawnPos);

        UnfreezeBacteriaCell(true);
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

    private void UnfreezeBacteriaCell(bool mutation)
    {
        foreach(Organism o in bacteriaCellList)
        {
            o.GetComponent<OrganismAttack>().enabled = true;
            o.GetComponent<OrganismMovement>().enabled = true;
            if (mutation) o.GetComponent<OrganismMutation>().enabled = true;
        }
    }



    public bool IsWeaponChangedBlocked()
    {
        return blockWeaponChange;
    }

    // private void ShowMoveText()
    // {
    //     // Trigger the first animation for player movement
    //     animator.SetTrigger("showMoveText");
    // }

    // private void ShowMoveCameraText()
    // {
    //     // Trigger the next animation for camera movement
    //     animator.SetTrigger("showMoveCameraText");
    // }

    // private void ShowShootText()
    // {
    //     // Trigger the next animation for shoot input
    //     animator.SetTrigger("showShootText");
    // }

    // private void TutorialFinished()
    // {
    //     // Trigger the end of the tutorial
    //     animator.SetTrigger("tutorialFinished");
    // }


    /***** COMPLETED FUNCTIONS *****/

    // public void OnMoveTextFadeInComplete()
    // {
    //     if (!gameController.CanPlayerMove())
    //     {
    //         gameController.SetCanPlayerMove(true);
    //     }
    // }

    // public void OnMoveCameraTextFadeInComplete()
    // {
    //     if (!gameController.CanPlayerMoveCamera())
    //     {
    //         gameController.SetCanPlayerMoveCamera(true);
    //     }
    // }

    // public void OnShootTextFadeInComplete()
    // {
    //     if (!gameController.CanPlayerShoot())
    //     {
    //         gameController.SetCanPlayerShoot(true);
    //     }
    // }

    // public void HideTutorial()
    // {
    //     // Only trigger when last thing has been done
    //     if (playerShooted)
    //     {
    //         gameObject.SetActive(false);

    //         // Start the game after
    //         gameController.SetupGame();

    //         // Display info UI after
    //         UIController.Instance.ToggleInfoPanel(true);
    //     }
    // }

    // public void ToggleAnimator()
    // {
    //     animator.enabled = !animator.enabled;
    // }

}

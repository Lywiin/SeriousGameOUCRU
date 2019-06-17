using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    public RawImage forwardKeyImage;
    public RawImage leftKeyImage;
    public Texture zKeyTexture;
    public Texture qKeyTexture;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;

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
        PlayerPrefs.SetInt("Tutorial", 0);

        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            PlayerPrefs.SetInt("Tutorial", 1);
        }

        if (!PlayerPrefs.HasKey("IsAzerty"))
        {
            PlayerPrefs.SetInt("IsAzerty", 0);
        }
    }

    private void Start()
    {
        gameController = GameController.Instance;

        // If tutorial on, reset what player can do
        if (PlayerPrefs.GetInt("Tutorial") == 1)
        {
            gameController.BlockPlayerInput();

            // Hide UI since it's useless for now
            UIController.Instance.ToggleInfoPanel(false);
        }else
        {
            // Start the game if tutorial is off
            gameController.SetupGame();
        }

        // Change key texture if detect french language
        if (PlayerPrefs.GetInt("IsAzerty") == 1 || Application.systemLanguage == SystemLanguage.French) 
        {
            forwardKeyImage.texture = zKeyTexture;
            leftKeyImage.texture = qKeyTexture;
        }
    }

    private void Update()
    {
        // Trigger when player move the character
        if (gameController.CanPlayerMove() && !playerMoved && 
            (Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d") || Input.GetKeyDown("z") || Input.GetKeyDown("q")))
        {
            // Add delay before next step
            StartCoroutine(PlayerMovedCoroutine(0.5f));
        }

        // Trigger when player move the camera
        else if (gameController.CanPlayerMoveCamera() && !playerMovedCamera && (Input.GetAxis("Mouse X") != 0))
        {
            // Add delay before next step
            StartCoroutine(PlayerMovedCameraCoroutine(0.5f));
        }

        // Trigger when player shoot
        else if (gameController.CanPlayerShoot() && !playerShooted && (Input.GetButton("Fire1") || Input.GetButton("Fire2")))
        {
            // Add delay before next step
            StartCoroutine(PlayerShootedCoroutine(1.0f));
        }
    }


    /***** COROUTINES FUNCTIONS *****/

    private IEnumerator PlayerMovedCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        playerMoved = true;
        ShowMoveCameraText();
    }
    private IEnumerator PlayerMovedCameraCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        playerMovedCamera = true;
        ShowShootText();
    }
    private IEnumerator PlayerShootedCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        playerShooted = true;
        TutorialFinished();
    }

    /***** FADE IN FUNCTIONS *****/

    public void StartTutorial()
    {
        ShowMoveText();
    }

    private void ShowMoveText()
    {
        // Trigger the first animation for player movement
        animator.SetTrigger("showMoveText");
    }

    private void ShowMoveCameraText()
    {
        // Trigger the next animation for camera movement
        animator.SetTrigger("showMoveCameraText");
    }

    private void ShowShootText()
    {
        // Trigger the next animation for shoot input
        animator.SetTrigger("showShootText");
    }

    private void TutorialFinished()
    {
        // Trigger the end of the tutorial
        animator.SetTrigger("tutorialFinished");
    }


    /***** COMPLETED FUNCTIONS *****/

    public void OnMoveTextFadeInComplete()
    {
        if (!gameController.CanPlayerMove())
        {
            gameController.SetCanPlayerMove(true);
        }
    }

    public void OnMoveCameraTextFadeInComplete()
    {
        if (!gameController.CanPlayerMoveCamera())
        {
            gameController.SetCanPlayerMoveCamera(true);
        }
    }

    public void OnShootTextFadeInComplete()
    {
        if (!gameController.CanPlayerShoot())
        {
            gameController.SetCanPlayerShoot(true);
        }
    }

    public void HideTutorial()
    {
        // Only trigger when last thing has been done
        if (playerShooted)
        {
            gameObject.SetActive(false);

            // Start the game after
            gameController.SetupGame();

            // Display info UI after
            UIController.Instance.ToggleInfoPanel(true);
        }
    }

    public void ToggleAnimator()
    {
        animator.enabled = !animator.enabled;
    }

}

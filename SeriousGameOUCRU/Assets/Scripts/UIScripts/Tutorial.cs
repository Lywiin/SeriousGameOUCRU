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

        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            PlayerPrefs.SetInt("Tutorial", 1);
        }
    }

    private void Start()
    {
        // If tutorial on, reset what player can do
        if (PlayerPrefs.GetInt("Tutorial") == 1)
        {
            GameController.Instance.BlockPlayerInput();
        }

        // Change key texture if detect french language
        if (Application.systemLanguage == SystemLanguage.French)
        {
            forwardKeyImage.texture = zKeyTexture;
            leftKeyImage.texture = qKeyTexture;
        }
    }

    private void Update()
    {
        // Trigger when player move the character
        if (GameController.Instance.CanPlayerMove() && !playerMoved && 
            (Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d") || Input.GetKeyDown("z") || Input.GetKeyDown("q")))
        {
            // Add delay before next step
            StartCoroutine(PlayerMovedCoroutine(0.5f));
        }

        // Trigger when player move the camera
        else if (GameController.Instance.CanPlayerMoveCamera() && !playerMovedCamera && (Input.GetAxis("Mouse X") != 0))
        {
            // Add delay before next step
            StartCoroutine(PlayerMovedCameraCoroutine(0.5f));
        }

        // Trigger when player shoot
        else if (GameController.Instance.CanPlayerShoot() && !playerShooted && (Input.GetButton("Fire1") || Input.GetButton("Fire2")))
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
        if (!GameController.Instance.CanPlayerMove())
        {
            GameController.Instance.SetCanPlayerMove(true);
        }
    }

    public void OnMoveCameraTextFadeInComplete()
    {
        if (!GameController.Instance.CanPlayerMoveCamera())
        {
            GameController.Instance.SetCanPlayerMoveCamera(true);
        }
    }

    public void OnShootTextFadeInComplete()
    {
        if (!GameController.Instance.CanPlayerShoot())
        {
            GameController.Instance.SetCanPlayerShoot(true);
        }
    }

    public void HideTutorial()
    {
        // Only trigger when last thing has been done
        if (playerShooted)
            gameObject.SetActive(false);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    public bool on = true;


    /*** PRIVATE VARIABLES ***/

    // Control what can the player do
    private bool canPlayerMove = true;
    private bool canPlayerMoveCamera = true;
    private bool canPlayerShoot = true;

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

        // If tutorial on, reset what player can do
        if (on)
        {
            canPlayerMove = canPlayerMoveCamera = canPlayerShoot = false;
        }
    }

    private void Update()
    {
        // Trigger when player move the character
        if (canPlayerMove && !playerMoved && 
            (Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d") || Input.GetKeyDown("z") || Input.GetKeyDown("q")))
        {
            playerMoved = true;
            ShowMoveCameraText();
        }

        // Trigger when player move the camera
        else if (canPlayerMoveCamera && !playerMovedCamera && (Input.GetAxis("Mouse X") != 0))
        {
            playerMovedCamera = true;
            ShowShootText();
        }

        // Trigger when player shoot
        else if (canPlayerShoot && !playerShooted && (Input.GetButton("Fire1") || Input.GetButton("Fire2")))
        {
            playerShooted = true;
            TutorialFinished();
        }
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


    /***** FADE IN COMPLETED FUNCTIONS *****/

    public void OnMoveTextFadeInComplete()
    {
        if (!canPlayerMove)
        {
            canPlayerMove = true;
        }
    }

    public void OnMoveCameraTextFadeInComplete()
    {
        if (!canPlayerMoveCamera)
        {
            canPlayerMoveCamera = true;
        }
    }

    public void OnShootTextFadeInComplete()
    {
        if (!canPlayerShoot)
        {
            canPlayerShoot = true;
        }
    }


    /***** FADE IN COMPLETED FUNCTIONS *****/

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
}

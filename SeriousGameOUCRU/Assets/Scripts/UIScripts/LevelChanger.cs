﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /*** PRIVATE VARIABLES ***/

    private int levelToLoad;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Start()
    {
        animator.ResetTrigger("FadeOut");
    }

    /***** FADE FUNCTIONS *****/

    public void FadeToLevel(int index)
    {
        levelToLoad = index;
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeOutComplete()
    {
        // Load the level
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnFadeInComplete()
    {
        // Unpause the game to start
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            //GameController.Instance.TogglePause();
            PlayerController.Instance.SetCanMove(true);
        }
    }
}
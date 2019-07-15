﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /*** PRIVATE VARIABLES ***/

    private int levelToLoad;

    /*** INSTANCE ***/

    private static LevelChanger _instance;
    public static LevelChanger Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    private void Start()
    {
        animator.ResetTrigger("FadeOut");

        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            GameController.Instance.SetupGame();
        }
    }

    /***** FADE FUNCTIONS *****/

    public void FadeToLevel(int index)
    {
        animator.enabled = true;
        levelToLoad = index;
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeOutComplete()
    {
        Time.timeScale = 1.0f;
        // Load the level
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnFadeInComplete()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.enabled = false;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (PlayerPrefs.GetInt("CurrentLevel") < sceneIndex)
            PlayerPrefs.SetInt("CurrentLevel", sceneIndex);

        // Unpause the game to start
        if (sceneIndex == 1)
        {
            Tutorial.Instance.StartTutorial();
        }
    }
}

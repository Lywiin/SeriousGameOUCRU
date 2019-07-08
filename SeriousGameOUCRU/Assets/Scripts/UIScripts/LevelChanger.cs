using UnityEngine;
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
        // Load the level
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnFadeInComplete()
    {
        // Unpause the game to start
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            Tutorial.Instance.StartTutorial();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelChanger : MonoBehaviour
{
    /*** PRIVATE VARIABLES ***/

    private AudioManager audioManager;
    private Animator animator;
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

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;

        if(audioManager)
        {
            if (SceneManager.GetActiveScene().buildIndex != 0)
                audioManager.SmoothPlay("GameTheme", 0.5f);
            else
                audioManager.SmoothPlay("MenuTheme", 0.5f);
        }
    }
    

    /***** FADE FUNCTIONS *****/

    public void FadeToLevel(int index)
    {
        animator.enabled = true;
        levelToLoad = index;
        animator.SetTrigger("FadeOut");

        if (audioManager)
        {
            audioManager.Play("FadeScreen");

            // Stop musics
            if (SceneManager.GetActiveScene().buildIndex == 0) 
                audioManager.SmoothStop("MenuTheme", 0.5f);
            else
                audioManager.SmoothStop("GameTheme", 0.5f);

            // Make sure motor are stopped
            if (PlayerController.Instance) PlayerController.Instance.StopMotorSound();
        }

        if (UIController.Instance)
        {
            UIController.Instance.SetPauseButtonInteractible(false);
        }
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

        // Unpause the game to start
        if (sceneIndex == 1)
        {
            Tutorial.Instance.StartTutorial();
        }

        if (UIController.Instance)
        {
            UIController.Instance.SetPauseButtonInteractible(true);
        }
    }
}

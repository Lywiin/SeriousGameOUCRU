using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    /*** PRIVATE VARIABLES ***/

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
        if (SceneManager.GetActiveScene().buildIndex != 0)
            AudioManager.Instance.PlayGameTheme();
        else
            AudioManager.Instance.SmoothPlay("MenuTheme", 1f);
    }
    

    /***** FADE FUNCTIONS *****/

    public void FadeToLevel(int index)
    {
        animator.enabled = true;
        levelToLoad = index;
        animator.SetTrigger("FadeOut");

        // Transition from menu to game music
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            AudioManager.Instance.Play("StartGame");
            AudioManager.Instance.SmoothStop("MenuTheme", 1f);
        }

        // Transition from game to menu music
        if (index == 0)
        {
            AudioManager.Instance.SmoothStop("GameTheme", 1f);
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

        if (PlayerPrefs.GetInt("CurrentLevel") < sceneIndex)
            PlayerPrefs.SetInt("CurrentLevel", sceneIndex);

        // Unpause the game to start
        if (sceneIndex == 1)
        {
            Tutorial.Instance.StartTutorial();
        }
    }
}

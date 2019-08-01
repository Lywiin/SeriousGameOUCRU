using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI humanCellCountText;
    public TextMeshProUGUI bacteriaCellCountText;
    public TextMeshProUGUI virusCountText;
    public Slider resistanceSlider;

    [Header("End Panel")]
    public GameObject endGamePanel;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI timeTextValue;
    public TextMeshProUGUI bacteriaKilledCountTextValue;
    public TextMeshProUGUI virusKilledCountTextValue;
    public GameObject restartButton;
    public GameObject nextLevelButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public GameObject pauseButton;


    /*** PRIVATE VARIABLES ***/

    private Animator animator;
    private float tempTime = 0f;

    private int previousBacteriaCount = 0;
    private int previousVirusCount = 0;
    private int previousCellCount = 0;


    /*** INSTANCE ***/

    private static UIController _instance;
    public static UIController Instance { get { return _instance; } }


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

    void Start()
    {
        // Make sure this panel is unactive
        endGamePanel.SetActive(false);

        // Init time survived
        tempTime = Time.time;
    }


    /***** DISPLAY FUNCTIONS *****/

    public void TriggerVictory()
    {
        DisplayEndGamePanel();

        // Desactivate useless things
        gameOverText.gameObject.SetActive(false);
        restartButton.SetActive(false);
    }

    public void TriggerGameOver()
    {
        DisplayEndGamePanel();

        // Desactivate useless things
        victoryText.gameObject.SetActive(false);
        nextLevelButton.SetActive(false);
    }

    private void DisplayEndGamePanel()
    {
        //Hide Mobile UI
        // MobileUI.Instance.gameObject.SetActive(false);

        // Calculate time spent and update text
        int minutes = (int)(Time.time - tempTime) / 60;
        int seconds = (int)(Time.time - tempTime) % 60;
        timeTextValue.text = minutes.ToString() + "m " + seconds.ToString() + "s";

        // Update killed count text
        bacteriaKilledCountTextValue.text = GameController.Instance.GetBacteriaCellKillCount().ToString();
        virusKilledCountTextValue.text = GameController.Instance.GetVirusKillCount().ToString();

        pauseButton.SetActive(false);
        ToggleInfoPanel(false);
        TogglePauseButton(false);
        GameController.Instance.TogglePlayerInput(false);

        // Display panel
        // endGamePanel.SetActive(true);
        animator.SetTrigger("FadeInEndGamePanel");
    }

    public void TogglePauseUI()
    {
        if (AudioManager.Instance) AudioManager.Instance.Play("Select1");

        GameController.Instance.TogglePause();
        bool isPaused = GameController.Instance.IsGamePaused();

        animator.enabled = !isPaused;
        infoPanel.SetActive(!isPaused);
        CloseEnnemyUI.Instance.gameObject.SetActive(!isPaused);
        if (Tutorial.Instance) Tutorial.Instance.GetComponentInChildren<CanvasGroup>().alpha = isPaused ? 0 : 1;

        pausePanel.SetActive(isPaused);
    }

    public void ToggleInfoPanel(bool b)
    {
        if (b)
        {
            animator.SetTrigger("FadeInInfoPanel");
            animator.ResetTrigger("FadeOutInfoPanel");
        }else
        {
            animator.SetTrigger("FadeOutInfoPanel");
            animator.ResetTrigger("FadeInInfoPanel");
        }
    }

    public void TogglePauseButton(bool b)
    {
        if (b)
            animator.SetTrigger("FadeInPauseButton");
        else
            animator.SetTrigger("FadeOutPauseButton");
    }

    // Hide everything except bacteria resistance
    public void ToggleInfoPanelCount(bool b)
    {
        infoPanel.transform.GetChild(1).gameObject.SetActive(b);
        infoPanel.transform.GetChild(2).gameObject.SetActive(b);
        infoPanel.transform.GetChild(3).gameObject.SetActive(b);
    }

    // Grow bacteria count text if count has increased
    public void UpdateBacteriaCellCount()
    {
        int currentBacteriaCount = BacteriaCell.bacteriaCellList.Count;
        bacteriaCellCountText.text = currentBacteriaCount.ToString();

        if (currentBacteriaCount > previousBacteriaCount)
            animator.SetTrigger("GrowBacteriaCount");

        previousBacteriaCount = currentBacteriaCount;
    }

    public void UpdateHumanCellCount()
    {
        int currentCellCount = HumanCell.humanCellList.Count;
        humanCellCountText.text = currentCellCount.ToString();

        if (currentCellCount < previousCellCount)
            animator.SetTrigger("ShrinkCellCount");

        previousCellCount = currentCellCount;
    }

    public void UpdateVirusCount()
    {

        int currentVirusCount = Virus.virusList.Count;
        virusCountText.text = currentVirusCount.ToString();

        if (currentVirusCount > previousVirusCount)
            animator.SetTrigger("GrowVirusCount");

        previousVirusCount = currentVirusCount;
    }

    public void UpdateGlobalMutationProba()
    {
        resistanceSlider.value = OrganismMutation.mutationProba;
        animator.SetTrigger("ShineResistanceSlider");
    }


    /***** LEVEL CHANGE FUNCTIONS *****/

    public void ChangeLevel(int index)
    {
        LevelChanger.Instance.FadeToLevel(index);
    }

    public void RestartLevel()
    {
        ChangeLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        ChangeLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

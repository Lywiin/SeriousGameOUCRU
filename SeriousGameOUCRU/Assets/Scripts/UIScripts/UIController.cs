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
    public TextMeshProUGUI killedCountTextValue;
    public Toggle tutorialToggle;

    [Header("Pause Panel")]
    public GameObject pausePanel;


    /*** PRIVATE VARIABLES ***/

    private Animator animator;
    private float tempTime = 0f;


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

        // Initialize toggle status
        tutorialToggle.isOn = PlayerPrefs.GetInt("Tutorial") == 0 ? false : true;

        // Init time survived
        tempTime = Time.time;
    }

    void Update()
    {
        // Update cell count text
        bacteriaCellCountText.text = BacteriaCell.bacteriaCellList.Count.ToString();
        humanCellCountText.text = HumanCell.humanCellList.Count.ToString();
        virusCountText.text = Virus.virusList.Count.ToString();

        // Update global mutation probability slider
        resistanceSlider.value = OrganismMutation.mutationProba;
    }


    /***** DISPLAY FUNCTIONS *****/

    public void TriggerVictory()
    {
        DisplayEndGamePanel();

        // Desactivate useless text
        gameOverText.gameObject.SetActive(false);

        // Block player inputs
        GameController.Instance.TogglePlayerInput(false);
    }

    public void TriggerGameOver()
    {
        DisplayEndGamePanel();

        // Desactivate useless text
        victoryText.gameObject.SetActive(false);

        // Hide info panel
        ToggleInfoPanel(false);
    }

    private void DisplayEndGamePanel()
    {
        //Hide Mobile UI
        MobileUI.Instance.gameObject.SetActive(false);

        // Calculate time spent and update text
        int minutes = (int)(Time.time - tempTime) / 60;
        int seconds = (int)(Time.time - tempTime) % 60;
        timeTextValue.text = minutes.ToString() + "m " + seconds.ToString() + "s";

        // Update killed count text
        killedCountTextValue.text = GameController.Instance.GetBacteriaCellKillCount().ToString();

        // Display panel
        endGamePanel.SetActive(true);
    }

    public void TogglePauseUI()
    {
        GameController.Instance.TogglePause();
        bool isPaused = GameController.Instance.IsGamePaused();

        animator.enabled = !isPaused;
        infoPanel.SetActive(!isPaused);
        MobileUI.Instance.gameObject.SetActive(!isPaused);
        CloseEnnemyUI.Instance.gameObject.SetActive(!isPaused);

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

    // Hide everything except bacteria resistance
    public void ToggleInfoPanelCount(bool b)
    {
        infoPanel.transform.GetChild(1).gameObject.SetActive(b);
        infoPanel.transform.GetChild(2).gameObject.SetActive(b);
        infoPanel.transform.GetChild(3).gameObject.SetActive(b);
    }

    // public void UpdateBacteriaCellCount(int count)
    // {
    //     bacteriaCellCountText.text = count.ToString();
    // }

    // public void UpdateHumanCellCount(int count)
    // {
    //     humanCellCountText.text = count.ToString();
    // }

    // public void UpdateGlobalMutationProba(float proba)
    // {
    //     resistanceSlider.value = proba;
    // }


    /***** ON EVENT FUNCTIONS *****/

    public void OnTutorialValueChanged()
    {
        // Change tutorial value in the preferences when player click on toggle
        int newValue = tutorialToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("Tutorial", newValue);
    }


    /***** LEVEL CHANGE FUNCTIONS *****/

    public void FadeToHome()
    {
        LevelChanger.Instance.FadeToLevel(0);
    }

    public void RestartLevel()
    {
        LevelChanger.Instance.FadeToLevel(SceneManager.GetActiveScene().buildIndex);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Info Panel")]
    public TextMeshProUGUI badBacteriaCountText;
    public TextMeshProUGUI goodBacteriaCountText;
    public TextMeshProUGUI mutationProbaText;

    [Header("End Panel")]
    public GameObject endGamePanel;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI timeTextValue;
    public TextMeshProUGUI killedCountTextValue;
    public Toggle tutorialToggle;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        gameController = GameController.Instance;

        // Make sure this panel is unactive
        endGamePanel.SetActive(false);

        // Initialize toggle status
        tutorialToggle.isOn = PlayerPrefs.GetInt("Tutorial") == 0 ? false : true;
    }

    void Update()
    {
        // Update bacteria count text
        badBacteriaCountText.text = "Bad bacteria: " + BadBacteria.badBacteriaList.Count.ToString();
        goodBacteriaCountText.text = "Good bacteria: " + GoodBacteria.goodBacteriaList.Count.ToString();

        // Update global mutation probability text
        float globalProba = Mathf.Round(gameController.GetGlobalMutationGlobalProba() * 1000f) / 1000f;
        mutationProbaText.text = "Mutation probability: " + globalProba.ToString();
    }


    /***** DISPLAY FUNCTIONS *****/

    public void TriggerVictory()
    {
        DisplayEndGamePanel();

        // Desactivate useless text
        gameOverText.gameObject.SetActive(false);

        // Block player inputs
        GameController.Instance.BlockPlayerInput();
    }

    public void TriggerGameOver()
    {
        DisplayEndGamePanel();

        // Desactivate useless text
        victoryText.gameObject.SetActive(false);
    }

    private void DisplayEndGamePanel()
    {
        // Calculate time spent and update text
        int minutes = (int)Time.time / 60;
        int seconds = (int)Time.time % 60;
        timeTextValue.text = minutes.ToString() + "m " + seconds.ToString() + "s";

        // Update killed count text
        killedCountTextValue.text = GameController.Instance.GetBadBacteriaKillCount().ToString();

        // Display panel
        endGamePanel.SetActive(true);
    }


    /***** ON EVENT FUNCTIONS *****/

    public void OnTutorialValueChanged()
    {
        // Change tutorial value in the preferences when player click on toggle
        int newValue = tutorialToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("Tutorial", newValue);
    }
}

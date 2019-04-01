using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Info Panel")]
    public TextMeshProUGUI badBacteriaCountText;
    public TextMeshProUGUI goodBacteriaCountText;
    public TextMeshProUGUI mutationProbaText;

    [Header("Finish Panel")]
    public GameObject finishPanel;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI gameOverText;


    /*** PRIVATE VARIABLES ***/

    private GameController gameController;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        gameController = GameController.Instance;
    }

    void Update()
    {
        // Update bacteria count text
        badBacteriaCountText.text = "Bad bacteria: " + gameController.GetCurrentBadBacteriaCount().ToString();
        goodBacteriaCountText.text = "Good bacteria: " + gameController.GetCurrentGoodBacteriaCount().ToString();

        // Update global mutation probability text
        float globalProba = Mathf.Round(gameController.GetGlobalMutationGlobalProba() * 1000f) / 1000f;
        mutationProbaText.text = "Mutation probability: " + globalProba.ToString();
    }


    /***** DISPLAY FUNCTIONS *****/

    public void DisplayVictory()
    {
        finishPanel.SetActive(true);
        victoryText.gameObject.SetActive(true);
    }

    public void DisplayGameOver()
    {
        finishPanel.SetActive(true);
        gameOverText.gameObject.SetActive(true);
    }
}

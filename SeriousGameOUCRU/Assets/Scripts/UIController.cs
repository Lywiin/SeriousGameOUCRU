using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Info Panel")]
    public TextMeshProUGUI badBacteriaCountText;
    public TextMeshProUGUI goodBacteriaCountText;
    public TextMeshProUGUI mutationProbaText;

    [Header("Finish Panel")]
    public GameObject finishPanel;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI gameOverText;

    private GameController gameController;

    void Start()
    {
        gameController = GameController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        badBacteriaCountText.text = "Bad bacteria: " + gameController.GetCurrentBadBacteriaCount().ToString();
        goodBacteriaCountText.text = "Good bacteria: " + gameController.GetCurrentGoodBacteriaCount().ToString();

        float globalProba = Mathf.Round(gameController.GetGlobalMutationGlobalProba() * 1000f) / 1000f;
        mutationProbaText.text = "Mutation probability: " + globalProba.ToString();
    }

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

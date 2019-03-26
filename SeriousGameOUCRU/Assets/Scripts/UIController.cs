using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public GameController gameController;
    public TextMeshProUGUI badBacteriaCountText;
    public TextMeshProUGUI goodBacteriaCountText;
    public TextMeshProUGUI mutationProbaText;

    // Update is called once per frame
    void Update()
    {
        badBacteriaCountText.text = "Bad bacteria: " + gameController.GetCurrentBadBacteriaCount().ToString();
        goodBacteriaCountText.text = "Good bacteria: " + gameController.GetCurrentGoodBacteriaCount().ToString();

        float globalProba = Mathf.Round(gameController.GetGlobalMutationGlobalProba() * 1000f) / 1000f;
        mutationProbaText.text = "Mutation probability: " + globalProba.ToString();
    }
}

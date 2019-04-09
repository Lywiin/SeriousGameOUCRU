using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /*** INSTANCE ***/

    private static Tutorial _instance;
    public static Tutorial Instance { get { return _instance; } }


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


    /***** ANIMATION FUNCTIONS *****/

    public void DisplayTutorial()
    {
        ShowMoveText();
        StartCoroutine(ShowMoveCameraText());
    }

    private void ShowMoveText()
    {
        animator.SetBool("showMoveText", true);
    }

    private IEnumerator ShowMoveCameraText()
    {
        yield return new WaitForSeconds(3f);
        animator.SetBool("showMoveCameraText", true);
    }
}

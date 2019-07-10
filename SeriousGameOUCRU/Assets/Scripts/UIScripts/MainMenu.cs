using UnityEngine;

public class MainMenu : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /***** SIZE FUNCTIONS *****/

    public void OnPlayClick()
    {
        animator.SetTrigger("FadeToLevelScreen");
        // animator.SetBool("CanFade", false);
    }

    public void CanFadeTrue()
    {
        animator.SetBool("CanFade", true);
    }

    public void CanFadeFalse()
    {
        animator.SetBool("CanFade", false);
    }

    public void ResetFadeTriggers()
    {
        animator.ResetTrigger("FadeToHomeScreen");
        animator.ResetTrigger("FadeToLevelScreen");
        animator.ResetTrigger("FadeToOptionScreen");
    }
}

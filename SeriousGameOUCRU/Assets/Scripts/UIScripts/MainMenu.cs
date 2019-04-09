using UnityEngine;

public class MainMenu : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /***** SIZE FUNCTIONS *****/

    public void GrowText()
    {
        animator.SetBool("grow", true);
    }

    public void ShrinkText()
    {
        animator.SetBool("grow", false);
    }
}

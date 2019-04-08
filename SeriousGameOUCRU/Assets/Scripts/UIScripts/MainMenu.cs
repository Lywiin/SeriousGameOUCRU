using UnityEngine;

public class MainMenu : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        
    }


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

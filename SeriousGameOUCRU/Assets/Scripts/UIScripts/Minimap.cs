using UnityEngine;

public class Minimap : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;


    /*** INSTANCE ***/

    private static Minimap _instance;
    public static Minimap Instance { get { return _instance; } }


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


    /***** FADE FUNCTIONS *****/

    public void ShowMinimap()
    {
        animator.SetBool("showMinimap", true);
    }

    public void HideMinimap()
    {
        animator.SetBool("showMinimap", false);
    }
}

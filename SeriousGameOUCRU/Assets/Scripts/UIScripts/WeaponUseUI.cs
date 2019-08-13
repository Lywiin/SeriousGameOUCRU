using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUseUI : MonoBehaviour
{
    /*** PRIVATE VARIABLES ***/

    private Animator animator;


    /*** INSTANCE ***/

    private static WeaponUseUI _instance;
    public static WeaponUseUI Instance { get { return _instance; } }


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

    
    /***** DISPLAY FUNCTIONS *****/

    public void DisplayWeaponUsePanel()
    {
        animator.SetTrigger("FadeInWeaponUsePanel");
    }

    public void HideWeaponUsePanel()
    {
        animator.SetTrigger("FadeOutWeaponUsePanel");
    }
}
